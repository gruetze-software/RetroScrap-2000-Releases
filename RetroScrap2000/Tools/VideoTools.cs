using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace RetroScrap2000.Tools
{
	public static class VideoTools
	{
		private static readonly HttpClient _http = new HttpClient();
		// URL+Size → Overlay-Image Cache
		private static readonly ConcurrentDictionary<string, Image> _urlVideoOverlayCache = new();
		// URL → Temp-Videopfad (damit nicht jedes Mal neu geladen wird)
		private static readonly ConcurrentDictionary<string, string> _tempVideoByUrl = new();
		/// <summary>
		/// Lädt ein Video (URL) einmalig in ein App-Temp-Verzeichnis, erzeugt bei Bedarf ein Preview-JPG
		/// und liefert ein Play-Overlay-Image + absoluten Pfad zur Temp-Videodatei.
		/// </summary>
		public static async Task<(Image? overlay, string videoAbsPath, string? videoPreviewAbsPath)?> 
			LoadVideoPreviewFromUrlAsync(
				string? videoUrl, bool withPreview, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(videoUrl))
				return null;

			// 1) Stabiler Temp-Pfad für diese URL
			var (tempVideo, tempPreview) = GetTempVideoAndPreviewPaths(videoUrl);

			// 2) Video ggf. herunterladen
			if (!System.IO.File.Exists(tempVideo))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(tempVideo)!);
				string urllogstring = videoUrl.Substring(videoUrl.IndexOf("?") + 1) + "xxxxxx";
				Log.Information($"VideoTools::LoadVideoPreviewFromUrlAsync(\"{urllogstring}\")");
				var bytes = await _http.GetByteArrayAsync(videoUrl, ct);
				Log.Debug(videoUrl);
				await System.IO.File.WriteAllBytesAsync(tempVideo, bytes, ct);
			}

			// 3) Preview erzeugen (wenn gewollt und es fehlt)
			if (!System.IO.File.Exists(tempPreview) && withPreview)
			{
				await ExtractPreviewImage(tempVideo, tempPreview); // deine FFmpeg-Methode (async)
			}

			ct.ThrowIfCancellationRequested();

			// 4) Preview laden & skalieren (nutze deinen lokalen Loader)
			if (withPreview)
			{
				var img = await ImageTools.LoadAbsoluteImageCachedAsync(tempPreview, ct);
				if (img == null) return null;

				// 5) Overlay aus Cache oder neu bauen
				var overlayKey = $"urlvid-ovl|{tempPreview}";
				if (!_urlVideoOverlayCache.TryGetValue(overlayKey, out var overlay))
				{
					overlay = ImageTools.DrawPlayOverlay((Bitmap)img);
					_urlVideoOverlayCache[overlayKey] = overlay;
				}
				// 6) Pfad zur Videodatei zurück – gut für „Klick → Player“
				return (overlay, tempVideo, tempPreview);
			}

			return (null, tempVideo, null);
		}

		public static (string video, string preview) GetTempVideoAndPreviewPaths(string file)
		{
			var hash = HashTools.GetHashCodeFile(file);
			var tempBase = FileTools.GetTempPath();
			var video = Path.Combine(tempBase, $"{hash}.mp4");          // Endung egal, nur konsistent
			var preview = Path.Combine(tempBase, $"{hash}_preview.jpg");
			_tempVideoByUrl.TryAdd(file, video);
			return (video, preview);
		}

		/// <summary>
		/// Räumt temporäre Downloads & Overlays auf (z. B. beim App-Exit).
		/// </summary>
		public static void ClearTempScrapeMedia(bool deleteFiles = false)
		{
			foreach (var kv in _urlVideoOverlayCache) { try { kv.Value.Dispose(); } catch { } }
			_urlVideoOverlayCache.Clear();

			if (deleteFiles)
			{
				var dir = Path.Combine(Path.GetTempPath(), "RetroScrap2000", "scrape_media");
				try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
			}

			_tempVideoByUrl.Clear();
		}

		public static async Task ExtractPreviewImage(string videoFile, string outputfile)
		{
			if (!System.IO.File.Exists(videoFile))
				return;

			if (System.IO.File.Exists(outputfile))
				return;

			var ffmpeg = FindFfmpegPath();
			var made = await ExtractPreviewImageAsync(ffmpeg, videoFile, outputfile);
		}

		public static async Task<string?> ExtractPreviewImageAsync(
				string? ffmpegExe, string videoFile, string outputFile, int timeoutMs = 30000)
		{
			if (!System.IO.File.Exists(ffmpegExe)) 
				throw new FileNotFoundException("ffmpeg.exe not found", ffmpegExe);
			if (!System.IO.File.Exists(videoFile)) 
				throw new FileNotFoundException($"videoFile not found", videoFile);

			// Zielordner sicherstellen
			var outDir = Path.GetDirectoryName(outputFile)!;
			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			// Mehr Output und kein Warten auf STDIN
			var args = $"-nostdin -y -hide_banner -loglevel info -ss 00:00:01 -i \"{videoFile}\" -frames:v 1 -update 1 \"{outputFile}\"";

			var psi = new ProcessStartInfo
			{
				FileName = ffmpegExe,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				// WorkingDirectory ist optional; kann auch weggelassen werden
				WorkingDirectory = Path.GetDirectoryName(ffmpegExe)!,
			};

			using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

			proc.OutputDataReceived += (_, e) => { if (e.Data != null) Log.Debug("[FFMPEG-OUT] " + e.Data); };
			proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Log.Debug("[FFMPEG-OUT] " + e.Data); };

			if (!proc.Start())
				throw new InvalidOperationException("FFmpeg could not started.");

			// Streams sofort asynchron lesen
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();

			using var cts = new CancellationTokenSource(timeoutMs);
			try
			{
				await proc.WaitForExitAsync(cts.Token);
			}
			catch (OperationCanceledException)
			{
				try { proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
				throw new TimeoutException($"FFmpeg Timeout ({timeoutMs} ms). Args: {args}");
			}

			// Flush sicherstellen
			proc.WaitForExit();

			if (proc.ExitCode != 0)
				throw new Exception($"FFmpeg ExitCode {proc.ExitCode}");

			return System.IO.File.Exists(outputFile) ? outputFile : null;
		}

		public static string? FindFfmpegPath(string? userPathInSettings = null)
		{
			var candidates = new List<string>();
			if (!string.IsNullOrWhiteSpace(userPathInSettings))
				candidates.Add(userPathInSettings);
			candidates.Add(Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"));
			candidates.Add(Path.Combine(AppContext.BaseDirectory, "tools", "ffmpeg", "win-x64", "ffmpeg.exe"));

			foreach (var p in candidates.Where(p => !string.IsNullOrWhiteSpace(p)))
				if (System.IO.File.Exists(p)) return p;

			// PATH prüfen
			var envPath = Environment.GetEnvironmentVariable("PATH") ?? "";
			foreach (var dir in envPath.Split(Path.PathSeparator))
			{
				try
				{
					var p = Path.Combine(dir, "ffmpeg.exe");
					if (System.IO.File.Exists(p)) return p;
				}
				catch { /* ignore */ }
			}
			return null;
		}
	}
}
