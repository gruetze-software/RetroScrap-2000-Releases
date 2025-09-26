using RetroScrap2000;
using System.CodeDom;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;

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

	/// <summary>
	/// Löscht einen Eintrag aus der gamelist.xml basierend auf dem relativen Pfad der ROM-Datei.
	/// </summary>
	/// <param name="xmlPath">Der vollständige Pfad zur gamelist.xml-Datei.</param>
	/// <param name="romPath">Der relative Pfad der ROM-Datei, wie in <path> gespeichert.</param>
	/// <returns>True bei Erfolg, andernfalls false.</returns>
	public static bool DeleteGameByPath(string xmlPath, string romRelPath)
	{
		// Sicherstellen, dass die XML-Datei existiert.
		if (!File.Exists(xmlPath))
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

				//// Finden Sie das <game>-Element mit dem passenden <path>-Tag.
				var gameToDelete = root.Elements("game")
															 .FirstOrDefault(g => g.Element("path")?.Value == romRelPath);

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
					// Eintrag wurde nicht gefunden.
					return false;
				}
			}
			catch
			{
				// Fehler beim Laden oder Speichern.
				return false;
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
						.Cast<string>(),
				StringComparer.OrdinalIgnoreCase
		);

		foreach (var romFile in existingRomsOnDisk)
		{
			var relativePath = "./" + Path.GetRelativePath(romDirectory, romFile).Replace('\\', '/');

			if (!pathsInXml.Contains(relativePath))
			{
				if (!string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(romFile)))
				{
					var newEntry = new GameEntry
					{
						Path = relativePath,
						Name = Path.GetFileNameWithoutExtension(romFile),
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

[XmlRoot("game")]
public class GameEntry
{
	[XmlIgnore]
	public int RetroSystemId { get; set; } = -1;

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
			var baseName = System.IO.Path.GetFileNameWithoutExtension(videoPath);

			// Vorschau-Dateiname anhängen
			return System.IO.Path.Combine(dir, baseName + "_preview.jpg");
		}
	}

	[XmlElement("genreid")]
	public int GenreId { get; set; }
}

