using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class UpdateChecker
	{
		private static string _githuburl = "https://api.github.com/repos/gruetze-software";
		private static string _githubrepo = "RetroScrap-2000-Releases";
		public UpdateChecker() 
		{
			
		}

		public string? DownloadUrl { get; private set; }

		public async Task<(bool update, string? newversion)> CheckForNewVersion()
		{
			var resp = await GetLatestGitHubVersion();

			if (resp == null)
				return (false, null);

			var gitInfo = ParseTagName(resp);
			if (string.IsNullOrEmpty(gitInfo.version))
				return (false, null);
			
			DownloadUrl = gitInfo.zipurl;

			// 1. Unnötige Präfixe und Suffixe entfernen
			string gitVersion = gitInfo.version!;
			int startIndex = 0;
			while (startIndex < gitVersion.Length && !char.IsDigit(gitVersion[startIndex]))
				startIndex++;
			string cleanedVersion = gitVersion.Substring(startIndex);

			// 2. Ersten Teil des Strings bis zum ersten nicht-numerischen Zeichen extrahieren
			// Dies ist nützlich, wenn das Format nicht immer konsistent ist.
			var versionParts = new List<char>();
			foreach (char c in cleanedVersion)
			{
				if (char.IsDigit(c) || c == '.')
				{
					versionParts.Add(c);
				}
				else
				{
					break; // Stoppe beim ersten Buchstaben
				}
			}

			string finalVersionString = new string(versionParts.ToArray());
			if (finalVersionString.Length > 0)
			{
				// 3. Jetzt kann der Versions-String in ein Version-Objekt umgewandelt werden
				if (Version.TryParse(finalVersionString, out var newversion))
				{
					var oldversion = new Version(Utils.GetAppInfo().ProductVersion);
					if (newversion > oldversion)
						return (true, finalVersionString);
				}
			}

			return (false, null);
		}

		private async Task<string?> GetLatestGitHubVersion()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroScrap2000");

			try
			{
				Trace.WriteLine($"Checking for new version at {_githuburl}/{_githubrepo}/releases/latest");

				var response = await client.GetStringAsync($"{_githuburl}/{_githubrepo}/releases/latest");
				// Der response-String ist ein JSON-Objekt
				return response;
			}
			catch (HttpRequestException e)
			{
				Trace.WriteLine("Exception!!! [UpdateChecker::GetLatestGitHubVersion()] " + Utils.GetExcMsg(e));
				// Fehlerbehandlung bei fehlgeschlagener Anfrage
				return null;
			}
		}

		private (string? version, string? zipurl) ParseTagName(string jsonResponse)
		{
			if (string.IsNullOrEmpty(jsonResponse))
				return (null, null);

			string? version = null;
			string? zipurl = null;

			using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
			{
				
				JsonElement root = doc.RootElement;
				// Tag-Name enthält die Versionsnummer
				if (root.TryGetProperty("tag_name", out JsonElement tagNameElement))
				{
					version = tagNameElement.GetString();
				}
				// Unter assets finden wir den Download-Link
				if (root.TryGetProperty("assets", out JsonElement assets) && assets.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement asset in assets.EnumerateArray())
					{
						// 1. Prüfen, ob das Element 'name' existiert und unseren Dateinamen enthält
						if (asset.TryGetProperty("name", out JsonElement nameElement) &&
								nameElement.GetString() != null && nameElement.GetString()!.EndsWith("zip") )
						{
							// 2. Das korrekte Asset wurde gefunden: Jetzt die Download-URL extrahieren
							if (asset.TryGetProperty("browser_download_url", out JsonElement urlElement) &&
									urlElement.ValueKind == JsonValueKind.String)
							{
								zipurl = urlElement.GetString();
								// Wenn die URL gefunden wurde, beenden wir die Schleife
								break;
							}
						}
					}
				}
			}
			return (version, zipurl);
		}
	}
}
