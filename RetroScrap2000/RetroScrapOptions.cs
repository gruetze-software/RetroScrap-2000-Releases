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
	public enum eMediaType
	{
		BoxImage = 0,
		Video,
		Marquee,
		Fanart,
		Screenshot,
		Wheel,
		Manual,
		Map,
		Unknown
	}

	public class RetroScrapOptions
	{
		public string? RomPath { get; set; }
		public string? Language { get; set; }
		public string? Region { get; set; }
		public string? ApiUser { get; set; }
		public bool? MediaBoxImage { get; set; }
		public bool? MediaVideo { get; set; }
		public bool? MediaMarquee { get; set; }
		public bool? MediaFanart { get; set; }
		public bool? MediaScreenshot { get; set; }
		public bool? MediaWheel { get; set; }
		public bool? MediaManual { get; set; }
		public bool? MediaMap { get; set; }


		[JsonIgnore]
		public PasswordVault? Secret { get; set; }

		public RetroScrapOptions() 
		{ 
			Secret = new PasswordVault(Path.Combine(GetOptionsPath(), "vault.dat"));
			MediaBoxImage = true;
			MediaScreenshot = true;
			MediaVideo = true;
			Region = "eu";
		}

		public static string GetMediaFolderAndXmlTag(eMediaType type)
		{
			return type switch
			{
				eMediaType.BoxImage => "image",
				eMediaType.Video => "video",
				eMediaType.Marquee => "marquee",
				eMediaType.Fanart => "fanart",
				eMediaType.Screenshot => "screenshot",
				eMediaType.Wheel => "wheel",
				eMediaType.Manual => "manual",
				eMediaType.Map => "map",
				_ => "unknown",
			};
		}

		public bool IsMediaTypeEnabled(eMediaType type)
		{
			return type switch
			{
				eMediaType.BoxImage => MediaBoxImage ?? false,
				eMediaType.Video => MediaVideo ?? false,
				eMediaType.Marquee => MediaMarquee ?? false,
				eMediaType.Fanart => MediaFanart ?? false,
				eMediaType.Screenshot => MediaScreenshot ?? false,
				eMediaType.Wheel => MediaWheel ?? false,
				eMediaType.Manual => MediaManual ?? false,
				eMediaType.Map => MediaMap ?? false,
				_ => false,
			};
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
			return JsonSerializer.Deserialize<RetroScrapOptions>(json) ?? new RetroScrapOptions();
		}
	}
}
