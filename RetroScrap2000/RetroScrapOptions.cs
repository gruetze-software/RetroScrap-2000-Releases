using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class RetroScrapOptions
	{
		public string? RomPath { get; set; }
		public string? Language { get; set; }
		public string? Region { get; set; }
		public bool? ScanRomStartup { get; set; }
		public bool? Logging { get; set; }
		public string? ApiUser { get; set; }
		public bool? MediaBoxImage { get; set; }
		public bool? MediaBox2DFront { get; set; }
		public bool? MediaBox3DFront { get; set; }
		public bool? MediaBoxSide { get; set; }
		public bool? MediaBoxBack { get; set; }
		public bool? MediaBoxTextures { get; set; }
		public bool? MediaBoxMix1 { get; set; }
		public bool? MediaBoxMix2 { get; set; }
		public bool? MediaVideo { get; set; }
		public bool? MediaMarquee { get; set; }
		public bool? MediaFanart { get; set; }
		public bool? MediaScreenshot { get; set; }
		public bool? MediaScreenshotTitle { get; set; }
		public bool? MediaWheel { get; set; }
		public bool? MediaWheelSteel { get; set; }
		public bool? MediaWheelCarbon { get; set; }
		public bool? MediaManual { get; set; }
		public bool? MediaMap { get; set; }
		public List<MediaManualSystem> MediaManualSystemList { get; set; } = new();


		[JsonIgnore]
		public PasswordVault? Secret { get; set; }
		public RetroSystems Systems { get; internal set; } = new();

		public RetroScrapOptions() 
		{ 
			Secret = new PasswordVault(Path.Combine(GetOptionsPath(), "vault.dat"));
			MediaBoxImage = true;
			MediaBox2DFront = true;
			MediaScreenshot = true;
			MediaVideo = true;
			Region = "eu";
		}

		public static GameMediaSettings? GetMediaSettings(eMediaType mediaType)
		{
			return GetMediaSettingsList().FirstOrDefault(x => x.Type == mediaType);
		}

		public static List<GameMediaSettings> GetMediaSettingsList()
		{
			return new List<GameMediaSettings>
			{
				{ new GameMediaSettings(eMediaType.ScreenshotTitle, "sstitle", "title" ) },
				{ new GameMediaSettings(eMediaType.ScreenshotGame, "ss", "screenshot") },
				{ new GameMediaSettings(eMediaType.Fanart, "fanart", "fanart") },
				{ new GameMediaSettings(eMediaType.Video, "video", "video") },
				{ new GameMediaSettings(eMediaType.Marquee, "screenmarquee", "marquee") },
				{ new GameMediaSettings(eMediaType.Manual, "manuel", "manual") },
				{ new GameMediaSettings(eMediaType.Map, "map", "map") },
				{ new GameMediaFront(eMediaBoxFrontType.TwoDim) },
				{ new GameMediaFront(eMediaBoxFrontType.ThreeDim) },
				{ new GameMediaFront(eMediaBoxFrontType.Mix1) },
				{ new GameMediaFront(eMediaBoxFrontType.Mix2) },
				{ new GameMediaSettings(eMediaType.BoxImageSide, "box-2D-side", "side") },
				{ new GameMediaSettings(eMediaType.BoxImageBack, "box-2D-back", "back") },
				{ new GameMediaSettings(eMediaType.BoxImageTexture, "box-texture", "texture") },
				{ new GameMediaWheel(eMediaWheelType.Normal) },
				{ new GameMediaWheel(eMediaWheelType.Steel) },
				{ new GameMediaWheel(eMediaWheelType.Carbon) },
			};
		}

		public bool IsMediaTypeEnabled(GameMediaSettings media)
		{
			if (media.Type == eMediaType.Wheel)
			{
				GameMediaWheel wheel = (GameMediaWheel)media;
				if ( wheel.WheelType == eMediaWheelType.Normal )
					return this.MediaWheel.HasValue && this.MediaWheel.Value == true ? true : false;
				else if (wheel.WheelType == eMediaWheelType.Steel)
					return this.MediaWheelSteel.HasValue && this.MediaWheelSteel.Value == true ? true : false;
				if (wheel.WheelType == eMediaWheelType.Carbon)
					return this.MediaWheelCarbon.HasValue && this.MediaWheelCarbon.Value == true ? true : false;
				else
					return false;
			}
			else if (media.Type == eMediaType.Fanart) return this.MediaFanart.HasValue && this.MediaFanart.Value == true ? true : false;
			else if (media.Type == eMediaType.Marquee) return this.MediaMarquee.HasValue && this.MediaMarquee.Value == true ? true : false;
			else if (media.Type == eMediaType.Map) return this.MediaMap.HasValue && this.MediaMap.Value == true ? true : false;
			else if (media.Type == eMediaType.Manual) return this.MediaManual.HasValue && this.MediaManual.Value == true ? true : false;
			else if (media.Type == eMediaType.Video) return this.MediaVideo.HasValue && this.MediaVideo.Value == true ? true : false;
			else if (media.Type == eMediaType.ScreenshotTitle) return this.MediaScreenshotTitle.HasValue && this.MediaScreenshotTitle.Value == true ? true : false;
			else if (media.Type == eMediaType.ScreenshotGame) return this.MediaScreenshot.HasValue && this.MediaScreenshot.Value == true ? true : false;
			else if (media.Type == eMediaType.BoxImageFront) 
			{
				if (!this.MediaBoxImage.HasValue || this.MediaBoxImage.Value == false)
					return false;

				GameMediaFront front = (GameMediaFront)media;
				if ( front.FrontType == eMediaBoxFrontType.TwoDim )
					return this.MediaBox2DFront.HasValue && this.MediaBox2DFront.Value == true ? true : false;
				else if (front.FrontType == eMediaBoxFrontType.ThreeDim )
					return this.MediaBox3DFront.HasValue && this.MediaBox3DFront.Value == true ? true : false;
				else if (front.FrontType == eMediaBoxFrontType.Mix1)
					return this.MediaBoxMix1.HasValue && this.MediaBoxMix1.Value == true ? true : false;
				else if (front.FrontType == eMediaBoxFrontType.Mix2)
					return this.MediaBoxMix2.HasValue && this.MediaBoxMix2.Value == true ? true : false;
				else return false;
			}
			else if (media.Type == eMediaType.BoxImageBack) return this.MediaBoxBack.HasValue && this.MediaBoxBack.Value == true ? true : false;
			else if (media.Type == eMediaType.BoxImageSide) return this.MediaBoxSide.HasValue && this.MediaBoxSide.Value == true ? true : false;
			else if (media.Type == eMediaType.BoxImageTexture) return this.MediaBoxTextures.HasValue && this.MediaBoxTextures.Value == true ? true : false;
			else return false;
		}
		private static string GetOptionsPath()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var dir = Path.Combine(appData, "RetroScrap2000");
			Directory.CreateDirectory(dir); // sicherstellen, dass Ordner existiert
			return dir; 
		}

		private static string GetOptionsFile()
		{
			return Path.Combine(GetOptionsPath(), "options.json");
		}

		public static string GetLoggingFile()
		{
			return Path.Combine(GetOptionsPath(), "logs", "retroscrap.log");
		}

		public string GetLanguageShortCode()
		{
			if (string.IsNullOrEmpty(Language))
				return "en"; // default
			if ( Language.Contains('-') == false )
				return Language.ToLower(); // already short code
			
			var parts = Language.Split('-');
			return parts[0];
		}

		/// <summary>
		/// Speichert die Optionen als Json-Datei.
		/// </summary>
		public void Save()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true // hübsch formatiert
			};
			var json = JsonSerializer.Serialize(this, options);
			File.WriteAllText(GetOptionsFile(), json);
		}

		/// <summary>
		/// Lädt Optionen aus einer Json-Datei. 
		/// Falls die Datei nicht existiert, wird ein neues Objekt zurückgegeben.
		/// </summary>
		public static RetroScrapOptions Load()
		{
			if (!File.Exists(GetOptionsFile()))
				return new RetroScrapOptions();

			var json = File.ReadAllText(GetOptionsFile());
			var loadobj = JsonSerializer.Deserialize<RetroScrapOptions>(json) ?? new RetroScrapOptions();
			// Aufräumen, machen wir vorsichtshalber immer
			if (loadobj.MediaBoxImage == null || loadobj.MediaBoxImage == false)
			{
				loadobj.MediaBoxMix1 = false;
				loadobj.MediaBoxMix2 = false;
				loadobj.MediaBoxBack = false;
				loadobj.MediaBoxSide = false;
				loadobj.MediaBoxTextures = false;
			}
			else if (loadobj.MediaBoxImage == true)
			{
				// 3D oder 2D muss aktiv sein
				if ( (loadobj.MediaBox2DFront == null || loadobj.MediaBox2DFront == false )
					&& (loadobj.MediaBox3DFront == null || loadobj.MediaBox3DFront == false ) )
					loadobj.MediaBox2DFront = true;
			}

			if (loadobj.MediaWheel == null || loadobj.MediaWheel == false)
			{
				loadobj.MediaWheelCarbon = false;
				loadobj.MediaWheelSteel = false;
			}
			
			return loadobj;
		}
	}

	public class MediaManualSystem
	{
		public string Name { get; set; } = string.Empty;
		public string AbsolutPathMedia { get; set; } = string.Empty;
		public string? RelPathMediaToRomPath { get; set; }
		public string? MediaExtensionFilter { get; set; }
		public int? RomSystemID { get; set; }
		public string XmlKeyName { get; set; } = string.Empty;

		public override string ToString()
		{
			return Name ?? "[Empty MediaManualSystem]";
		}

		public void CopyFrom(MediaManualSystem other)
		{
			// Alle Properties von 'other' auf 'this' kopieren
			this.AbsolutPathMedia = other.AbsolutPathMedia;
			this.Name = other.Name;
			this.MediaExtensionFilter = other.MediaExtensionFilter;
			this.RomSystemID = other.RomSystemID;
			this.XmlKeyName = other.XmlKeyName;
			this.RelPathMediaToRomPath = other.RelPathMediaToRomPath;
		}

	}
}
