using RetroScrap2000;
using RetroScrap2000.Tools;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;


public class RomStatus
{
	public GameEntry Game { get; set; } = new();
	public bool CoverExists { get; set; }
	public bool ScreenshotExists { get; set; }
	public bool VideoExists { get; set; }
}

public class GameManager
{
	public event EventHandler<LoadXmlActionEventArgs>? LoadXmlActionStart;
	public event EventHandler<LoadXmlActionEventArgs>? LoadXmlActionEnde;

	public Dictionary<string, GameList> SystemList { get; set; } = new Dictionary<string, GameList>();
	public string? RomPath { get; set; }
	public GameListLoader Loader { get; set; }
	public GameManager() 
	{
		Loader = new GameListLoader();
	}

	private void LoadSystem(string xmlpath, RetroSystem sys)
	{
		if ( string.IsNullOrEmpty(RomPath) )
			throw new ApplicationException("RomPath is null!");
		
		var key = Directory.GetParent(xmlpath)!.Name;
		LoadXmlActionStart?.Invoke(this, new LoadXmlActionEventArgs(sys));

		// WICHTIG: Cache vor dem Laden leeren, falls die XML-Datei extern geändert wurde
		// (obwohl die Cache-Logik dies prüft, ist es hier gut, vor einem Ladevorgang sicher zu sein).
		GameListXmlCache.ClearCache();

		// Die Load-Methode liefert immer eine GameList zurück, auch wenn die XML-Datei nicht existiert oder leer ist.
		var loadresult = Loader.Load(xmlpath, sys);
		GameList gl = loadresult.Games;

		if (gl.Games.Count > 0)
			SystemList.Add(key, gl);
		
		if (loadresult.HasChanges && gl.Games.Count > 0)
		{
			Trace.WriteLine("" + sys.Name_eu + ": Änderungen in der gamelist.xml erkannt. Save...");
			sys.SaveAllRomsToGamelistXml(RomPath, gl.Games);
			// KRITISCH: Den Cache leeren, da die Datei auf der Platte geändert wurde!
			GameListXmlCache.ClearCache();
		}
		LoadXmlActionEnde?.Invoke(this, new LoadXmlActionEventArgs(sys));
	}

	public RetroSystem? GetSystemById(int id)
	{
		foreach (var sys in SystemList.Values)
		{
			if (sys.RetroSys.Id == id)
				return sys.RetroSys;
		}
		return null;
	}

	public void	Load(string rompath, RetroSystems systems)
	{
		if (string.IsNullOrEmpty(rompath))
			throw new ApplicationException("RomPath is null!");

		SystemList.Clear();
		RomPath = rompath;
		if (!rompath.ToLower().EndsWith("roms"))
		{
			LoadSystem(Path.Combine(RomPath, "gamelist.xml"),
				systems.SystemList.FirstOrDefault(x => x.RomFolderName?.ToLower() == Path.GetFileName(RomPath).ToLower())!);
		}
		else
		{
			foreach (var sysDir in Directory.EnumerateDirectories(RomPath))
			{
				var key = Path.GetFileName(sysDir);
				RetroSystem? system = systems.SystemList.FirstOrDefault(x => x.RomFolderName?.ToLower() == key.ToLower());
				if (system == null)
				{
					Trace.WriteLine($"Warnung: Kein System für den Ordner '{key}' gefunden. Überspringe...");
					continue;
				}
				var xmlfile = Path.Combine(RomPath, sysDir, "gamelist.xml");
				LoadSystem(xmlfile, system);
			}
		}
	}
}

public class GameListLoader
{
	public GameListLoader()
	{
		
	}

	private static readonly object _xmlFileLock = new(); // primitive Sperre pro Prozess

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

	/// <summary>
	/// Löscht einen Eintrag aus der gamelist.xml basierend auf dem relativen Pfad der ROM-Datei.
	/// </summary>
	/// <param name="xmlPath">Der vollständige Pfad zur gamelist.xml-Datei.</param>
	/// <param name="romPath">Der relative Pfad der ROM-Datei, wie in <path> gespeichert.</param>
	/// <param name="deleteAllReferences">Kommt der path mehrfach vor, werden alle Einträge dazu gelöscht, wenn true gesetzt</param>
	/// <returns>True bei Erfolg, andernfalls false.</returns>
	public static bool DeleteGame(string xmlPath, GameEntry rom, bool deleteAllReferences = false)
	{
		// Sicherstellen, dass die XML-Datei existiert.
		if (!File.Exists(xmlPath) || string.IsNullOrEmpty(rom.Path))
		{
			return false;
		}

		// Stellen Sie sicher, dass das Lock-Objekt verfügbar ist.
		lock (_xmlFileLock)
		{
			try
			{
				// MUSS HIER GELADEN WERDEN, da wir den XML-Baum modifizieren und speichern.
				XDocument doc = XDocument.Load(xmlPath);
				var root = doc.Element("gameList");
				if (root == null)
				{
					return false;
				}

				// Normalisiere den Pfad einmal für alle Vergleiche
				string normalizedRomPath = NormalizeRelativePath(rom.Path);
				bool changesMade = false;

				// --- 1. Prüfen und Löschen aller Duplikate (wenn deleteAllReferences = true) ---

				var duplicates = root.Elements("game")
						.Where(g =>
						{
							string? xmlPathValue = g.Element("path")?.Value;
							if (xmlPathValue == null) return false;

							string normalizedXmlPath = NormalizeRelativePath(xmlPathValue);
							return normalizedXmlPath.Equals(normalizedRomPath, StringComparison.OrdinalIgnoreCase);
						})
						.ToList();

				if (deleteAllReferences && duplicates.Count > 1)
				{
					Trace.WriteLine($"Warnung: Mehrere Einträge ({duplicates.Count}) mit dem Pfad '{rom.Path}' gefunden. Lösche alle...");
					foreach (var dup in duplicates)
					{
						dup.Remove();
						changesMade = true;
					}
				}

				// Wenn alle Referenzen gelöscht wurden, speichern wir und beenden die Methode.
				if (changesMade)
				{
					doc.Save(xmlPath);
					
					// Cache inkrementell aktualisieren, nicht komplett löschen!
					// Wir müssen wissen, wie oft gelöscht wurde.
					int deletedCount = deleteAllReferences ? duplicates.Count : 1;
					for (int i = 0; i < deletedCount; i++)
					{
						GameListXmlCache.DecrementEntryCount(xmlPath, rom.Path);
					}

					// Falls nur ein spezifischer Eintrag gelöscht wurde:
					// GameListXmlCache.DecrementEntryCount(xmlPath, rom.Path);

					return true;
				}

				// --- 2. Finden und Löschen des spezifischen Eintrags (mit zusätzlichen Kriterien) ---

				// Wir verwenden FirstOrDefault nur, wenn deleteAllReferences FALSE ist.
				var gameToDelete = root.Elements("game")
						.FirstOrDefault(g =>
						{
							// 1. Pfad aus XML extrahieren und normalisieren
							string? xmlPathValue = g.Element("path")?.Value;
							string normalizedXmlPath = NormalizeRelativePath(xmlPathValue!);

							// 2. Vergleich des Pfades
							if (normalizedXmlPath != normalizedRomPath) return false;

							// 3. Zusätzliche Kriterien (nur prüfen, wenn Pfad übereinstimmt)
							return g.Element("name")?.Value == rom.Name
													&& g.Element("developer")?.Value == rom.Developer
													&& g.Element("publisher")?.Value == rom.Publisher
													&& g.Element("genre")?.Value == rom.Genre
													&& g.Element("releasedate")?.Value == rom.ReleaseDateRaw;
						});

				if (gameToDelete != null)
				{
					// Das Element aus dem Baum entfernen.
					gameToDelete.Remove();
					changesMade = true;
				}

				if (changesMade)
				{
					// Die Änderungen in der XML-Datei speichern.
					doc.Save(xmlPath);
					// KRITISCH: Den Cache leeren!
					GameListXmlCache.ClearCache();
					return true;
				}
				else
				{
					// Eintrag wurde nicht gefunden. Das ist okay.
					return true;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Fehler beim Löschen oder Speichern der XML: {ex.Message}");
				return false;
			}
		}
	}

	public static int GetNumbersOfEntriesInXml(string xmlPath, GameEntry rom)
	{
		// Sicherheitsprüfung, ob der Pfad der ROM überhaupt vorhanden ist
		if (string.IsNullOrEmpty(rom.Path))
		{
			return 0;
		}

		// Übergabe an die Cache-Methode. Diese ist schnell, da sie den Index nutzt.
		return GameListXmlCache.GetNumbersOfEntriesInXmlCached(xmlPath, rom.Path);
	}

	public static List<IGrouping<string, XElement>>? GetDuplicates(string xmlPath)
	{
		if (!File.Exists(xmlPath))
		{
			return null;
		}

		try
		{
			// XML-Dokument laden
			XDocument doc = XDocument.Load(xmlPath);
			var root = doc.Element("gameList");
			if (root == null)
			{
				return null;
			}

			// Alle '<game>'-Elemente laden
			var games = root.Elements("game");

			// Duplikate finden basierend auf dem 'id'-Attribut
			// Ignoriere Elemente, deren ID 0 oder leer ist
			var duplicates = games.Where(x => (string?)x.Attribute("id")?.Value != "0"
																		&& !string.IsNullOrEmpty((string?)x.Attribute("id")?.Value))
						.GroupBy(x => (string)x.Attribute("id")!.Value) 
						.Where(g => g.Count() > 1)
						.ToList();

			return duplicates;
		}
		catch (Exception ex)
		{
			// Fehlerbehandlung, falls das Laden der XML fehlschlägt
			// Sie können hier eine Meldung loggen
			Trace.WriteLine($"Fehler beim Prüfen der XML-Datei: {Utils.GetExcMsg(ex)}");
			return null;
		}
	}

	/// <summary>
	/// Bereinigt die gamelist.xml, indem Einträge entfernt werden, deren ROM- oder Mediendateien nicht mehr existieren.
	/// </summary>
	/// <param name="xmlPath">Der vollständige Pfad zur gamelist.xml.</param>
	/// <param name="romDirectory">Das Basisverzeichnis der ROMs (normalerweise das übergeordnete Verzeichnis der XML-Datei).</param>
	/// <returns>True, wenn Änderungen vorgenommen und gespeichert wurden, andernfalls false.</returns>
	public static (int anzRomDelete, int anzMediaDelete) CleanGamelistXmlByExistence(string xmlPath)
	{
		if (!File.Exists(xmlPath))
		{
			return (0 ,0);
		}

		var romDirectory = Path.GetDirectoryName(xmlPath);
		if (string.IsNullOrEmpty(romDirectory) || !Directory.Exists(romDirectory))
		{
			Trace.WriteLine("Fehler: Das ROM-Verzeichnis konnte nicht gefunden werden.");
			return (0, 0); 
		}

		// Lock-Objekt ist erforderlich, da wir mit der Datei arbeiten.
		lock (_xmlFileLock)
		{
			try
			{
				XDocument doc = XDocument.Load(xmlPath);
				var root = doc.Element("gameList");
				if (root == null)
				{
					return (0, 0);
				}

				bool changesMade = false;
				int anzRoms = 0;
				int anzMedia = 0;
				// Eine Liste, um die zu entfernenden Elemente zu sammeln
				List<XElement> elementsToRemove = new List<XElement>();

				foreach (var gameElement in root.Elements("game"))
				{
					// --- A. Existenz der ROM-Datei prüfen (im <path>-Tag) ---
					var pathElement = gameElement.Element("path");
					if (pathElement != null && !string.IsNullOrEmpty(pathElement.Value))
					{
						// Relativen Pfad (aus XML) zu einem absoluten Pfad auflösen
						string? absoluteRomPath = FileTools.ResolveMediaPath(romDirectory, pathElement.Value);

						if (!File.Exists(absoluteRomPath))
						{
							Trace.WriteLine($"Entferne Eintrag, da ROM-Datei nicht existiert: {pathElement.Value}");
							elementsToRemove.Add(gameElement);
							anzRoms++;
							changesMade = true;
							continue; // Gehe zum nächsten Game-Eintrag, wenn ROM fehlt
						}
					}

					// --- B. Existenz der Mediendateien prüfen (z.B. <image>, <video>) ---

					// Wir nehmen an, dass NUR gelöscht wird, wenn die ROM fehlt. 
					// Wenn jedoch ALLE Mediendateien fehlen und die ROM existiert, soll nur das Media-Tag entfernt werden.

					var imageElement = gameElement.Element("image");
					if (imageElement != null && !string.IsNullOrEmpty(imageElement.Value))
					{
						string? absoluteImagePath = FileTools.ResolveMediaPath(romDirectory, imageElement.Value);
						if (!File.Exists(absoluteImagePath))
						{
							imageElement.Remove();
							changesMade = true;
							anzMedia++;
							Trace.WriteLine($"Entferne <image>-Tag für {gameElement.Element("name")?.Value}, da Mediendatei fehlt.");
						}
					}

					var videoElement = gameElement.Element("video");
					if (videoElement != null && !string.IsNullOrEmpty(videoElement.Value))
					{
						string? absoluteVideoPath = FileTools.ResolveMediaPath(romDirectory, videoElement.Value);
						if (!File.Exists(absoluteVideoPath))
						{
							videoElement.Remove();
							changesMade = true;
							anzMedia++;
							Trace.WriteLine($"Entferne <video>-Tag für {gameElement.Element("name")?.Value}, da Mediendatei fehlt.");
						}
					}

					// Fügen Sie hier weitere Mediendateien (z.B. <manual>) hinzu.
				}

				// Alle gesammelten Elemente aus dem XML-Baum entfernen
				foreach (var element in elementsToRemove)
				{
					element.Remove();
				}

				if (changesMade)
				{
					doc.Save(xmlPath);

					// KRITISCH: Den Cache leeren, da die Datei geändert wurde!
					GameListXmlCache.ClearCache();

					return (anzRomDelete: anzRoms, anzMediaDelete: anzMedia);
				}

				return (0, 0); // Keine Änderungen vorgenommen
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Fehler beim Bereinigen der XML: {ex.Message}");
				return (0, 0);
			}
		}
	}

	public (GameList Games, bool HasChanges) Load(string? xmlPath, RetroSystem? system)
	{
		if (string.IsNullOrEmpty(xmlPath) || system == null)
			return (new GameList(), false);

		// Die ROMs werden typischerweise im selben Ordner wie die XML-Datei gespeichert.
		var romDirectory = Path.GetDirectoryName(xmlPath);
		if (string.IsNullOrEmpty(romDirectory) || !Directory.Exists(romDirectory))
		{
			Trace.WriteLine("ROM-Verzeichnis nicht gefunden.");
			return (new GameList(), false);
		}

		// Die Liste, die wir aufbauen werden. Starten mit einer leeren Liste.
		GameList loadedList = new GameList { RetroSys = system };

		bool hasChanges = false;
		// Schritt 1: Versuchen, die gamelist.xml zu laden
		
		try
		{
			if (File.Exists(xmlPath))
			{
				// Erstmal bereinigen TODO: Das dauert ewig und sollte als extra Methode auf der GUI sein
				// Vielleicht als "Experten-Tab" iun den Optionenß
				//CleanGamelistXmlByExistence(xmlPath);
				var serializer = new XmlSerializer(typeof(GameList));
				using var fs = new FileStream(xmlPath, FileMode.Open);
				loadedList = (GameList)serializer.Deserialize(fs)!;
				loadedList.RetroSys = system;
				foreach (var g in loadedList.Games)
				{
					g.RetroSystemId = system.Id;
				}
			}
			else
			{
				hasChanges = true;
				Trace.WriteLine($"XML-Datei nicht gefunden, erstelle neue Liste für {system.Name_eu}.");
			}
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Fehler beim Laden der XML: {Utils.GetExcMsg(ex)}");
			loadedList = new GameList { RetroSys = system }; // Leere Liste bei Fehler
			
		}

		// Definieren der Dateierweiterungen, die ausgeschlossen werden 
		var excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
				".db", ".xml", ".bak", ".sav", ".cfg", ".p2k", ".tmp", ".temp", ".txt", ".nfo", ".jpg", ".png", ".bmp",
				".jpeg", ".avi", ".mp4", ".mkv", ".m3u", ".cue", ".doc", ".pdf", ".keep"
		};

		// Schritt 2: Scannen des Dateisystems und Ausschließen von Dateien
		var existingRomsOnDisk = Directory.EnumerateFiles(romDirectory, "*.*", SearchOption.TopDirectoryOnly)
				.Where(filePath => !excludedExtensions.Contains(Path.GetExtension(filePath)))
				.ToList();

		var pathsInXml = new HashSet<string>(
			loadedList.Games
				.Select(g => g.Path)
				.Where(p => p != null)
				.Select(p => NormalizeRelativePath(p!)));

		foreach (var romFile in existingRomsOnDisk)
		{
			// 1. Erzeuge den relativen Pfad (dieser enthält dein "./" oder den relativen Ordner)
			var relativePathWithPrefix = "./" + Path.GetRelativePath(romDirectory, romFile).Replace('\\', '/');

			// 2. Normalisiere diesen Pfad für den Vergleich
			var normalizedPathForComparison = NormalizeRelativePath(relativePathWithPrefix);

			// Jetzt prüfen: Ist der NORMALISIERTE Pfad bereits in der normalisierten XML-Liste?
			if (!pathsInXml.Contains(normalizedPathForComparison))
			{
				string fileNameWithoutExt = Utils.GetNameFromFile(romFile);
				if (!string.IsNullOrEmpty(fileNameWithoutExt))
				{
					// FÜR DAS SPEICHERN: Entweder speicherst du den bereinigten Pfad (empfohlen)
					// oder den Pfad mit "./" (wenn du das so beibehalten möchtest).
					// Am saubersten ist es, den normalisierten (ohne ./) Pfad zu speichern.
					var newEntry = new GameEntry
					{
						Path = normalizedPathForComparison, // <--- Normalisierten Pfad speichern
						Name = fileNameWithoutExt,
						RetroSystemId = system.Id
					};

					loadedList.Games.Add(newEntry);
					hasChanges = true;
					Trace.WriteLine($"{system.Name_eu}: Neuen Eintrag hinzugefügt: \"{newEntry.Name}\"");
				}
			}
		}

		// Sortieren Sie die gesamte Liste nach dem Dateinamen
		loadedList.Games.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
		Trace.WriteLine("Load success " + system.Name_eu + ": Gesamtanzahl der Einträge in der gamelist.xml: " + loadedList.Games.Count);

		return (loadedList, hasChanges);
	}
}

public class LoadXmlActionEventArgs : EventArgs
{
	public RetroSystem? System { get; }

	public LoadXmlActionEventArgs(RetroSystem sys)
	{
		System = sys;
	}
}


