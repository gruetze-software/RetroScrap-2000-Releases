using Microsoft.Win32.SafeHandles;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetroScrap2000.Tools
{
	public static class FileTools
	{
		// Konstanten und DllImport für GetFinalPathNameByHandle
		private const uint FILE_SHARE_READ = 0x00000001;
		private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
		private const uint OPEN_EXISTING = 3;
		private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
		private const int MAX_PATH = 260; // Standardpfadlänge
		private const uint FILE_NAME_NORMALIZED = 0x0;

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern SafeFileHandle CreateFile(
				string lpFileName,
				uint dwDesiredAccess,
				uint dwShareMode,
				IntPtr lpSecurityAttributes,
				uint dwCreationDisposition,
				uint dwFlagsAndAttributes,
				IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern uint GetFinalPathNameByHandle(
				SafeFileHandle hFile,
				[Out] StringBuilder lpszFilePath,
				uint cchFilePath,
				uint dwFlags);

		/// <summary>
		/// Ermittelt den tatsächlichen, Case-sensitiven Pfad eines existierenden ROMs unter Windows.
		/// </summary>
		/// <param name="romDirectory">Das Basisverzeichnis der ROMs.</param>
		/// <param name="caseInsensitiveAbsolutePath">Der absolute Pfad zur ROM-Datei (kann Case-insensitiv sein).</param>
		/// <returns>Der Case-sensitive relative Pfad (wie in der gamelist.xml erwartet) oder null bei Fehler/wenn die Datei nicht existiert.</returns>
		public static string? GetActualCaseSensitivePath(string romDirectory, string caseInsensitiveAbsolutePath)
		{
			if (!File.Exists(caseInsensitiveAbsolutePath))
			{
				return null;
			}

			// 1. Handle zur Datei erzeugen
			SafeFileHandle fileHandle = CreateFile(
					caseInsensitiveAbsolutePath,
					0, // Keine speziellen Zugriffsrechte erforderlich
					FILE_SHARE_READ,
					IntPtr.Zero,
					OPEN_EXISTING,
					FILE_ATTRIBUTE_NORMAL | FILE_FLAG_BACKUP_SEMANTICS, // Backup-Semantik für Dateien/Ordner
					IntPtr.Zero);

			if (fileHandle.IsInvalid)
			{
				// Fehler beim Öffnen der Datei.
				return null;
			}

			try
			{
				// 2. Tatsächlichen Pfad mittels Handle abfragen
				StringBuilder pathBuffer = new StringBuilder(MAX_PATH);
				uint result = GetFinalPathNameByHandle(
						fileHandle,
						pathBuffer,
						(uint)pathBuffer.Capacity,
						FILE_NAME_NORMALIZED);

				if (result == 0)
				{
					// Fehler bei GetFinalPathNameByHandle
					return null;
				}

				string fullCaseSensitivePath = pathBuffer.ToString();

				// Der Pfad wird von Windows oft mit dem Präfix "\\?\" zurückgegeben.
				const string pathPrefix = @"\\?\";
				if (fullCaseSensitivePath.StartsWith(pathPrefix, StringComparison.Ordinal))
				{
					fullCaseSensitivePath = fullCaseSensitivePath.Substring(pathPrefix.Length);
				}

				// 3. Absoluten Pfad in relativen Pfad umwandeln
				// Wir benötigen den Pfad relativ zum romDirectory, um ihn mit dem <path>-Tag zu vergleichen.
				if (fullCaseSensitivePath.StartsWith(romDirectory, StringComparison.OrdinalIgnoreCase))
				{
					// Ersetze Backslashes (\) durch Forward-Slashes (/), wie in EmulationStation üblich
					string relativePath = fullCaseSensitivePath.Substring(romDirectory.Length);
					relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

					// Fügen Sie das benötigte Slash-Präfix hinzu (z.B. ./rom.zip) oder /rom.zip, 
					// je nachdem, wie Ihre gamelist.xml die relativen Pfade speichert. 
					// Hier wird das führende Slash hinzugefügt, falls ES das erwartet:
					return "/" + relativePath.Replace('\\', '/');
				}

				return null;
			}
			finally
			{
				if (fileHandle != null && !fileHandle.IsInvalid)
				{
					fileHandle.Dispose();
				}
			}
		}

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
		/// Macht aus einem String "00168 Ping-Pong-Weltmeisterschaft (Europa)"
		/// "Ping-Pong-Weltmeisterschaft"
		/// </summary>
		/// <param name="rawGameName"></param>
		/// <returns>clean string</returns>
		public static string CleanSearchName(string? rawGameName, bool removeLeadingNumerics = false)
		{
			if (string.IsNullOrEmpty(rawGameName))
			{
				return string.Empty;
			}

			string cleanedName = rawGameName;

			// 0. Dateierweiterung entfernen
			cleanedName = Path.GetFileNameWithoutExtension(cleanedName);

			// 1. Optional: Entferne fortlaufende numerische Präfixe und das folgende Trennzeichen.
			if (removeLeadingNumerics)
			{
				// Logik: Sucht nach \d+ gefolgt von einem oder mehreren Trennzeichen (Leerzeichen, Bindestrich, Punkt, Unterstrich).
				// Behandelt: "007 - Game", "00168 Game"
				cleanedName = Regex.Replace(cleanedName, @"^\d+[\s\-\._]+", "").Trim();

				// Optional: Wenn der Name danach noch mit "The " beginnt, verschiebe ihn (nur sinnvoll nach ID-Entfernung)
				if (cleanedName.StartsWith("The ", StringComparison.OrdinalIgnoreCase))
				{
					cleanedName = Regex.Replace(cleanedName, @"^The\s", "").Trim();
				}
			}

			// 2. Entferne Regions- und Versions-Tags (in Klammern) vom Ende.
			// Muster: Erkenne tags in runden oder eckigen Klammern am ENDE der Zeichenkette.
			cleanedName = Regex.Replace(cleanedName, @"\s+[\(\[]\s*([^\)\]]*?)\s*[\)\]]$", "", RegexOptions.IgnoreCase).Trim();

			// Wiederhole 2., um verschachtelte oder aufeinanderfolgende Tags zu erfassen
			cleanedName = Regex.Replace(cleanedName, @"\s+[\(\[]\s*([^\)\]]*?)\s*[\)\]]$", "", RegexOptions.IgnoreCase).Trim();

			// 3. Entferne spezifische, typische ROM-Tags (Regionen, Versionen, Status-Tags)
			cleanedName = Regex.Replace(cleanedName, @"\s*[\(\[]\s*(?:(?:Europe|USA|Japan|World|En|De|Fr|v\d+\.\d+|Rev\s*\d|\!|Beta|Proto|Hack|Demo)\s*)*[\)\]]", "", RegexOptions.IgnoreCase).Trim();

			// 4. Bereinige überschüssige Leerzeichen
			cleanedName = Regex.Replace(cleanedName, @"\s+", " ").Trim();

			return cleanedName;
		
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
				var ret = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
				ret = ret.TrimEnd(Path.DirectorySeparatorChar);
				return ret;
			}
			catch (Exception)
			{
				// Bei unerwartetem Fehler (z.B. ungültiges Format)
				return absolutePath;
			}
		}

		public static string? ResolveMediaPath(string? systemDir, string? xmlValue)
		{
			if (string.IsNullOrEmpty(systemDir))
				return null;

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

		public static string NormalizeRelativePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			// 1. Alle Backslashes (\) durch Forward Slashes (/) ersetzen (Standard in Batocera/Linux)
			string normalizedPath = path.Replace('\\', '/');

			// 2. Führendes "./" oder nur "." entfernen, falls vorhanden
			if (normalizedPath.StartsWith("./"))
			{
				normalizedPath = normalizedPath.Substring(2);
			}
			else if (normalizedPath.StartsWith("."))
			{
				// Falls nur ein Punkt ohne Slash vorkommt
				normalizedPath = normalizedPath.Substring(1);
			}

			return normalizedPath;
		}

		public static bool SetLocalMediaFilesToGame(GameEntry game, string baseDir)
		{
			if (string.IsNullOrEmpty(game.Path)
				|| string.IsNullOrEmpty(baseDir))
				return false;
			
			string? absPathRom = ResolveMediaPath(game.Path, baseDir);
			if (string.IsNullOrEmpty(absPathRom))
				return false;

			// Medien-Verzeichnisse abklappern, in Frage kommen der Dateiname und der Name des Game	
			var dirMedienStrg = Path.Combine(baseDir, "media");
			DirectoryInfo dirMedien = new DirectoryInfo(dirMedienStrg);
			if (!dirMedien.Exists)
			{
				Log.Error($"FileTools::SetLocalMediaFilesToGame(\"{game.FileName}\") \"{dirMedienStrg}\" not exist.");
				return false;
			}

			var dirs = dirMedien.GetDirectories();
			foreach (var m in RetroScrapOptions.GetStandardMediaFolderAndXmlTagList())
			{
				var founddir = dirs.FirstOrDefault(x => x.Name.ToLower() == m.Value.ToLower());
				if (founddir != null)
				{
					var foundgame = Directory.GetFiles(founddir.FullName, game.FileName! + "*", SearchOption.TopDirectoryOnly);
					if ( foundgame == null || foundgame.Length == 0 )
						foundgame = Directory.GetFiles(founddir.FullName, game.Name + "*", SearchOption.TopDirectoryOnly);
					if (foundgame == null || foundgame.Length == 0)
						foundgame = Directory.GetFiles(founddir.FullName, FileTools.MakeSafeFileName(game.Name!) + "*", SearchOption.TopDirectoryOnly);
					if (foundgame == null || foundgame.Length == 0)
						continue;
					else
						game.SetMediaPath(m.Key, FileTools.GetRelativePath(foundgame[0], baseDir));
				}
			}

			return true;
		}

		public static (bool ok, string? file) MoveOrCopyScrapFileRom(bool move, GameEntry rom,
			string? sourcefile,
			string destbasedir,
			string destrelpath)
		{
			if (!string.IsNullOrEmpty(rom.FileName)
				&& !string.IsNullOrEmpty(sourcefile)
				&& File.Exists(sourcefile))
			{
				string destfileend = Path.GetExtension(sourcefile).ToLower();

				if (string.IsNullOrEmpty(destrelpath))
					throw new ApplicationException("Relative Path not set for " + rom.FileName + ".");

				string? abspath = ResolveMediaPath(destbasedir, destrelpath);
				if (!string.IsNullOrEmpty(abspath))
				{
					if (!Directory.Exists(abspath))
						Directory.CreateDirectory(abspath);

					string destfilename = Utils.GetNameFromFile(rom.FileName) + destfileend;
					string destfile = Path.Combine(abspath, destfilename);
					if (move)
					{
						Log.Information($"FileMove: \"{sourcefile}\" -> \"{destfile}\"");
						File.Move(sourcefile, destfile, overwrite: true);
					}
					else
					{
						Log.Information($"FileCopy: \"{sourcefile}\" -> \"{destfile}\"");
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
					Log.Information($"FileDelete: \"{abspath}\"");
					File.Delete(abspath);
					ImageTools.InvalidateCacheEntry(abspath); // Cache-Eintrag entfernen, falls vorhanden
					return true;
				}
				catch (Exception ex)
				{
					// 4. Fehlerbehandlung (z.B. Datei wird von einem anderen Prozess verwendet)
					// Hier könnten Sie auch ein MessageBox.Show() einfügen.
					Log.Error($"Error deleting file \"{abspath}\": {Utils.GetExcMsg(ex)}");
					return false;
				}
			}

			// Die Datei existiert nicht (mehr),
			// Kein Löschen nötig, als Erfolg werten.
			return true;
		}

		/// <summary>
		/// Erstellt eine M3U-Datei aus einer Liste von GameEntry-Objekten (Discs).
		/// </summary>
		/// <param name="romDirectory">Das Basisverzeichnis der ROMs.</param>
		/// <param name="selectedGames">Die ausgewählten GameEntry-Objekte (z.B. Disk 1, Disk 2, etc.).</param>
		/// <returns>Der Name der erzeugten M3U-Datei, oder null bei Fehler.</returns>
		public static string? CreateM3uFromGames(string romDirectory, IEnumerable<GameEntry> selectedGames)
		{
			var gameList = selectedGames.ToList();
			if (gameList.Count < 2) return null; // Mindestens zwei Discs benötigt

			// 1. Konsistenzprüfung: Alle Discs müssen im gleichen Unterordner liegen
			string? commonBaseDir = Path.GetDirectoryName(FileTools.ResolveMediaPath(romDirectory, gameList[0].Path));
			if (commonBaseDir == null) return null;

			// 2. Namensfindung für die M3U-Datei
			// Wir nehmen den Namens-Teil vor der Disc-Nummer und verwenden ihn als M3U-Namen.
			// Beispiel: "Game (Disk 1).chd" -> "Game.m3u"
			string baseName = GetBaseNameForM3u(Utils.GetNameFromFile(gameList[0].FileName!));
			if (string.IsNullOrEmpty(baseName)) baseName = "MultiDiscGame"; // Fallback

			// Erstelle den vollständigen Pfad zur M3U-Datei
			string m3uFileName = $"{baseName}.m3u";
			string m3uPath = Path.Combine(commonBaseDir, m3uFileName);

			var sb = new StringBuilder();

			// 3. Inhalt generieren (relativ zur M3U-Datei)
			foreach (var game in gameList.OrderBy(g => g.Path)) // Optional: Sortiere nach Pfad/Name, um Reihenfolge zu sichern
			{
				// Wir brauchen nur den Dateinamen relativ zum M3U-Verzeichnis
				string discFileName = game.FileName!;
				sb.AppendLine(discFileName);
			}

			// 4. M3U-Datei schreiben
			try
			{
				File.WriteAllText(m3uPath, sb.ToString(), Encoding.UTF8);

				// 5. Cleanup (optional, aber empfohlen): Originale Einträge markieren
				// Im UI-Kontextmenü müssen Sie anschließend:
				// a) die gamelist.xml mit den einzelnen Discs bereinigen (am besten mit Ihrer Clean-Funktion)
				// b) die einzelnen ROM-Dateien löschen/verschieben (falls gewünscht)
				// c) EINEN NEUEN GameEntry für die M3U-Datei erstellen und zur Roms.Games Liste hinzufügen.

				return m3uFileName;
			}
			catch (Exception ex)
			{
				// Fehlerbehandlung
				Log.Error($"Error create m3u-entry: {Utils.GetExcMsg(ex)}");
				return null;
			}
		}

		private static string GetBaseNameForM3u(string gameName)
		{
			// Entferne typische Disc- oder Part-Bezeichnungen
			var cleanedName = Regex.Replace(gameName, @"\s+\(\s*(Disc|Disk|CD|Part)\s*\d+\s*\)", "", RegexOptions.IgnoreCase);
			cleanedName = Regex.Replace(cleanedName, @"\s+\[\s*(Disc|Disk|CD|Part)\s*\d+\s*\]", "", RegexOptions.IgnoreCase);
			return cleanedName.Trim();
		}

		/// <summary>
		/// Liest eine M3U-Datei und extrahiert alle referenzierten Dateipfade.
		/// </summary>
		/// <param name="m3uPath">Der absolute Pfad zur M3U-Datei.</param>
		/// <returns>Eine Liste von relativen Pfaden der Disc-Dateien (relativ zum M3U-Ordner).</returns>
		public static List<string> GetM3uReferencedFiles(string m3uPath)
		{
			var referencedFiles = new List<string>();

			if (!File.Exists(m3uPath))
				return referencedFiles;

			try
			{
				// M3U-Dateien sind einfache Textdateien.
				var lines = File.ReadAllLines(m3uPath);
				var m3uDirectory = Path.GetDirectoryName(m3uPath);

				if (m3uDirectory == null)
					return referencedFiles;

				foreach (var line in lines)
				{
					string trimmedLine = line.Trim();
					if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
					{
						continue; // Überspringe leere Zeilen und Kommentare
					}

					// Normalerweise enthält die M3U nur den Dateinamen (relativ zum M3U-Ordner)
					// Beispiel: Resident Evil (Disc 1).chd
					// Da wir im Haupt-Load-Loop den Pfad relativ zum "romDirectory" benötigen, 
					// müssen wir ihn entsprechend auflösen.

					// 1. Absoluten Pfad der Disc-Datei ermitteln
					string absoluteDiscPath = Path.Combine(m3uDirectory, trimmedLine);

					// 2. Relativen Pfad zum Haupt-ROM-Ordner (romDirectory) ermitteln
					// (Hier ist eine Annahme über Ihre FileTools.ResolveMediaPath Logik, ansonsten 
					// müssen Sie den relativen Pfad direkt berechnen.)
					string? relativeDiscPath = FileTools.GetRelativePath(absoluteDiscPath, m3uDirectory);
					
					// Stellen Sie sicher, dass es das gewünschte Format (z.B. './' Präfix oder nur der relative Pfad) hat.
					// Für den Vergleich in 'm3uReferencedPaths' sollte es das NORMALISIERTE Format sein.
					var normalizedPath = NormalizeRelativePath("./" + relativeDiscPath?.Replace('\\', '/'));

					referencedFiles.Add(normalizedPath);
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error reading M3U-File \"{m3uPath}\": {Utils.GetExcMsg(ex)}");
			}

			return referencedFiles;
		}
	}
}
