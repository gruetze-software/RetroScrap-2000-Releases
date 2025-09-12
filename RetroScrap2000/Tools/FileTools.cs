using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000.Tools
{
	public static class FileTools
	{
		/// <summary>
		/// Speichert ein vorhandenes Image (im Speicher) als PNG in ein Temp-File und gibt den Pfad zurück.
		/// </summary>
		public static string? SaveImageToTempFile(Image? img, string baseName = "image")
		{
			if (img == null)
				return null;

			var dir = Path.Combine(Path.GetTempPath(), "RetroScrap2000", "preview_cache");
			Directory.CreateDirectory(dir);
			var safe = MakeSafeFileName(baseName);
			var abs = Path.Combine(dir, $"{safe}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.png");

			try
			{
				img.Save(abs, System.Drawing.Imaging.ImageFormat.Png);
				return abs;
			}
			catch { return null; }
		}

		public static string MakeSafeFileName(string name)
		{
			foreach (var c in Path.GetInvalidFileNameChars())
				name = name.Replace(c, '_');
			name = name.Replace(' ', '_');
			return name;
		}

		public static string? ResolveMediaPath(string systemDir, string? xmlValue)
		{
			if (string.IsNullOrWhiteSpace(xmlValue)) return null;

			// Bereits absolut?
			if (Path.IsPathRooted(xmlValue)) return xmlValue;

			// "./" entfernen und Slashes normalisieren
			var rel = xmlValue.Trim();
			if (rel.StartsWith("./") || rel.StartsWith(".\\"))
				rel = rel.Substring(2);

			rel = rel.Replace('/', Path.DirectorySeparatorChar)
							 .Replace('\\', Path.DirectorySeparatorChar);

			return Path.Combine(systemDir, rel);
		}

		public static (bool ok, string? file) MoveScrapFileRom(string? romname,
			string? sourcefile,
			string destbasedir,
			string destrelpath,
			string? destfileend)
		{
			if (!string.IsNullOrEmpty(romname)
				&& !string.IsNullOrEmpty(sourcefile)
				&& File.Exists(sourcefile))
			{
				if (string.IsNullOrEmpty(destfileend)
					|| Path.GetExtension(sourcefile).ToLower() != destfileend.ToLower())
					destfileend = Path.GetExtension(sourcefile).ToLower();

				if (string.IsNullOrEmpty(destrelpath))
					throw new ApplicationException("Relative Path not set for " + romname + ".");

				string? abspath = ResolveMediaPath(destbasedir, destrelpath);
				if (!string.IsNullOrEmpty(abspath))
				{
					if (!Directory.Exists(abspath))
						Directory.CreateDirectory(abspath);

					string destfilename = MakeSafeFileName(romname) + destfileend;
					string destfile = Path.Combine(abspath, destfilename);
					Trace.WriteLine($"FileMove: \"{sourcefile}\" -> \"{destfile}\"");
					File.Move(sourcefile, destfile, overwrite: true);
					return (true, Path.Combine(destrelpath, destfilename));
				}
			}

			return (false, null);
		}
	}
}
