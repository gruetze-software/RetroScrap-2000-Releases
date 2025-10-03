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

		/// <summary>
		/// Berechnet einen relativen Pfad vom Basisordner zum Zielordner.
		/// </summary>
		/// <param name="absolutePath">Der Zielordner (z.B. C:\Manuals\SNES).</param>
		/// <param name="relativeTo">Der Basisordner (z.B. C:\Roms\SNES).</param>
		/// <returns>Der relative Pfad (z.B. ..\..\Manuals\SNES) oder der absolute Pfad, falls kein Bezug möglich ist.</returns>
		public static string GetRelativePath(string absolutePath, string relativeTo)
		{
			// 1. Laufwerkprüfung (muss dasselbe sein)
			string baseRoot = Path.GetPathRoot(relativeTo) ?? string.Empty;
			string targetRoot = Path.GetPathRoot(absolutePath) ?? string.Empty;

			if (!baseRoot.Equals(targetRoot, StringComparison.OrdinalIgnoreCase))
			{
				// Fehler: Läuft auf verschiedenen Laufwerken. Rückgabe des absoluten Pfades oder Fehlermeldung
				return absolutePath;
			}

			// 2. Relativen Pfad über die URI-Klasse berechnen (sauberste Methode)
			try
			{
				// Beide Pfade müssen mit einem abschließenden Slash versehen sein
				Uri baseUri = new Uri(relativeTo.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
				Uri targetUri = new Uri(absolutePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

				Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

				// Ergebnis als normalen String zurückgeben und Slashes korrigieren
				return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
			}
			catch (Exception)
			{
				// Bei unerwartetem Fehler (z.B. ungültiges Format)
				return absolutePath;
			}
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

		public static (bool ok, string? file) MoveOrCopyScrapFileRom(bool move, string? romname,
			string? sourcefile,
			string destbasedir,
			string destrelpath)
		{
			if (!string.IsNullOrEmpty(romname)
				&& !string.IsNullOrEmpty(sourcefile)
				&& File.Exists(sourcefile))
			{
				string destfileend = Path.GetExtension(sourcefile).ToLower();

				if (string.IsNullOrEmpty(destrelpath))
					throw new ApplicationException("Relative Path not set for " + romname + ".");

				string? abspath = ResolveMediaPath(destbasedir, destrelpath);
				if (!string.IsNullOrEmpty(abspath))
				{
					if (!Directory.Exists(abspath))
						Directory.CreateDirectory(abspath);

					string destfilename = MakeSafeFileName(romname) + destfileend;
					string destfile = Path.Combine(abspath, destfilename);
					if (move)
					{
						Trace.WriteLine($"FileMove: \"{sourcefile}\" -> \"{destfile}\"");
						File.Move(sourcefile, destfile, overwrite: true);
					}
					else
					{
						Trace.WriteLine($"FileCopy: \"{sourcefile}\" -> \"{destfile}\"");
						File.Copy(sourcefile, destfile, overwrite: true);
					}
						
					return (true, Path.Combine(destrelpath, destfilename));
				}
			}

			return (false, null);
		}

		/// <summary>
		/// Löscht eine Medien-Datei.
		/// </summary>
		/// <param name="destbasedir">Der Basisordner (z.B. der Rom-Ordner).</param>
		/// <param name="destrelpath">Der relative Pfad zur Datei (z.B. ./media/images/).</param>
		/// <returns>True, wenn die Operation erfolgreich war oder keine Löschung angefordert wurde. False bei Fehler.</returns>
		public static bool DeleteScrapFile(string destbasedir, string destrelpath)
		{

			// 1. Absoluten Pfad auflösen
			// Annahme: ResolveMediaPath ist eine verfügbare Funktion in Ihrer Utility-Klasse
			string? abspath = ResolveMediaPath(destbasedir, destrelpath);

			if (string.IsNullOrEmpty(abspath))
			{
				// Der Pfad konnte nicht aufgelöst werden, oder ist bereits leer.
				// Keine Aktion notwendig, als Erfolg werten.
				return true;
			}

			// 2. Prüfen, ob die Datei existiert
			if (File.Exists(abspath))
			{
				try
				{
					// 3. Datei löschen
					Trace.WriteLine($"FileDelete: \"{abspath}\"");
					File.Delete(abspath);
					ImageTools.InvalidateCacheEntry(abspath); // Cache-Eintrag entfernen, falls vorhanden
					return true;
				}
				catch (Exception ex)
				{
					// 4. Fehlerbehandlung (z.B. Datei wird von einem anderen Prozess verwendet)
					// Hier könnten Sie auch ein MessageBox.Show() einfügen.
					Trace.WriteLine($"ERROR deleting file \"{abspath}\": {Utils.GetExcMsg(ex)}");
					return false;
				}
			}

			// Die Datei existiert nicht (mehr),
			// Kein Löschen nötig, als Erfolg werten.
			return true;
		}
	}
}
