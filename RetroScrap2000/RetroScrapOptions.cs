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
		public List<RetroSystem> RetroSysteme { get; set; } = new List<RetroSystem>();
		public string? ApiUser { get; set; }
		[JsonIgnore]
		public PasswordVault? Secret { get; set; }

		public RetroScrapOptions() 
		{ 
			Secret = new PasswordVault(Path.Combine(GetOptionsPath(), "vault.dat"));
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
