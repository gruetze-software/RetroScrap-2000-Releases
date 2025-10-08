using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

		[XmlElement("video")]
		public string? MediaVideoPath { get; set; }
		[XmlElement("marquee")]
		public string? MediaMarqueePath { get; set; }

		[XmlElement("fanart")]
		public string? MediaFanArtPath { get; set; }
		[XmlElement("screenshot")]
		public string? MediaScreenshotPath { get; set; }
		[XmlElement("wheel")]
		public string? MediaWheelPath { get; set; }
		[XmlElement("manual")]
		public string? MediaManualPath { get; set; }
		[XmlElement("map")]
		public string? MediaMapPath { get; set; }


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
				{ eMediaType.BoxImage, this.MediaImageBoxPath },
				{ eMediaType.Screenshot, this.MediaScreenshotPath },
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
				case eMediaType.BoxImage:
					this.MediaImageBoxPath = path;
					break;
				case eMediaType.Screenshot:
					this.MediaScreenshotPath = path;
					break;
				case eMediaType.Fanart:
					this.MediaFanArtPath = path;
					break;
				case eMediaType.Marquee:
					this.MediaMarqueePath = path;
					break;
				case eMediaType.Manual:
					this.MediaManualPath = path;
					break;
				case eMediaType.Map:
					this.MediaMapPath = path;
					break;
				case eMediaType.Video:
					this.MediaVideoPath = path;
					break;
				case eMediaType.Wheel:
					this.MediaWheelPath = path;
					break;
				default:
					Debug.Assert(false, "Unbekannter Medientyp");
					break;
			}
		}

	}

}
