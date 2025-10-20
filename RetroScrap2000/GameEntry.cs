using RetroScrap2000.Tools;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RetroScrap2000
{
	[XmlRoot("gameList")]
	public class GameList
	{
		[XmlIgnore]
		public RetroSystem RetroSys { get; set; } = new();

		[XmlElement("game")]
		public List<GameEntry> Games { get; set; } = new();

		/// <summary>
		/// Erstellt eine M3U-Datei aus den ausgewählten GameEntry-Objekten, entfernt die Quell-Einträge
		/// und fügt den neuen M3U-Eintrag zur Liste hinzu.
		/// </summary>
		/// <param name="romDirectory">Das Basisverzeichnis der ROMs.</param>
		/// <param name="selectedGames">Die Einträge, aus denen die M3U generiert werden soll (mind. 2).</param>
		/// <returns>True, wenn die M3U erfolgreich erstellt und die Liste aktualisiert wurde, andernfalls false.</returns>
		public GameEntry? GenerateM3uForSelectedGames(string romDirectory, IEnumerable<GameEntry> selectedGames)
		{
			var gameList = selectedGames.ToList();
			if (gameList.Count < 2)
			{
				Log.Error("M3U generation canceled: At least two entries must be selected.");
				return null;
			}

			// 1. M3U-Datei auf der Festplatte erstellen
			string? m3uFileName = FileTools.CreateM3uFromGames(romDirectory, gameList);

			if (string.IsNullOrEmpty(m3uFileName))
			{
				Log.Error("M3U-File does not created.");
				return null;
			}

			// 2. XML-Einträge der einzelnen Discs aus der aktuellen GameList entfernen
			int deletedCount = 0;
			GameEntry? data = null;
			foreach (var gameToRemove in gameList)
			{
				if ( data == null && !string.IsNullOrEmpty(gameToRemove.Description) && !string.IsNullOrEmpty(gameToRemove.Name))
					data = gameToRemove;

				if (this.Games.Remove(gameToRemove))
				{
					deletedCount++;
				}
			}

			if (deletedCount != gameList.Count)
			{
				Log.Warning($"{gameList.Count - deletedCount} of the source entries could not be removed from the in-memory list.");
			}

			// 3. Neuen GameEntry für die M3U-Datei erstellen und hinzufügen

			// Der Pfad muss dem normalisierten Format entsprechen, das in der Load-Funktion erwartet wird.
			// Beispiel: "Game.m3u" wird zu "/Game.m3u" (falls NormalizeRelativePath das './' entfernt)

			// Annahme: Die m3uFileName ist bereits relativ zum romDirectory und muss nur noch normalisiert werden.
			string m3uNormalizedPath = FileTools.NormalizeRelativePath("/" + m3uFileName);

			var newM3uEntry = new GameEntry
			{
				// Name basiert auf dem von FileTools ermittelten M3U-Basisnamen
				Name = data != null ? data.Name : Utils.GetNameFromFile(m3uFileName),
				Path = m3uNormalizedPath,
				RetroSystemId = this.RetroSys?.Id ?? 0,
				Description = data != null ? data.Description : "",
				Publisher = data != null ? data.Publisher : "",
				Favorite = data != null ? data.Favorite : false,
				Developer = data != null ? data.Developer : "",
				Genre = data != null ? data.Genre : "",
				Id = data != null ? data.Id : 0,
				MediaFanArtPath = data != null ? data.MediaFanArtPath : null,
				MediaImageBoxPath = data != null ? data.MediaImageBoxPath : null,
				MediaImageBoxBack = data != null ? data.MediaImageBoxPath : null,
				MediaImageBoxSide = data != null ? data.MediaImageBoxSide : null,
				MediaImageBoxTexture = data != null ? data.MediaImageBoxTexture : null,
				MediaManualPath = data != null ? data.MediaManualPath : null,
				MediaMapPath = data != null ? data.MediaMapPath : null,
				MediaMarqueePath = data !=null ? data.MediaMarqueePath : null,
				MediaScreenshotPath = data != null ? data.MediaScreenshotPath : null,
				MediaScreenshotTitlePath = data != null ? data.MediaScreenshotTitlePath : null,
				MediaVideoPath = data != null ? data.MediaVideoPath : null,
				MediaWheelPath = data != null ? data.MediaWheelPath : null,
				ReleaseDate = data != null ? data.ReleaseDate : null,
				Players = data != null ? data.Players : null,
				Rating = data != null ? data.Rating : 0,
				Source = data != null ? data.Source : null
			};

			this.Games.Add(newM3uEntry);

			// Sortieren Sie die gesamte Liste neu, damit der neue M3U-Eintrag korrekt einsortiert wird
			this.Games.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));

			Log.Information($"Success: M3U '{m3uFileName}' is created and {deletedCount} source entries deleted.");

			// Wichtig: Setzen Sie ein Flag, das dem Hauptprogramm signalisiert, dass die XML-Datei 
			// gespeichert werden muss.
			// Wenn Ihre GameList-Klasse ein 'HasUnsavedChanges'-Flag hat, setzen Sie es hier auf true.
			// Wenn nicht, wird das Speichern oft beim Beenden oder manuell ausgelöst.

			return newM3uEntry;
		}

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
		public string? FileName { get { if (string.IsNullOrEmpty(Path)) return null; else return System.IO.Path.GetFileName(Path); } }

		[XmlIgnore]
		public eState State { get; set; } = eState.None;

		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlAttribute("source")]
		public string? Source { get; set; }
		
		[XmlElement("favorite")]
		public string? FavoriteString { get; set; }

		[XmlIgnore]
		public bool Favorite
		{
			get
			{
				return FavoriteString != null
				&& (FavoriteString.Equals("true", StringComparison.OrdinalIgnoreCase)
				|| FavoriteString.Equals("1"));
			}
			set { if (value == true) FavoriteString = "true"; else FavoriteString = null; }
		}
		

		[XmlElement("path")]
		public string? Path { get; set; }

		[XmlElement("name")]
		public string? Name { get; set; }

		[XmlElement("desc")]
		public string? Description { get; set; }

		[XmlElement("rating")]
		public double Rating { get; set; }

		[XmlIgnore]
		public double RatingStars { get => (Rating > 0.0 && Rating <= 1.0 ? Rating * 5.0 : 0.0); }  // nur lesend: 0..5


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
				else if (int.TryParse(ReleaseDateRaw, out int year) && year > 1900 && year < 3000)
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
		public string? MediaImageBoxPath { get; set; }
		
		[XmlElement("back")]
		public string? MediaImageBoxBack { get; set; }
		[XmlElement("side")]
		public string? MediaImageBoxSide { get; set; }
		[XmlElement("texture")]
		public string? MediaImageBoxTexture { get; set; }

		[XmlElement("video")]
		public string? MediaVideoPath { get; set; }
		[XmlElement("marquee")]
		public string? MediaMarqueePath { get; set; }

		[XmlElement("fanart")]
		public string? MediaFanArtPath { get; set; }
		[XmlElement("screenshot")]
		public string? MediaScreenshotPath { get; set; }
		[XmlElement("title")]
		public string? MediaScreenshotTitlePath { get; set; }

		[XmlElement("wheel")]
		public string? MediaWheelPath { get; set; }
		[XmlElement("manual")]
		public string? MediaManualPath { get; set; }
		[XmlElement("map")]
		public string? MediaMapPath { get; set; }


		public override string ToString()
		{
			return Name ?? FileName ?? "[Empty GameEntry]";
		}

		/// <summary>
		/// Liefert den relativen Pfad zum Preview-JPG für ein Video. 
		/// </summary>
		/// <param name="relVideoPath"></param>
		/// <returns></returns>
		public static string? GetMediaVideoPreviewImagePath(string relVideoPath)
		{
			if (string.IsNullOrEmpty(relVideoPath))
				return null;

			// Absoluten Pfad zum Video bauen
			var videoPath = relVideoPath.TrimStart('.', '/', '\\');

			// Verzeichnis des Videos
			var dir = System.IO.Path.GetDirectoryName(videoPath);
			if (string.IsNullOrEmpty(dir))
				return null;

			// Dateiname ohne .mp4
			var baseName = Utils.GetNameFromFile(videoPath);

			// Vorschau-Dateiname anhängen
			return System.IO.Path.Combine(dir, baseName + "_preview.jpg");
		}


		[XmlElement("genreid")]
		public int GenreId { get; set; }

		[XmlIgnore]
		public Dictionary<eMediaType, string?> MediaTypeDictionary
		{
			get
			{
				return new Dictionary<eMediaType, string?>()
				{
					{ eMediaType.BoxImageFront, this.MediaImageBoxPath },
					{ eMediaType.BoxImageBack, this.MediaImageBoxBack },
					{ eMediaType.BoxImageSide, this.MediaImageBoxSide },
					{ eMediaType.BoxImageTexture, this.MediaImageBoxTexture },
					{ eMediaType.ScreenshotGame, this.MediaScreenshotPath },
					{ eMediaType.ScreenshotTitle, this.MediaScreenshotTitlePath },
					{ eMediaType.Fanart, this.MediaFanArtPath },
					{ eMediaType.Marquee, this.MediaMarqueePath },
					{ eMediaType.Manual, this.MediaManualPath },
					{ eMediaType.Map, this.MediaMapPath },
					{ eMediaType.Video, this.MediaVideoPath },
					{ eMediaType.Wheel, this.MediaWheelPath }
				};
			}
		}

		public void SetMediaPath(eMediaType type, string? path)
		{
			switch (type)
			{
				case eMediaType.BoxImageFront: this.MediaImageBoxPath = path;	break;
				case eMediaType.BoxImageSide: this.MediaImageBoxSide = path; break;
				case eMediaType.BoxImageBack: this.MediaImageBoxBack = path; break;
				case eMediaType.BoxImageTexture: this.MediaImageBoxTexture = path; break;
				case eMediaType.ScreenshotGame: this.MediaScreenshotPath = path; break;
				case eMediaType.ScreenshotTitle: this.MediaScreenshotTitlePath = path; break;
				case eMediaType.Fanart: this.MediaFanArtPath = path; break;
				case eMediaType.Marquee: this.MediaMarqueePath = path; break;
				case eMediaType.Manual: this.MediaManualPath = path; break;
				case eMediaType.Map: this.MediaMapPath = path; break;
				case eMediaType.Video: this.MediaVideoPath = path; break;
				case eMediaType.Wheel: this.MediaWheelPath = path; break;
				default: Debug.Assert(false, "Unbekannter Medientyp"); break;
			}
		}

	}

}
