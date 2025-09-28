using RetroScrap2000;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;

public class GameManager
{
	public Dictionary<string, GameList> SystemList { get; set; } = new Dictionary<string, GameList>();
	public string? RomPath { get; set; }
	public GameManager() { }

	private void LoadSystem(string xmlpath, RetroSystems systems, RetroSystem sys)
	{
		if ( string.IsNullOrEmpty(RomPath) )
			throw new ApplicationException("RomPath is null!");

		var key = Directory.GetParent(xmlpath).Name;
		// Die Load-Methode liefert immer eine GameList zurück, auch wenn die XML-Datei nicht existiert oder leer ist.
		var loadresult = GameListLoader.Load(xmlpath, sys);
		GameList gl = loadresult.Games;
		if (gl.Games.Count > 0)
			SystemList.Add(key, gl);
		if (loadresult.HasChanges && gl.Games.Count > 0)
		{
			Trace.WriteLine("" + sys.Name + ": Änderungen in der gamelist.xml erkannt. Save...");
			// Es gab Änderungen, also speichern wir die aktualisierte Liste zurück in die XML-Datei.
			systems.SaveAllRomsToGamelistXml(RomPath, gl.Games);
		}
	}

	public void	Load(string rompath, RetroSystems systems)
	{
		if (string.IsNullOrEmpty(rompath))
			throw new ApplicationException("RomPath is null!");

		SystemList.Clear();
		RomPath = rompath;
		if (!rompath.ToLower().EndsWith("roms"))
		{
			LoadSystem(Path.Combine(RomPath, "gamelist.xml"), systems,
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
				LoadSystem(xmlfile, systems, system);
			}
		}
	}
}

public static class GameListLoader
{
	private static readonly object _xmlFileLock = new(); // primitive Sperre pro Prozess

	private static string NormalizeRelativePath(string path)
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

		// 3. Alle Pfade in Kleinbuchstaben konvertieren (um Case-Insensitivity zu erzwingen)
		return normalizedPath.ToLowerInvariant();
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
		if (!File.Exists(xmlPath) || string.IsNullOrEmpty(rom.Path) )
		{
			return false;
		}

		// Stellen Sie sicher, dass das Lock-Objekt verfügbar ist.
		// Falls es in einer anderen Klasse liegt, muss es entsprechend referenziert werden.
		lock (_xmlFileLock)
		{
			try
			{
				// XML-Dokument laden.
				XDocument doc = XDocument.Load(xmlPath);
				var root = doc.Element("gameList");
				if (root == null)
				{
					return false;
				}

				// Prüfen, ob die Datei mehrfach vorhanden ist
				string normalizedRomPath = NormalizeRelativePath(rom.Path);
				// Prüfen, ob die Datei mehrfach vorhanden ist
				var duplicates = root.Elements("game")
						.Where(g =>
						{
							var pathElem = g.Element("path");
							if (pathElem == null)
							{
								return false;
							}

							// Normalisiere den Pfad aus der XML
							string normalizedXmlPath = NormalizeRelativePath(pathElem.Value);

							// Führe den Vergleich mit den bereinigten Werten durch
							return normalizedXmlPath.Equals(normalizedRomPath, StringComparison.OrdinalIgnoreCase);
						})
						.ToList();

				if (deleteAllReferences)
				{
					if (duplicates.Count > 1)
					{
						Trace.WriteLine($"Warnung: Mehrere Einträge ({duplicates.Count}) mit dem Pfad '{rom.Path}' in der gamelist.xml " +
							"gefunden. Versuche, alle Einträge zu löschen.");
						foreach (var dup in duplicates)
						{
							dup.Remove();
						}
						doc.Save(xmlPath);
						return true;
					}
				}

				// Finden Sie das <game>-Element mit dem passenden <path>-Tag.
				var gameToDelete = root.Elements("game")
						.FirstOrDefault(g =>
						{
							// 1. Pfad aus XML extrahieren und normalisieren
							string? xmlPath = g.Element("path")?.Value;
							string normalizedXmlPath = NormalizeRelativePath(xmlPath!);

							// 2. Vergleich
							bool pathMatches = normalizedXmlPath == normalizedRomPath;

							// 3. Optional: Wenn der Pfad nicht übereinstimmt, sofort false zurückgeben
							if (!pathMatches) return false;

							// 4. Zusätzliche Kriterien (nur prüfen, wenn Pfad übereinstimmt)
							return g.Element("name")?.Value == rom.Name
									&& g.Element("developer")?.Value == rom.Developer
									&& g.Element("publisher")?.Value == rom.Publisher
									// Hinweis: Bei releasedate/genre/etc. können ebenfalls Formatierungsprobleme auftreten,
									// wenn diese nicht standardisiert gespeichert werden.
									&& g.Element("genre")?.Value == rom.Genre
									&& g.Element("releasedate")?.Value == rom.ReleaseDateRaw
									;
						});

				if (gameToDelete != null)
				{
					// Das Element aus dem Baum entfernen.
					gameToDelete.Remove();

					// Die Änderungen in der XML-Datei speichern.
					doc.Save(xmlPath);
					return true;
				}
				else
				{
					// Eintrag wurde nicht gefunden. Das ist okay, da vermutlich nur das File gelöscht werden soll.
					return true;
				}
			}
			catch
			{
				// Fehler beim Laden oder Speichern.
				return false;
			}
		}
	}

	public static int GetNumbersOfEntriesInXml(string xmlPath, GameEntry rom)
	{
		// Sicherstellen, dass die XML-Datei existiert.
		if (!File.Exists(xmlPath))
		{
			return 0;
		}

		// Stellen Sie sicher, dass das Lock-Objekt verfügbar ist.
		// Falls es in einer anderen Klasse liegt, muss es entsprechend referenziert werden.
		lock (_xmlFileLock)
		{
			try
			{
				// XML-Dokument laden.
				XDocument doc = XDocument.Load(xmlPath);
				var root = doc.Element("gameList");
				if (root == null)
				{
					return 0;
				}

				// Prüfen, wie oft die Datei über den NORMALISIERTEN Pfad vorhanden ist.
				string normalizedRomPath = NormalizeRelativePath(rom.Path!);
				var duplicates = root.Elements("game")
						.Where(g =>
						{
							// 1. Pfad aus XML extrahieren
							string? xmlPath = g.Element("path")?.Value;

							// 2. Den XML-Pfad normalisieren
							string normalizedXmlPath = NormalizeRelativePath(xmlPath!);

							// 3. Den normalisierten Pfad mit dem normalisierten Zielpfad vergleichen
							return normalizedXmlPath == normalizedRomPath;
						})
						.ToList();

					return duplicates.Count;
			}
			catch
			{
				// Fehler beim Laden
				return 0;
			}
		}
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

	public static bool CleanGamelistXml(string xmlPath)
	{
		if (!File.Exists(xmlPath) )
		{
			return false;
		}

		var duplicates = GetDuplicates(xmlPath);
		if ( duplicates == null || duplicates.Count == 0)
			return false;

		try
		{
			// XML-Dokument laden
			XDocument doc = XDocument.Load(xmlPath);
			var root = doc.Element("gameList");
			if (root == null)
			{
				return false;
			}

			// Alle doppelten Einträge entfernen und nur den ersten behalten
			foreach (var group in duplicates)
			{
				// Alle doppelten Einträge entfernen
				foreach (var element in group.Skip(1))
				{
					element.Remove();
				}
			}

			// Die bereinigte XML-Datei speichern
			doc.Save(xmlPath);
			return true;
		}
		catch (Exception ex)
		{
			// Fehlerbehandlung, falls das Laden der XML fehlschlägt
			// Sie können hier eine Meldung loggen
			Trace.WriteLine($"Fehler beim Bereinigen der XML-Datei: {Utils.GetExcMsg(ex)}");
			return false;
		}
	}
	public static (GameList Games, bool HasChanges) Load(string? xmlPath, RetroSystem? system)
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
				Trace.WriteLine($"XML-Datei nicht gefunden, erstelle neue Liste für {system.Name}.");
			}
		}
		catch (Exception ex)
		{
			Trace.WriteLine($"Fehler beim Laden der XML: {ex.Message}");
			loadedList = new GameList { RetroSys = system }; // Leere Liste bei Fehler
		}

		// Definieren der Dateierweiterungen, die ausgeschlossen werden 
		var excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
				".db", ".xml", ".bak", ".sav", ".cfg", ".p2k", ".tmp", ".temp", ".txt", ".nfo", ".jpg", ".png", ".bmp",
				".jpeg", ".avi", ".mp4", ".mkv", ".m3u", ".cue", ".doc", ".pdf", ".keep"
		};

		// Schritt 2: Scannen des Dateisystems und Ausschließen von Dateien
		var existingRomsOnDisk = Directory.EnumerateFiles(romDirectory, "*.*", SearchOption.AllDirectories)
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
					Trace.WriteLine($"{system.Name}: Neuen Eintrag hinzugefügt: \"{newEntry.Name}\"");
				}
			}
		}

		// Sortieren Sie die gesamte Liste nach dem Dateinamen
		loadedList.Games.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
		Trace.WriteLine("Load success " + system.Name + ": Gesamtanzahl der Einträge in der gamelist.xml: " + loadedList.Games.Count);
		return (loadedList, hasChanges);
	}
}

[XmlRoot("gameList")]
public class GameList
{
	[XmlIgnore]
	public RetroSystem RetroSys { get; set; } = new();

	[XmlElement("game")]
	public List<GameEntry> Games { get; set; } = new();
	
}

public enum eState : byte
{
	None = 0,
	NoData,
	Scraped,
	Error
};

[XmlRoot("game")]
public class GameEntry
{
	[XmlIgnore]
	public int RetroSystemId { get; set; } = -1;
	
	[XmlIgnore]
	public eState State { get; set; } = eState.None;

	[XmlAttribute("id")]
	public int Id { get; set; }

	[XmlAttribute("source")]
	public string? Source { get; set; }

	[XmlElement("path")]
	public string? Path { get; set; }

	[XmlElement("name")]
	public string? Name { get; set; }

	[XmlElement("desc")]
	public string? Description { get; set; }

	[XmlElement("rating")]
	public double Rating { get; set; }

	[XmlIgnore]
	public double RatingStars { get => ( Rating > 0.0 && Rating <= 1.0 ? Rating * 5.0 : 0.0 ); }  // nur lesend: 0..5
																																			

	[XmlElement("releasedate")]
	public string? ReleaseDateRaw { get; set; }

	[XmlIgnore]
	public DateTime? ReleaseDate
	{
		get
		{
			if (DateTime.TryParseExact(
							ReleaseDateRaw,
							"yyyyMMdd'T'HHmmss",
							null,
							System.Globalization.DateTimeStyles.None,
							out var dt))
				return dt;
			else if ( int.TryParse(ReleaseDateRaw, out int year ) && year > 1900 && year < 3000 )
			return new DateTime(year, 1, 1);
			else
				return null;
		}
		set
		{
			ReleaseDateRaw = value.HasValue
					? value.Value.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture)
					: null;
		}
	}

	[XmlElement("developer")]
	public string? Developer { get; set; }

	[XmlElement("publisher")]
	public string? Publisher { get; set; }

	[XmlElement("genre")]
	public string? Genre { get; set; }

	[XmlElement("players")]
	public string? Players { get; set; }

	[XmlElement("image")]
	public string? MediaScreenshotPath { get; set; }

	[XmlElement("thumbnail")]
	public string? MediaCoverPath { get; set; }

	[XmlElement("video")]
	public string? MediaVideoPath { get; set; }

	[XmlIgnore]
	public string? MediaVideoPreviewImagePath
	{
		get
		{
			if (string.IsNullOrEmpty(MediaVideoPath))
				return null;

			// Absoluten Pfad zum Video bauen
			var videoPath = MediaVideoPath.TrimStart('.', '/', '\\');

			// Verzeichnis des Videos
			var dir = System.IO.Path.GetDirectoryName(videoPath);
			if (string.IsNullOrEmpty(dir))
				return null;

			// Dateiname ohne .mp4
			var baseName = Utils.GetNameFromFile(videoPath);

			// Vorschau-Dateiname anhängen
			return System.IO.Path.Combine(dir, baseName + "_preview.jpg");
		}
	}

	[XmlElement("genreid")]
	public int GenreId { get; set; }
}

