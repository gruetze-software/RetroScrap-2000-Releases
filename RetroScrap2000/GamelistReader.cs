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

	public void	Load(string rompath, RetroSystems systems)
	{
		if (string.IsNullOrEmpty(rompath))
			throw new ApplicationException("RomPath is null!");

		SystemList.Clear();
		RomPath = rompath;
		if (!rompath.ToLower().EndsWith("roms"))
		{
			RomPath = Directory.GetParent(rompath)?.FullName;
			var key = Path.GetFileName(rompath);
			RetroSystem? system = systems.SystemList.FirstOrDefault(x => x.RomFolderName?.ToLower() == key.ToLower());
			if (system == null)
			{
				Debug.Assert(false);
				return;
			}

			var xmlfile = Path.Combine(rompath, "gamelist.xml");
			if (File.Exists(xmlfile))
			{
				var l = GameListLoader.Load(xmlfile, system);
				if (l != null && l.Games.Count > 0)
				{
					l.RetroSys = system;
					SystemList.Add(key, l);
				}
			}
		}
		else
		{
			foreach (var sysDir in Directory.EnumerateDirectories(rompath))
			{
				var key = Path.GetFileName(sysDir);
				RetroSystem? system = systems.SystemList.FirstOrDefault(x => x.RomFolderName?.ToLower() == key.ToLower());
				if (system == null)
				{
					continue;
				}
				var xmlfile = Path.Combine(rompath, sysDir, "gamelist.xml");
				if (File.Exists(xmlfile))
				{
					var l = GameListLoader.Load(xmlfile, system);
					if (l != null && l.Games.Count > 0)
					{
						l.RetroSys = system;
						SystemList.Add(key, l);
					}
				}
			}
		}
	}
}

public static class GameListLoader
{
	public static GameList Load(string xmlPath, RetroSystem system)
	{
		try
		{
			Trace.WriteLine($"{system}: Read {xmlPath}");
			var serializer = new XmlSerializer(typeof(GameList));
			using var fs = new FileStream(xmlPath, FileMode.Open);
			GameList liste = (GameList)serializer.Deserialize(fs)!;
			liste.RetroSys = system;
			foreach (var g in liste.Games)
				g.RetroSystemId = system.Id;
			return liste;
		}
		catch (Exception ex)
		{
			Trace.WriteLine(ex.Message);
			if (ex.InnerException != null)
				Trace.WriteLine(ex.InnerException.Message);
			return new GameList();
		}
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

