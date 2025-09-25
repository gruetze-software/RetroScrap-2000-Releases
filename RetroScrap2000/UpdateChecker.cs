using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class UpdateChecker
	{
		private static string _githuburl = "https://api.github.com/repos/gruetze-software";
		private static string _githubrepo = "RetroScrap-2000-Releases";
		public UpdateChecker() { }

		public string DownloadUrl { get { return $"https://github.com/gruetze-software/{_githubrepo}"; } }

		public async Task<(bool update, string? newversion)> CheckForNewVersion()
		{
			var resp = await GetLatestGitHubVersion();
			if (resp == null)
				return (false, null);

			var gitVersion = ParseTagName(resp);
			if (string.IsNullOrEmpty(gitVersion))
				return (false, null);

			// 1. Unnötige Präfixe und Suffixe entfernen
			string cleanedVersion = gitVersion.TrimStart("Release_".ToCharArray());
			cleanedVersion = cleanedVersion.TrimEnd("_beta".ToCharArray());

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

		private string? ParseTagName(string jsonResponse)
		{
			if (string.IsNullOrEmpty(jsonResponse))
				return null;

			using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
			{
				JsonElement root = doc.RootElement;
				if (root.TryGetProperty("tag_name", out JsonElement tagNameElement))
				{
					return tagNameElement.GetString();
				}
			}
			return null;
		}
	}
}
