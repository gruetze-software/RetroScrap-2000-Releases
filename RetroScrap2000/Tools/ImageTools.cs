using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace RetroScrap2000.Tools
{
	public static class ImageTools
	{
		private static readonly HttpClient _http = new HttpClient();
		private static readonly ConcurrentDictionary<string, Image> _urlImageCache = new();
		private static readonly ConcurrentDictionary<string, Image> _localImageCache = new();

		/// <summary>
		/// Lädt ein Bild von einer URL, cached es und liefert ein Image.
		/// </summary>
		public static async Task<Image?> LoadImageFromUrlCachedAsync(string? url, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;

			var key = url;
			if (_urlImageCache.TryGetValue(key, out var cached))
				return cached;

			var bytes = await _http.GetByteArrayAsync(url, ct);
			using var ms = new MemoryStream(bytes);
			// WICHTIG: vom Stream entkoppeln → Bitmap-Kopie erstellen
			// (sonst bleibt eine Abhängigkeit zum Stream und/oder wird disposed)
			using var tmp = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false);
			var copy = new Bitmap(tmp); // echte Kopie im Speicher

			_urlImageCache[key] = copy;
			return copy;
		}

		/// <summary>Leert den URL-Bildcache und disposed die Images.</summary>
		public static void ClearUrlImageCache()
		{
			foreach (var kv in _urlImageCache)
			{
				try { kv.Value.Dispose(); } catch { /* ignore */ }
			}
			_urlImageCache.Clear();
		}

		/// <summary>
		/// Lädt ein Bild von einem (relativen oder absoluten) Pfad,
		/// und cached es.
		/// baseDir + relPath werden mit ResolveMediaPath kombiniert.
		/// </summary>
		public static async Task<Image?> LoadImageCachedAsync(
				string baseDir, string? relOrAbsPath, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(relOrAbsPath))
				return null;

			// absolut machen (unterstützt "./media/..." usw.)
			var abs = FileTools.ResolveMediaPath(baseDir, relOrAbsPath);
			if (string.IsNullOrEmpty(abs) )
			{
				Trace.WriteLine($"[Tools.LoadImageCachedAsync]: ResolveMediaPath() from {relOrAbsPath} not success!");
				return null;
			}
			else if ( !System.IO.File.Exists(abs))
			{
				Trace.WriteLine($"[Tools.LoadImageCachedAsync]: File not found: {abs}");
				return null;
			}

			return await LoadAbsoluteImageCachedAsync(abs, ct);
		}

		/// <summary>
		/// Lädt ein Bild von einem absoluten Pfad, und cached es.
		/// </summary>
		public static async Task<Image?> LoadAbsoluteImageCachedAsync(
				string absolutePath, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(absolutePath) || !System.IO.File.Exists(absolutePath))
				return null;

			var key = $"{absolutePath}";
			if (_localImageCache.TryGetValue(key, out var cached))
				return cached;

			// I/O im Hintergrund
			return await Task.Run(() =>
			{
				ct.ThrowIfCancellationRequested();

				using var raw = LoadBitmapNoLock(absolutePath);
				if (raw == null) return null;

				var w = Math.Max(1, raw.Width);
				var h = Math.Max(1, raw.Height);

				var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
				using var g = Graphics.FromImage(bmp);
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.Clear(Color.Transparent);

				var s = Math.Min((float)w / raw.Width, (float)h / raw.Height);
				var nw = (int)(raw.Width * s);
				var nh = (int)(raw.Height * s);
				var x = (w - nw) / 2;
				var y = (h - nh) / 2;

				g.DrawImage(raw, new Rectangle(x, y, nw, nh));
				_localImageCache[key] = bmp;
				return bmp;
			}, ct);
		}


		public static async Task<(Image overlay, string videoAbsPath)?> LoadVideoPreviewAsync(string baseDir,
			GameEntry rom, CancellationToken ct)
		{
			if (string.IsNullOrEmpty(rom.MediaVideoPath))
				return null;

			var videoAbs = FileTools.ResolveMediaPath(baseDir, rom.MediaVideoPath);
			if (!System.IO.File.Exists(videoAbs))
				return null;

			var previewAbs = FileTools.ResolveMediaPath(baseDir, rom.MediaVideoPreviewImagePath);
			if (!string.IsNullOrEmpty(previewAbs) && !System.IO.File.Exists(previewAbs))
			{
				// erzeugen (dein ffmpeg‑Wrapper ist bereits async)
				await VideoTools.ExtractPreviewImage(videoAbs, previewAbs);
			}

			ct.ThrowIfCancellationRequested();

			var img = await LoadImageCachedAsync(baseDir, rom.MediaVideoPreviewImagePath, ct);
			if (img == null)
				return null;

			// Play‑Overlay einmalig erzeugen & cachen
			var cacheKey = $"overlay|{previewAbs}";
			if (!_localImageCache.TryGetValue(cacheKey, out var overlay))
			{
				overlay = DrawPlayOverlay((Bitmap)img);
				_localImageCache[cacheKey] = overlay;
			}

			return (overlay, videoAbs);
		}

		/// <summary>Leert den lokalen Datei-Image-Cache und disposed die Images.</summary>
		public static void ClearLocalImageCache()
		{
			foreach (var kv in _localImageCache)
			{
				try { kv.Value.Dispose(); } catch { /* ignore */ }
			}
			_localImageCache.Clear();
		}

		public static Image? LoadBitmapNoLock(string? absolutePath)
		{
			if (string.IsNullOrWhiteSpace(absolutePath) || !System.IO.File.Exists(absolutePath))
				return null;

			Trace.WriteLine("LoadBitmapNoLock(" + absolutePath + ")");
			try
			{
				using var fs = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var tmp = Image.FromStream(fs);      // lädt ins GDI-Objekt
				return new Bitmap(tmp);                    // kopie -> Datei bleibt frei
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
				return null;
			}
		}

		public static Image? GetSystemImage(string? systemKey)
		{
			if (systemKey == null)
				return null;

			var baseDir = Path.Combine(AppContext.BaseDirectory, "Resources", "Systems");
			var img = Path.Combine(baseDir, systemKey.Trim() + ".png");

			if (System.IO.File.Exists(img))
				return LoadBitmapNoLock(img);
			else
				return null;
		}

		public static Image DrawPlayOverlay(Image original)
		{
			Bitmap bmp = new Bitmap(original);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				// halbtransparenter Kreis
				int size = Math.Min(bmp.Width, bmp.Height) / 4;
				int x = (bmp.Width - size) / 2;
				int y = (bmp.Height - size) / 2;
				using var brushCircle = new SolidBrush(Color.FromArgb(128, Color.Black));
				g.FillEllipse(brushCircle, x, y, size, size);

				// weißes Dreieck (Play-Symbol)
				using var brushTriangle = new SolidBrush(Color.White);
				Point[] triangle =
				{
						new Point(x + size/3, y + size/4),
						new Point(x + size/3, y + size*3/4),
						new Point(x + size*2/3, y + size/2)
				};
				g.FillPolygon(brushTriangle, triangle);
			}
			return bmp;
		}

		public static bool ImagesAreDifferent(string? image1, string? image2)
		{
			if (string.IsNullOrEmpty(image1) || string.IsNullOrEmpty(image2))
				return false;

			var pix1 = HashTools.GetImagePixelHash(image1);
			var pix2 = HashTools.GetImagePixelHash(image2);
			bool identisch = pix1 != null && pix1 == pix2;

			bool visuellUnterschiedlich = false;
			if (!identisch && System.IO.File.Exists(image1) && System.IO.File.Exists(image2))
			{
				ulong d1 = HashTools.GetImageDHash64(image1);
				ulong d2 = HashTools.GetImageDHash64(image2);
				int hd = HashTools.HammingDistance(d1, d2);
				visuellUnterschiedlich = hd > 8; // Schwellwert nach Bedarf (5–12)
			}
			return visuellUnterschiedlich;
		}
	}
}
