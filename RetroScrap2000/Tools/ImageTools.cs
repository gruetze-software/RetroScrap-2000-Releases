using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
		public static async Task<Image?> LoadImageFromUrlCachedAsync(string? url, eMediaType type, CancellationToken ct)
		{
			
			if (string.IsNullOrWhiteSpace(url))
				return null;

			var key = url;
			if (_urlImageCache.TryGetValue(key, out var cached))
				return cached;

			string urllogstring = url.Substring(0, url.IndexOf("?") + 1) + "xxxxxx";
			Log.Information($"[{type.ToString()}]: ImageTools::LoadImageFromUrlCachedAsync(\"{urllogstring}\")");
			var bytes = await _http.GetByteArrayAsync(url, ct);
			Log.Debug(url);
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
		public static async Task<Image?> LoadImageCachedAsync(eMediaType type,
				string baseDir, string? relOrAbsPath, CancellationToken ct, bool bypassCache = false)
		{
			if (string.IsNullOrWhiteSpace(relOrAbsPath))
				return null;

			// absolut machen (unterstützt "./media/..." usw.)
			var abs = FileTools.ResolveMediaPath(baseDir, relOrAbsPath);
			if (string.IsNullOrEmpty(abs) )
			{
				Log.Debug($"[Tools::LoadImageCachedAsync]: ResolveMediaPath() from {relOrAbsPath} not success!");
				return null;
			}
			else if ( !System.IO.File.Exists(abs))
			{
				Log.Debug($"[Tools.LoadImageCachedAsync]: File not found: {abs}");
				return null;
			}

			try
			{
				// Versuche das Bild normal aus dem Cache oder der Datei zu laden
				var img = await LoadAbsoluteImageCachedAsync(type.ToString(), abs, ct, bypassCache);

				// Falls LoadAbsoluteImageCachedAsync intern null bei Fehlern liefert (z.B. bei PDFs),
				// generieren wir hier den Platzhalter.
				if (img == null)
				{
					Log.Debug($"[LoadImageCachedAsync]: Konnte Bild nicht laden (evtl. PDF/Manual), erstelle Platzhalter.");
					return CreatePlaceholderImage(Path.GetFileName(abs), type.ToString());
				}

				return img;
      }
      catch (Exception ex)
      {
        Log.Debug($"[LoadImageCachedAsync]: Fehler beim Laden von {abs}: {Utils.GetExcMsg(ex)}");
        // Rückfallebene: Platzhalter für Dokumente/Nicht-Bilder
        return CreatePlaceholderImage(Path.GetFileName(abs), type.ToString());
      }
    }

		/// <summary>
		/// Lädt ein Bild von einem absoluten Pfad, und cached es.
		/// </summary>
		public static async Task<Image?> LoadAbsoluteImageCachedAsync( string displayname,
				string absolutePath, CancellationToken ct, bool byPassCache = false)
		{
			if (string.IsNullOrWhiteSpace(absolutePath) || !System.IO.File.Exists(absolutePath))
				return null;

			var key = $"{absolutePath}";
			if (!byPassCache)
			{
				if (_localImageCache.TryGetValue(key, out var cached))
					return cached;
			}

			// I/O im Hintergrund
			return await Task.Run(() =>
			{
				ct.ThrowIfCancellationRequested();

				using var raw = LoadBitmapNoLock(absolutePath, displayname);
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


		/// <summary>
		/// Erzeugt ein Play-Overlay-Image und liefert es zusammen mit dem absoluten Videopfad.
		/// </summary>
		/// <param name="baseDir">Der Rom-Pfad inkl. System-Ordner</param>
		/// <param name="relPath">Der relative Pfad zur Videodatei</param>
		/// <param name="ct">Cancel-Token</param>
		/// <returns>Overlay-Image und Absolut-Pfad zur Videodatei</returns>
		public static async Task<(Image overlay, string videoAbsPath)?> LoadVideoPreviewAsync(eMediaType type, string baseDir,
			string? relPath, CancellationToken ct, bool byPassCache = false)
		{
			if (string.IsNullOrEmpty(relPath))
				return null;

			var videoAbs = FileTools.ResolveMediaPath(baseDir, relPath);
			if (!System.IO.File.Exists(videoAbs) || Path.GetExtension(videoAbs).ToLower() != ".mp4" )
				return null;

			var previewImgRelPath = GameEntry.GetMediaVideoPreviewImagePath(relPath);
			var previewAbs = FileTools.ResolveMediaPath(baseDir, previewImgRelPath);
			if (!string.IsNullOrEmpty(previewAbs) && !System.IO.File.Exists(previewAbs))
			{
				// erzeugen (dein ffmpeg‑Wrapper ist bereits async)
				await VideoTools.ExtractPreviewImage(videoAbs, previewAbs);
			}

			ct.ThrowIfCancellationRequested();

			var img = await LoadImageCachedAsync(type, baseDir, previewImgRelPath, ct, byPassCache);
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

		/// <summary>
		/// Entfernt den Cache-Eintrag für eine spezifische Datei. 
		/// Muss aufgerufen werden, wenn die Datei gelöscht oder ersetzt wurde.
		/// </summary>
		/// <param name="absoluteFilePath">Der absolute Pfad der gelöschten Datei.</param>
		public static void InvalidateCacheEntry(string absoluteFilePath)
		{
			Image? removedImage = null;

			// Prüfen, ob der Schlüssel existiert, ist nicht zwingend notwendig, 
			// da TryRemove einfach fehlschlägt, falls der Schlüssel nicht da ist.
			// Wir verwenden TryRemove direkt:
			if (_localImageCache.TryRemove(absoluteFilePath, out removedImage))
			{
				// Wichtig: Das entfernte Image-Objekt muss freigegeben werden,
				// damit die Systemressourcen (Handles) nicht hängen bleiben.
				removedImage.Dispose();
				Log.Debug($"Cache Invalidate: {absoluteFilePath} removed and disposed.");
			}
			else
			{
				Log.Debug($"Cache Invalidate: {absoluteFilePath} not found in cache or removal failed.");
			}
		}

		public static Image CreateErrorImage(string pathToShow) // Parameter auf vollen Pfad umbenannt
		{
			const int width = 144;
			const int height = 149;
			var bitmap = new Bitmap(width, height);

			using (Graphics g = Graphics.FromImage(bitmap))
			// Verkleinere die Schrift für den Pfad, um mehr Zeilenumbruch zu ermöglichen
			using (Font fontBold = new Font("Arial", 10, FontStyle.Bold))
			using (Font fontNormal = new Font("Arial", 7))
			{
				g.Clear(Color.LightGray); // Hintergrundfarbe

				string errorText1 = Properties.Resources.Txt_Fail_FileNotExist;
				string errorText2 = pathToShow; // Der volle Pfad

				// 1. Format für den oberen Fehlertext (Zentriert)
				var formatCenter = new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Near // Oben starten
				};

				// 2. Format für den Pfad (Links ausgerichtet, Umbruch zulassen)
				var formatPath = new StringFormat
				{
					Alignment = StringAlignment.Near, // Links ausrichten
					LineAlignment = StringAlignment.Near
				};

				// --- Zeichenbereiche definieren (mit 5px Rand) ---
				const int padding = 5;

				// Bereich für den Fehlertext (Startet oben, 30px hoch)
				var rectError = new RectangleF(padding, padding, width - 2 * padding, 30);

				// Bereich für den Pfad (Startet unter dem Fehlertext, nimmt den Rest des Platzes ein)
				var rectPath = new RectangleF(padding, rectError.Bottom + 5, width - 2 * padding, height - rectError.Bottom - 2 * padding);

				// Text 1: Fehlertext (oben zentriert)
				g.DrawString(errorText1, fontBold, Brushes.Red, rectError, formatCenter);

				// Text 2: Pfad (linksbündig, bricht automatisch um)
				g.DrawString(errorText2, fontNormal, Brushes.Black, rectPath, formatPath);

				// Füge einen Rahmen hinzu
				g.DrawRectangle(Pens.Red, 0, 0, width - 1, height - 1);
			}

			return bitmap;
		}

		public static Image? LoadBitmapNoLock(string? absolutePath, string displayname)
		{
			if (string.IsNullOrWhiteSpace(absolutePath) || !System.IO.File.Exists(absolutePath))
				return null;

			Log.Debug($"LoadBitmapNoLock(\"{absolutePath}\")");
			try
			{
				using var fs = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var tmp = Image.FromStream(fs);      // lädt ins GDI-Objekt
				return new Bitmap(tmp);                    // kopie -> Datei bleibt frei
			}
			catch (Exception ex)
			{
        Log.Debug($"Nicht-Bild oder Fehler erkannt, erstelle Platzhalter: {Utils.GetExcMsg(ex)}");
        return CreatePlaceholderImage(Path.GetFileName(absolutePath), displayname);
      }
		}

    private static Bitmap CreatePlaceholderImage(string fileName, string displayname)
    {
      // Größe des Platzhalters
      int width = 144;
      int height = 154;

      Bitmap bmp = new Bitmap(width, height);
      using (Graphics g = Graphics.FromImage(bmp))
      {
				// Hintergrund (Dunkelgrau für Dokumente/PDFs)
				g.Clear(Color.FromKnownColor(KnownColor.ControlLight));

        // Rahmen
        using (Pen pen = new Pen(Color.DarkBlue, 2))
        {
          g.DrawRectangle(pen, 10, 10, width - 15, height - 15);
        }

        // Text-Formatierung
        string extension = Path.GetExtension(fileName).ToUpper();
				string displayName = displayname;
        if ( string.IsNullOrEmpty(displayName))
          displayName = fileName.Length > 20 ? fileName.Substring(0, 19) + "..." : fileName;

        using (Font fontLabel = new Font("Arial", 8))
        using (Font fontExt = new Font("Arial", 20, FontStyle.Bold))
        using (Brush brush = new SolidBrush(Color.DarkBlue))
        {
          StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

          // Dateiendung groß in die Mitte (z.B. PDF)
          g.DrawString(extension, fontExt, brush, new RectangleF(0, 40, width, 60), sf);

          // displayname klein darunter
          g.DrawString(displayName, fontLabel, brush, new RectangleF(10, 100, width - 20, 40), sf);
        }
      }
      return bmp;
    }

    public static Image? GetSystemImage(string? systemKey)
		{
			if (systemKey == null)
				return null;

			var baseDir = Path.Combine(AppContext.BaseDirectory, "Resources", "Systems");
			var img = Path.Combine(baseDir, systemKey.Trim() + ".png");

			if (System.IO.File.Exists(img))
				return LoadBitmapNoLock(img, systemKey);
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
