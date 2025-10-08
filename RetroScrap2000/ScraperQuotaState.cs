using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class ScraperQuotaState
	{
		public int LastReportedRequestsToday { get; set; } = 0;
		public int MaxRequestsPerDay { get; set; } = 0;
		public DateTime? LastUsageDate { get; set; } = null;

		private static string GetQuotaFile()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var dir = Path.Combine(appData, "RetroScrap2000");
			Directory.CreateDirectory(dir); // sicherstellen, dass Ordner existiert
			return Path.Combine(dir, "scraperQuota.json");
		}

		public void Save()
		{
			Trace.WriteLine("[ScraperQuotaState::Save()]");
			var options = new JsonSerializerOptions
			{
				WriteIndented = true // hübsch formatiert
			};
			var json = JsonSerializer.Serialize(this, options);
			File.WriteAllText(GetQuotaFile(), json);
		}

		/// <summary>
		/// Lädt die Retrosysteme aus einer Json-Datei. 
		/// Falls die Datei nicht existiert, wird ein neues Objekt zurückgegeben.
		/// </summary>
		public static ScraperQuotaState? Load()
		{
			Trace.WriteLine("[ScraperQuotaState::Load()]");
			var file = GetQuotaFile();
			if (!File.Exists(file))
			{
				return null;
			}

			var json = File.ReadAllText(GetQuotaFile());
			return JsonSerializer.Deserialize<ScraperQuotaState>(json);
		}

	}
}
