using RetroScrap2000;
using RetroScrap2000.Tools;
using Serilog;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Mime;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using static RetroScrap2000.ScraperManager;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RetroScrap2000
{
	/// <summary>
	/// ScrapperManager für ScreenScraper.fr
	/// </summary>
	public class ScraperManager
	{
		private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
		private DeveloperVault _devVault = new DeveloperVault();
		private string _soft = Utils.GetAppInfo().ProductName.Replace(" ", "");
		private string? _ssid, _sspw;

		private const string BaseUrl = "https://api.screenscraper.fr/api2/";
		private static readonly JsonSerializerOptions _jsonOpts = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public SsUser? LatestSsUser { get; set; }
		private ScraperQuotaState? _quotaState;
		public ScraperManager()
		{
			if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
				_http.DefaultRequestHeaders.Add("User-Agent", $"{_soft}");

			var saveState = ScraperQuotaState.Load();
			this._quotaState = saveState;
			if (_quotaState != null
				&& _quotaState.LastUsageDate.HasValue && _quotaState.LastUsageDate.Value < DateTime.Now.Date)
				// Zähler zurücksetzen
				_quotaState.LastReportedRequestsToday = 0;

		}

		public void RefreshSecrets(string ssid, string ssPassword)
		{
			_ssid = ssid; 
			_sspw = ssPassword;
		}

		private string BuildAuthQuery() => $"devid={getDD().s1}&devpassword={getDD().s2}&softname={_soft}&ssid={_ssid}&sspassword={_sspw}&output=json";
		private string BuildAuthQueryWithOutSs() => $"devid={getDD().s1}&devpassword={getDD().s2}&softname={_soft}&output=json";

		public async Task<SsUserInfosResponse> FetchSsUserInfosAsync()
		{
			Log.Information($"FetchSsUserInfosAsync() Call Api {BaseUrl}ssuserInfos.php?xxxxxxx");
			var url = $"{BaseUrl}ssuserInfos.php?{BuildAuthQuery()}";
			Log.Debug(url);
			
			using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			if (!resp.IsSuccessStatusCode)
			{
				// einige typische Codes lt. Doku:
				// 403 = Dev-Login falsch, 401 = API für Nicht-Mitglieder (Server Last), 429/430 = Limits/Quota
				throw new ScreenScraperApiException((int)resp.StatusCode, $"HTTP {(int)resp.StatusCode} ({resp.ReasonPhrase})");
			}

			var text = await resp.Content.ReadAsStringAsync();
			await using var s = await resp.Content.ReadAsStreamAsync();
			var obj = await JsonSerializer.DeserializeAsync<SsUserInfosResponse>(
				s,
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					NumberHandling = JsonNumberHandling.AllowReadingFromString
				});

			// Minimalvalidierung
			if (obj?.response is null || obj.response.ssuser is null)
				throw new ScreenScraperApiException(0, Properties.Resources.Txt_Api_Err_UserFail);

			return obj;
		}

		public void UpdateStateAndSsUser(SsUser? ssUser)
		{
			if (ssUser == null)
				return;

			Log.Debug("[ScrapperManager::UpdateStateAndSsUser()]");
			LatestSsUser = ssUser;

			// Speichern des aktuellen Status
			if (_quotaState == null)
				_quotaState = new ScraperQuotaState();

			_quotaState.LastReportedRequestsToday = LatestSsUser.requeststoday ?? 0;
			_quotaState.MaxRequestsPerDay = LatestSsUser.maxrequestsperday ?? 0;

			// Speichern des aktuellen Datums und der Uhrzeit
			_quotaState.LastUsageDate = DateTime.Now;

			// Den aktualisierten Zustand sofort speichern, um ihn für den nächsten Start zu sichern
			_quotaState.Save();
		}

		public async Task WaitForRateLimitAndAddRequestCounter()
		{
			if (LatestSsUser == null || LatestSsUser.maxrequestspermin == null)
			{
				// Fallback-Drosselung, falls noch kein SsUser geladen wurde
				Log.Debug($"[ScrapperManager::WaitForRateLimitAndAddRequestCounter()] - 1000ms, SsUser is null");
				await Task.Delay(1000);
				return;
			}

			Log.Debug($"[ScrapperManager::WaitForRateLimitAndAddRequestCounter()] - MaxRequestsForDay: {LatestSsUser.maxrequestsperday} - Today: {_quotaState!.LastReportedRequestsToday}");
			if (_quotaState!.LastReportedRequestsToday >= LatestSsUser.maxrequestsperday)
			{
				throw new Exception(Properties.Resources.Txt_Log_DailyLimitReached);
			}

			// Berechnung der Wartezeit (wie zuvor: 60s / maxReq + Puffer)
			int delayMs = GetCalculatedDelay(LatestSsUser.maxrequestspermin.Value);
			await Task.Delay(delayMs);

			// Interne Zählung hochsetzen
			_quotaState!.LastReportedRequestsToday++;
		}

		public static int GetCalculatedDelay(int? val)
		{
			if (val == null || val.Value <= 0)
				return 1000; // Standard-Fallback, z.B. 1 Sekunde

			// Berechnung: 60 Sekunden * 1000 ms / maxrequestspermin
			// Wir fügen einen Puffer von 10% hinzu, um auf der sicheren Seite zu sein.
			double baseDelay = 60000.0 / val.Value;
			return (int)(baseDelay * 1.1); // Warten Sie 10% länger als nötig
		}

		/// <summary>
		/// Liefert (ok, daten, fehler). Bei ok=false ist daten leer und fehler enthält den Grund.
		/// </summary>
		public async Task<(bool ok, Systeme[] data, string? error)> GetSystemsAsync(CancellationToken ct = default)
		{
			Log.Information($"GetSystemsAsync() Call Api {BaseUrl}systemesListe.php?xxxxxxx");
			var url = $"{BaseUrl}systemesListe.php?{BuildAuthQueryWithOutSs()}";
			Log.Debug(url);
			try
			{
				using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
				var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

				if (!resp.IsSuccessStatusCode)
					return (false, Array.Empty<Systeme>(), $"HTTP {(int)resp.StatusCode}: {body}");

				// Deserialisieren mit Wrapper
				SystemeRoot? root = JsonSerializer.Deserialize<SystemeRoot>(body, _jsonOpts);
				var systems = root?.response?.systemes ?? Array.Empty<Systeme>();

				// API-interner Status prüfen (falls vorhanden)
				var status = root?.response?.ssstatus;
				if (systems.Length == 0)
				{
					var msg = status?.message ?? "Empty Response";
					return (false, systems, msg);
				}

				// alles gut
				return (true, systems, null);
			}
			catch (TaskCanceledException)
			{
				return (false, Array.Empty<Systeme>(), "Timeout");
			}
			catch (Exception ex)
			{
				return (false, Array.Empty<Systeme>(), $"Unknown Failure: {ex.Message}");
			}
		}

		/// <summary>
		/// Lädt die Icons und Banner aller Retro-Systeme herunter
		/// </summary>
		/// <param name="systems"></param>
		/// <param name="delayMs"></param>
		/// <returns></returns>
		public static async Task DownloadSystemImagesAsync(
				IEnumerable<Systeme> systems,
				int delayMs = 500)
		{
			DirectoryInfo diricons = new DirectoryInfo(RetroSystems.FolderIcons);
			if ( !diricons.Exists )
				Directory.CreateDirectory(diricons.FullName);

			DirectoryInfo dirwheels = new DirectoryInfo(RetroSystems.FolderBanner);
			if (!dirwheels.Exists)
				Directory.CreateDirectory(dirwheels.FullName);

			foreach (var sys in systems)
			{
				var batfoldername = BatoceraFolders.MapToBatoceraFolder(sys.noms);
				if (string.IsNullOrEmpty(batfoldername))
				{
					Log.Debug("[ScrapperManager::DownloadSystemImagesAsync()] Skip " + sys.Name_eu);
					continue;
				}
				// beide Typen (icon, wheel) sichern
				if (sys.Media.icon != null)
				{
					string file = Path.Combine(diricons.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Log.Debug($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Dowmload Icon \"{batfoldername}\"");
						await DownloadOne(sys.Media.icon, file);
					}
					else
					{
						Log.Debug($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Skip Icon \"{batfoldername}\" (Always exist)");
					}
				}
				if (sys.Media.wheel != null)
				{
					string file = Path.Combine(dirwheels.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Log.Debug($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Dowmload Wheel \"{batfoldername}\"");
						await DownloadOne(sys.Media.wheel, file);
					}
					else
					{
						Log.Debug($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Skip Wheel \"{batfoldername}\" (Always exist)");
					}
				}
				// kleine Pause, um die API nicht zu überlasten
				await Task.Delay(delayMs);
			}
		}

		private static async Task DownloadOne(string url, string path)
		{
			try
			{
				string urllogstring = url.Substring(0, url.IndexOf("?") + 1) + "xxxxxx";
				Log.Information($"ScrapperManager::DownloadOne(\"{urllogstring}\")");
				var data = await _http.GetByteArrayAsync(url);
				Log.Debug(url);
				await File.WriteAllBytesAsync(path, data);
			}
			catch (Exception ex)
			{
				Log.Debug($"Fail Download {url}: {Utils.GetExcMsg(ex)}");
			}
		}

		public async Task<ScrapRechercheApiResponse> GetGameRechercheListAsync(
			string? romFileName, int systemId, RetroScrapOptions opt, CancellationToken ct = default)
		{

			ScrapRechercheApiResponse retVal = new ScrapRechercheApiResponse();

			if (string.IsNullOrEmpty(romFileName))
				return retVal;

			Log.Information($"GetGameRechercheListAsync(\"{romFileName}\") Call Api {BaseUrl}jeuRecherche.php?xxxxxxx");
			var url = $"{BaseUrl}jeuRecherche.php?{BuildAuthQuery()}&systemeid={systemId}&recherche={Uri.EscapeDataString(romFileName)}";
			Log.Debug(url);
			await WaitForRateLimitAndAddRequestCounter();
			using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
			var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			if (!resp.IsSuccessStatusCode)
			{
				retVal.HttpCode = (int)resp.StatusCode;
				if (retVal.HttpCode == 404)
					retVal.Error = Properties.Resources.Txt_Msg_Scrap_NoDataFound;
				else
					retVal.Error = body;
				return retVal;
			}
			// JSON?
			var head = body.TrimStart();
			if (!(head.StartsWith("{") || head.StartsWith("[")))
			{
				retVal.Error = $"{Properties.Resources.Txt_Log_Scrap_NoJson}: {head[..Math.Min(120, head.Length)]}";
				return retVal;
			}

			var root = System.Text.Json.JsonSerializer.Deserialize<GameRechercheRoot>(body, _jsonOpts);
			// Aktualisieren der Scraper-Steuerung mit den NEUESTEN Werten
			UpdateStateAndSsUser(root?.response?.ssuser);

			var jeux = root?.response?.jeux;
			if (jeux == null)
			{
				retVal.HttpCode = 404;
				retVal.Error = Properties.Resources.Txt_Msg_Scrap_NoDataFound;
				return retVal;
			}
				
			var lst = jeux.ToList();
			if (lst.Count == 1 && ( lst[0] == null || lst[0].noms == null))
			{
				retVal.HttpCode = 404;
				retVal.Error = Properties.Resources.Txt_Msg_Scrap_NoDataFound;
				return retVal;
			}
			else
			{
				retVal.Ok = true;
				retVal.RechercheResult = lst;
				return retVal;
			}
		}


		public string? GetApiRomIDwhereMatch(string localFilePath, List<ApiRom> apiRoms)
		{
			// 1. Lokale Prüfsummen berechnen
			var localHashes = FileTools.CalculateChecksums(localFilePath);

			// Wenn die lokale Datei nicht gehasht werden konnte, brechen Sie ab.
			if (string.IsNullOrEmpty(localHashes.MD5))
			{
				Log.Warning($"Could not calculate Hashes for \"{Path.GetFileName(localFilePath)}\".");
				return null;
			}

			// 2. Abgleich mit dem 'roms'-Array der API
			// Wir verwenden einen Fall-Through-Vergleich (MD5 > SHA1 > CRC), um die beste Übereinstimmung zu finden.

			foreach (var apiRom in apiRoms)
			{
				// 2a. Vergleich über MD5 (höchste Priorität, da robust)
				if (!string.IsNullOrEmpty(localHashes.MD5) &&
						string.Equals(localHashes.MD5, apiRom.rommd5, StringComparison.OrdinalIgnoreCase))
				{
					Log.Information($"Perfect MD5-Match found for \"{Path.GetFileName(localFilePath)}\"." );
					return apiRom.id; // Match gefunden
				}

				// 2b. Vergleich über SHA1 (gute Alternative)
				if (!string.IsNullOrEmpty(localHashes.SHA1) &&
						string.Equals(localHashes.SHA1, apiRom.romsha1, StringComparison.OrdinalIgnoreCase))
				{
					Log.Information($"SHA1-Match found for \"{Path.GetFileName(localFilePath)}\".");
					return apiRom.id; // Match gefunden, Scraper kann fortfahren
				}

				// 2c. Vergleich über CRC32 (niedrigste Priorität, da Kollisionen möglich)
				// Dies erfordert, dass wir lokale CRC32s berechnen und die API-Daten prüfen.
				// if (localHashes.CRC32 == apiRom.rom_crc) { return true; } 
			}

			// 3. Kein Match gefunden
			Log.Warning($"No Hash-Match found for \"{Path.GetFileName(localFilePath)}\"." );

			return null;
		}

		public async Task<(ScrapGameApiResponse resp, List<MediaDownloadJob>? mediaJobs)> GetGameAsync(GameEntry? game, int gameid, int systemid, 
			FileInfo? romFile, RetroScrapOptions opt, CancellationToken ct = default)
		{
			return await GetGameAsync(game, romFile, systemid, opt, gameid, ct);
		}

		/// <summary>
		/// Scrapt ein Spiel. Über den
		/// </summary>
		/// <param name="romFileName">Name des Spiels</param>
		/// <param name="systemId">Die Id des Systems</param>
		/// <param name="ct"></param>
		/// <returns></returns>
		public async Task<(ScrapGameApiResponse resp, List<MediaDownloadJob>? mediaJobs)> GetGameAsync(
				GameEntry? game, FileInfo? romFile, int systemId, RetroScrapOptions opt, int gameid = 0, CancellationToken ct = default)
		{
			(ScrapGameApiResponse resp, List<MediaDownloadJob>? mediaJobs) retVal = (new ScrapGameApiResponse(), null);
			if ( (romFile == null || !romFile.Exists ) )
			{
				retVal.resp.Error = Properties.Resources.Txt_Log_Scrap_FileNotExist;
				return retVal;
			}

			string url = string.Empty;
			ct.ThrowIfCancellationRequested();

			////////////////////////////////////////////////////////////////////////
			// Aufruf - Id

			if (gameid > 0)
			{
				Log.Information($"Call with Game-Id {gameid} and System-Id {systemId}: GetGameAsync() -> Call Api {BaseUrl}jeuInfos.php?xxxxxxx");
				url = $"{BaseUrl}jeuInfos.php?{BuildAuthQuery()}&systemeid={systemId}&gameid={gameid}" +
					$"&romtaille={romFile.Length}&romnom={Uri.EscapeDataString(romFile.Name)}";
				Log.Debug(url);
			}
			else
			{
				// Aufruf - Hash
				///////////////////////////////////////////////////////////////////////
				var hashes = FileTools.CalculateChecksums(romFile.FullName);
				if (string.IsNullOrEmpty(hashes.MD5) || string.IsNullOrEmpty(hashes.SHA1))
				{
					retVal.resp.Error = Properties.Resources.Txt_Msg_Scrap_NoHashesFromFile;
					return retVal;
				}

				Log.Information($"Call with Hashes, Size and Name of File: GetGameAsync() -> Call Api {BaseUrl}jeuInfos.php?xxxxxxx");
				url = $"{BaseUrl}jeuInfos.php?{BuildAuthQuery()}&systemeid={systemId}&" +
					$"md5={hashes.MD5}&sha1={hashes.SHA1}&romtaille={romFile.Length}&romnom={Uri.EscapeDataString(romFile.Name)}";
				Log.Debug(url);
			}
			

			string? body = null;
			await WaitForRateLimitAndAddRequestCounter();
			using (var resp = await _http.GetAsync(url, ct).ConfigureAwait(false))
			{
				body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

				if (!resp.IsSuccessStatusCode)
				{
					retVal.resp.HttpCode = (int)resp.StatusCode;
					if (retVal.resp.HttpCode == 404)
						retVal.resp.Error = Properties.Resources.Txt_Msg_Scrap_NoDataFound;
					else
						retVal.resp.Error = body;
					return retVal;
				}
			}

			// JSON?
			var head = body!.TrimStart();
			if (!(head.StartsWith("{") || head.StartsWith("[")))
			{
				retVal.resp.Error = $"{Properties.Resources.Txt_Log_Scrap_NoJson}: {head[..Math.Min(120, head.Length)]}";
				return retVal;
			}

			var root = System.Text.Json.JsonSerializer.Deserialize<GameRoot>(body, _jsonOpts);

			// Aktualisieren der Scraper-Steuerung mit den NEUESTEN Werten
			UpdateStateAndSsUser(root?.response?.ssuser);

			var jeu = root?.response?.jeu;
			if (jeu == null)
			{
				retVal.resp.Error = Properties.Resources.Txt_Log_Scrap_NoName;
				return retVal;
			}

			var sg = new GameScrap();
			sg.Id = jeu.romid;

			// Name 
			if (jeu.noms != null && jeu.noms.Length > 0)
			{
				var euname = jeu.noms.FirstOrDefault(x => x.region != null && x.region == opt.Region);
				if (euname != null)
				{
					sg.Name = euname.text;
				}
				else
				{
					sg.Name = jeu.noms[0].text;
				}
			}
			// Description
			if (jeu.synopsis != null && jeu.synopsis.Length > 0)
			{
				var desc = jeu.synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == opt.GetLanguageShortCode());
				if (desc != null)
				{
					sg.Description = desc.text;
				}
				else
				{
					// Default ist englisch
					desc = jeu.synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == "en");
					if (desc != null)
						sg.Description = desc.text;
				}
			}
			// Genre
			if (jeu.genres != null && jeu.genres.Length > 0)
			{
				var gen = jeu.genres.LastOrDefault(x => x.noms != null && x.noms.Length > 0);
				if (gen != null)
				{
					var degen = gen.noms?.FirstOrDefault(x => x.langue == opt.GetLanguageShortCode());
					if (degen != null)
					{
						sg.Genre = degen.text;
					}
					else
					{
						degen = gen.noms?.FirstOrDefault(x => x.langue == "en");
						sg.Genre = degen?.text;
					}
				}
			}
			// Anzahl Player
			sg.Players = jeu.joueurs?.text;
			// Entwicklerstudio
			sg.Developer = jeu.developpeur?.text;
			// Publisher
			sg.Publisher = jeu.editeur?.text;
			// Rating
			if (jeu.note != null && jeu.note.text != null && double.TryParse(jeu.note.text,
				NumberStyles.Float, CultureInfo.InvariantCulture, out var note) && note > 0.0)
				sg.RatingNormalized = note / 20.0;  // Rating hier zwischen 1 und 20

			// Releasedatum
			if (jeu.dates != null && jeu.dates.Length > 0)
			{
				var dd = jeu.dates.FirstOrDefault(x => x.region != null && x.region == opt.Region);
				if (dd == null)
					dd = jeu.dates.FirstOrDefault(x => x.region != null);
				sg.ReleaseDateRaw = dd?.text;
			}

			////////////////////////////////////////////////////////////////////////
			// Medien
			///////////////////////////////////////////////////////////////////////
			var mediaJobs = GetMediaJobs(sg, game!, romFile, jeu.medias, opt, ct);

			retVal.resp.Ok = true;
			retVal.resp.ScrapGameResult = sg;
			retVal.mediaJobs = mediaJobs;
			return retVal;
		}

		public List<MediaDownloadJob> GetMediaJobs(GameScrap sg, GameEntry game, FileInfo romFile, 
			Medium[]? medias, RetroScrapOptions opt, CancellationToken ct)
		{
			var medien = sg.PossibleMedien.Where(m => opt.IsMediaTypeEnabled(m));
			// Zunächst lokale Dateien prüfen und zuordnen
			FileTools.SetLocalMediaFilesToGame(game, romFile.Directory!.FullName);
			List<MediaDownloadJob> mediaJobs = new List<MediaDownloadJob>();
			foreach (var medium in medien)
			{
				medium.Url = GetMediumUrl(medias, medium.ApiKey, opt) ?? string.Empty;
				if (!string.IsNullOrEmpty(medium.Url))
				{
					// Es gibt eine Mediendatei online, dann prüfen wir, ob es eine identische lokale Datei
					// gibt und setzen als Url dann den Pfad zur lokalen Datei.
					var fileNameWithoutExtension = Utils.GetNameFromFile(romFile.Name);
					var mediaFilePath = Path.Combine(romFile.Directory!.FullName, $"media\\{medium.XmlFolderAndKey}");
					string? mediaFile = null;

					// Wir nehmen an, dass nur eine Datei existiert. Falls Dateien mit gleichen Namen 
					// und unterschiedlichen Dateiendungen existieren, wird einfach die erste Datei genommen
					var possiblePaths = medium.KnowFileExtensions.Select(
						ext => Path.Combine(mediaFilePath, $"{fileNameWithoutExtension}{ext}"));
					if (possiblePaths != null && possiblePaths.Any())
						mediaFile = possiblePaths.FirstOrDefault(path => File.Exists(path));

					medium.FilePath = mediaFile;

					mediaJobs.Add(new MediaDownloadJob()
					{
						MediaType = medium.Type,
						GameName = sg.Name,
						ExecuteAsync = CreateMediaCheckAndDownloadJob(medium, ct)
					});
				}
			}
			return mediaJobs;
		}

		// Diese Methode erzeugt einen Job für den Download und die Prüfung eines Mediums
		private Func<Task> CreateMediaCheckAndDownloadJob(
				GameMediaSettings medium,
				CancellationToken ct)
		{
			return async () =>
			{
				var result = await GetMediaBytesAsync(medium.Url, medium.FilePath, medium.Type, ct);
				medium.ContentType = result.contentype;
				if (result.status == eMediaCheckStatus.Success_UpToDate)
				{
					medium.IsUpToDate = true;
				}
				else if (result.status == eMediaCheckStatus.Success_Updated)
				{
					// Wenn der Download erfolgreich war, speichern Sie die Daten im Medium-Objekt
					medium.NewData = result.mediadata;
				}
			};			
		}

		/// <summary>
		/// Scrapt mehrere Spiele automatisch über Hash. Eine verfeinerte Suche wird nicht angeboten.
		/// da alle Roms automatisch durchlaufen werden sollen.
		/// </summary>
		/// <param name="gameList"></param>
		/// <param name="onlyLocalMedias">Anhand des Namens der Rom-Datei werden lokale Medien gesucht und zugeordnet</param>
		/// <param name="systemid"></param>
		/// <param name="baseDir"></param>
		/// <param name="progress"></param>
		/// <param name="opt"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		public async Task ScrapGamesAsync(GameList gameList, bool onlyLocalMedias, int systemid, string baseDir,
			IProgress<ProgressObj> progress, RetroScrapOptions opt, CancellationToken ct = default)
		{
			int iGesamt = gameList.Games.Count;
			gameList.Games.ForEach(g => g.State = eState.None);
			int iPerc = 0;

			// Schleife über alle Roms
			// Hier werden wir die Anzahl der möglichen Threads berücksichten 
			// des Users. Allerdings nicht für die Rom-Metadaten
			// sondern für die Mediendateien.
			/////////////////////////////////////////////////////////////////////
			for (int i = 0; i < iGesamt; ++i)
			{
				GameEntry game = gameList.Games[i];
				iPerc = Utils.CalculatePercentage(i + 1, iGesamt);

				ct.ThrowIfCancellationRequested();
				
				progress.Report(new ProgressObj(iPerc, i + 1,
						game.Name!, Properties.Resources.Txt_Status_Label_Scrap_Running));

				try
				{
					// Rom scrappen
					FileInfo romFile = new FileInfo(FileTools.ResolveMediaPath(baseDir, game.Path)!);
					Log.Information($"--> await GetGameAsync \"{romFile.FullName}\"");
					var result = await GetGameAsync(game, romFile, gameList.RetroSys.Id, opt, 0, ct);
					// Ergebnis prüfen
					if (!result.resp.Ok)
					{
						game.State = eState.Error;
						ProgressObj error = new ProgressObj(iPerc, i + 1,
							game.Name!, $"{result.resp.Error ?? "Error"}");
						error.Typ = ProgressObj.eTyp.Error;
						progress.Report(error);
						continue;
					}

					var scrapgame = result.resp.ScrapGameResult;
					if (scrapgame == null)
					{
						ProgressObj err = new ProgressObj(iPerc, i + 1,
								game.Name!, Properties.Resources.Txt_Log_Scrap_NoDataFoundFor + game.Name!);
						err.Typ = ProgressObj.eTyp.Error;
						progress.Report(err);
						continue;
					}

					progress.Report(new ProgressObj(iPerc, i + 1,
						game.Name!, $"{Properties.Resources.Txt_Log_Scrap_CheckDataFrom} \"{game.Name}\"."));
					if (!string.IsNullOrEmpty(scrapgame.Id) && int.TryParse(scrapgame.Id, out int id) && id > 0)
						game.Id = id;
					game.Source = scrapgame.Source ?? "screenscraper.fr";
					if (!string.IsNullOrEmpty(scrapgame.Description))
						game.Description = Utils.DecodeTextFromApi(scrapgame.Description);
					if (!string.IsNullOrEmpty(scrapgame.Developer))
						game.Developer = scrapgame.Developer;
					if (!string.IsNullOrEmpty(scrapgame.Genre))
						game.Genre = scrapgame.Genre;
					if (!string.IsNullOrEmpty(scrapgame.Name))
						game.Name = scrapgame.Name;
					if (!string.IsNullOrEmpty(scrapgame.Players))
						game.Players = scrapgame.Players;
					if (!string.IsNullOrEmpty(scrapgame.Publisher))
						game.Publisher = scrapgame.Publisher;
					if (scrapgame.RatingNormalized.HasValue && game.Rating <= 0)
						game.Rating = scrapgame.RatingNormalized.Value;
					if (scrapgame.ReleaseDate != null && scrapgame.ReleaseDate != DateTime.MinValue)
						game.ReleaseDate = scrapgame.ReleaseDate;

					game.State = eState.Scraped;

					if (!onlyLocalMedias)
					{
						await ScrapMedienParallelAsync(result.mediaJobs!, progress, iPerc, i, game.Name!, ct);

						// Neue Dateien kopieren
						var mlist = scrapgame.PossibleMedien.Where(m => opt.IsMediaTypeEnabled(m));
						foreach ( var m in mlist )
						{
							if (m.IsUpToDate)
							{
								progress.Report(new ProgressObj(
											 iPerc, i + 1, game.Name!,
													$"{Properties.Resources.Txt_Log_Scrap_Identical_Skip} {m.Type}"));
								continue;
							}

							if ( m.NewData != null && m.NewData.Length > 0)
							{
								var resultlm = await LoadMediaAsync( m.Type, m.FilePath, baseDir, m.NewData, m.ContentType, ct, false, true);
								if (string.IsNullOrEmpty(resultlm.absPath) || !File.Exists(resultlm.absPath))
								{
									ProgressObj err = new ProgressObj(iPerc, i + 1,
										game.Name!, $"Error saving media {m.Type} to \"{m.FilePath}\".");
									err.Typ = ProgressObj.eTyp.Error;
									progress.Report(err);
								}
								else
								{
									var resultmove = FileTools.MoveOrCopyScrapFileRom(
										move: true,
										rom: game,
										sourcefile: resultlm.absPath,
										destbasedir: baseDir,
										destrelpath: $"./media/{RetroScrapOptions.GetMediaSettings(m.Type)!.XmlFolderAndKey}/");
									if (!resultmove.ok)
									{
										ProgressObj err = new ProgressObj(iPerc, i + 1,
										game.Name!, $"Error moving temp-media {m.Type}.");
										err.Typ = ProgressObj.eTyp.Error;
										progress.Report(err);
									}
									else
									{
										game.SetMediaPath(m.Type, resultmove.file);
									}
								}
							}
						}
					}
					else
					{
						// Nur Medien lokal suchen und zuordnen
						var res = FileTools.SetLocalMediaFilesToGame(game, baseDir);
					}
				}
				catch (OperationCanceledException)
				{
					progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
					break;
				}
				catch (Exception e)
				{
					game.State = eState.Error;
					ProgressObj err = new ProgressObj(iPerc, i + 1,
						game.Name!, $"Error! {Utils.GetExcMsg(e)}");
					err.Typ = ProgressObj.eTyp.Error;
					progress.Report(err);
					continue;
				}
			} // Next game i++

			progress.Report(new ProgressObj(0, Properties.Resources.Txt_Log_Scrap_End));
		}

		public enum eMediaCheckStatus
		{
			Success_UpToDate,        // CRCOK / MD5OK / SHA1OK (Kein Update nötig)
			Success_Updated,         // Image im Body (Update durchgeführt, Bild muss gespeichert werden)
			NotFound_NoMedia,        // NOMEDIA (Kein Bild auf dem Server)
			Error_ApiFailure,        // HTTP-Fehler (4xx/5xx) oder unerwarteter Response
			None
		}

		public async Task ScrapMedienParallelAsync(List<MediaDownloadJob>? mediaJobs, IProgress<ProgressObj>? progress, 
			int iPerc, int index, string? gamename, CancellationToken ct)
		{
			// ----------------------------------------------------------------------
			// PARALLELER MEDIEN-DOWNLOAD (falls erlaubt)
			// ----------------------------------------------------------------------
			if (mediaJobs != null && mediaJobs.Any())
			{
				// maxthreads aus der Instanz-Variable des Managers holen
				// Sicherstellen, dass das Limit mindestens 1 ist.
				int maxConcurrentMediaDownloads = LatestSsUser?.maxthreads ?? 1;
				if (maxConcurrentMediaDownloads < 1)
					maxConcurrentMediaDownloads = 1;

				// Semaphore zur Begrenzung der parallelen Downloads
				var mediaSemaphore = new SemaphoreSlim(maxConcurrentMediaDownloads);
				var downloadTasks = new List<Task>();

				for (int j = 0; j < mediaJobs.Count; j++)
				{
					var job = mediaJobs[j];
					int threadId = (j % maxConcurrentMediaDownloads) + 1; // Einfache, rotierende ID (1, 2, 3...)
																																// 1. Auf die Erlaubnis des Thread-Semaphores warten
					await mediaSemaphore.WaitAsync(ct);
					// 2. Erzeuge den Download-Task
					downloadTasks.Add(Task.Run(async () =>
					{
						try
						{
							// 3. Globale Quota-Drosselung (maxrequestspermin) vor dem Aufruf
							// Diese Methode muss die Pause und die interne/globale Zählung durchführen.
							await WaitForRateLimitAndAddRequestCounter();
							ct.ThrowIfCancellationRequested();
							// 4. Job ausführen
							// Progress: Starte Download
							if (progress != null)
							{
								progress.Report(new ProgressObj(
							 iPerc, index + 1, gamename!,
									$"{Properties.Resources.Txt_Log_Scrap_Loading} {job.MediaType}... (Thread {threadId})")
								{ ThreadId = threadId });
							}
							else
							{
								Log.Information($"ScrapMedienParallelAsync() (Thread {threadId}): {job.MediaType} starting...");
							}

							await job.ExecuteAsync();
						}
						catch (OperationCanceledException)
						{
							if (progress != null)
							{
								// Wenn der Task abgebrochen wird (z.B. durch ct.ThrowIfCancellationRequested())
								progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
										Properties.Resources.Txt_Log_Scrap_CancelRequest));
							}
							else
							{
								Log.Warning($"ScrapMedienParallelAsync() (Thread {threadId}): {job.MediaType} canceled...");
							}
						}
						catch (Exception ex)
						{
							if (progress != null)
							{
								// Fehlerbehandlung für den Thread
								ProgressObj err = new ProgressObj(iPerc, index + 1,
										gamename!, $"Error in media job: {Utils.GetExcMsg(ex)}");
								err.Typ = ProgressObj.eTyp.Error;
								progress.Report(err);
							}
							else
							{
								Log.Error($"ScrapMedienParallelAsync() (Thread {threadId}): Error in media job: {Utils.GetExcMsg(ex)}");
							}
							
						}
						finally
						{
							// 5. Die Thread-Erlaubnis freigeben
							mediaSemaphore.Release();
						}
					}, ct));
				}

				// Wichtig! Erst auf alle Jobs warten
				await Task.WhenAll(downloadTasks);
			}
		}

		public async Task<(eMediaCheckStatus status, byte[]? mediadata, string? contentype)> GetMediaBytesAsync(
			string? url, string? mediaFile, eMediaType type, CancellationToken ct)
		{
			/*
			 *	APIDoku 
			 *	https://www.screenscraper.fr/webapi2.php?alpha=0&numpage=0#mediaJeu:
			 *	https://www.screenscraper.fr/webapi2.php?alpha=0&numpage=0#mediaVideoJeu
				 * Zurückgegebenes Element: Mediafile in bytes
						ODER:
						CRCOK oder MD5OK oder SHA1OK Text, wenn der Parameter crc, md5 oder sha1 mit der crc-, md5- oder sha1-Berechnung des Server-Images (Update-Optimierung)
						identisch ist, oder
						NOMEDIA-Text, wenn die Mediendatei nicht gefunden
				 * 
				 */
			if (string.IsNullOrWhiteSpace(url) )
				return (status: eMediaCheckStatus.NotFound_NoMedia, mediadata: null, contentype: null);

			if (!url.ToLower().Contains("mediaJeu.php?".ToLower()) 
			 && !url.ToLower().Contains("mediaVideoJeu.php?".ToLower()))
			{
				//Debug.Assert(false, $"condition is mediaxxx.php Call");
				return (status: eMediaCheckStatus.NotFound_NoMedia, mediadata: null, contentype: null);
			}

			// Ist das übergebene MediaFile leer oder null, sind auch die hashes leer.
			// In dem Fall bekommen wir auf jeden Fall Daten zurück, falls es welche gibt
			var mediaFileHashes = FileTools.CalculateChecksums(mediaFile);
			string urllogstring = url.Substring(0, url.IndexOf("?") + 1) + "xxxxxx";
			Log.Information($"[Check and Get Medium {type}]: \"{urllogstring}\"");
			url += $"&md5={mediaFileHashes.MD5}&sha1={mediaFileHashes.SHA1}"; //&crc={mediaFileHashes.CRC32}";
			Log.Debug(url);

			await WaitForRateLimitAndAddRequestCounter();
			string? body = null;
			string? contentType = null;
			using (var httpResponse = await _http.GetAsync(url, ct).ConfigureAwait(false))
			{
				body = await httpResponse.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
				if (!httpResponse.IsSuccessStatusCode)
				{
					// 1. HTTP-Fehler (400, 500 etc.): Echter Fehler
					Log.Error("Error: " + httpResponse.StatusCode.ToString() + ": " + body);
					return (status: eMediaCheckStatus.Error_ApiFailure, mediadata: null, contentype: contentType);
				}

				// 2. Prüfen des Content-Types zur Unterscheidung zwischen Bild und Status-Text
				// 1. Content-Type prüfen, BEVOR der Body gelesen wird
				contentType = httpResponse.Content.Headers.ContentType?.MediaType;
				var isBinaryMedia = contentType != null &&
														(contentType.StartsWith("image/") ||
														 contentType.StartsWith("video/") ||
														 contentType.StartsWith("application/octet-stream"));

				// 2. Body-Daten einmal als Byte-Array lesen
				var bodyBytes = await httpResponse.Content.ReadAsByteArrayAsync();

				if (isBinaryMedia )
				{
					// --- Medien-Rückgabe (Updated) ---
					Log.Information($"[{type}] Media - File received. Needs saving.");
					if (bodyBytes == null || !bodyBytes.Any())
					{
						Log.Error($"[{type}] Error! bodyBytes are emtpy.");
						return (status: eMediaCheckStatus.Error_ApiFailure, mediadata: null, contentype: contentType);
					}
					else
					{
						// Den Body als Byte-Array zurückgeben, damit er gespeichert werden kann.
						return (status: eMediaCheckStatus.Success_Updated, mediadata: bodyBytes, contentype: contentType);
					}
				}
			}

			// 3. Wenn es kein Bild ist, muss es ein Text-Status sein (text/plain)
			// Wir lesen den Body als String für die Statusprüfung
			if (string.IsNullOrEmpty(body))
			{
				Log.Error($"[{type}] No readable body text response from API.");
				return (status: eMediaCheckStatus.Error_ApiFailure, mediadata: null, contentype: contentType);
			}

			string trimmedBody = body.Trim().ToUpperInvariant();
			if (trimmedBody != null && (trimmedBody == "CRCOK" || trimmedBody == "MD5OK" || trimmedBody == "SHA1OK"))
			{
				// --- TEXT-RÜCKGABE (Kein Update erforderlich) ---
				Log.Information($"[{type}] Hashes identical. File is already up-to-date.");
				return (status: eMediaCheckStatus.Success_UpToDate, mediadata: null, contentype: contentType);
			}
			else if (trimmedBody == "NOMEDIA")
			{
				// --- TEXT-RÜCKGABE (NOMEDIA) ---
				Log.Warning($"[{type}] Media not found on server for this request.");
				return (status: eMediaCheckStatus.NotFound_NoMedia, mediadata: null, contentype: contentType);
			}
			else
			{
				// --- Unerwarteter Text-Status ---
				Log.Error($"[{type}] Unexpected text response from API: {trimmedBody}");
				return (status: eMediaCheckStatus.Error_ApiFailure, mediadata: null, contentype: contentType);
			}
		}
		

		public static string? GetMediumUrl(Medium[]? medias, string type, RetroScrapOptions opt)
		{
			if (medias == null)
				return string.Empty;

			var medien = medias.Where(x => x.type != null && x.type.ToLower() == type.ToLower());
			Medium? media = null;
			if (medien != null && medien.Any())
			{
				media = medien.FirstOrDefault(x => x.region != null && x.region == opt.Region);
				if (media == null && opt.Region != "eu" )
					media = medien.FirstOrDefault(x => x.region != null && x.region == "eu"); // Default
				if (media == null)
					media = medien.FirstOrDefault(x => x.region != null && x.region == "wor");
				if (media == null)
					media = medien.FirstOrDefault();
			}
			return media?.url ?? string.Empty; 
		}

		public async Task<(System.Drawing.Image? imgForShow, string? absPath)> LoadMediaAsync(
			eMediaType type, string? relPath, string baseDir, 
			byte[]? mediadata, string? contentType,
			CancellationToken ct, 
			bool videoWithPreviewImage, bool byPassCache = false)
		{

			// Folgende Fälle kann es geben
			// 1. Die Datei ist bereits heruntergeladen aber nur als byte[]-Array übergeben. 
			//    In diesem Fall wird die Datei (bei einem Video mitsamt Vorschau-Image) 
			//    physikalisch in einem Temp-Ordner erzeugt. Rückgabe ist dann der absolute Pfad zur Datei.
			// 2. Es soll einfach nur das Image zurückgegeben werden, welches in der gamelist.xml als relativer
			//    Pfad angegeben ist. In diesem Fall lösen wir den relativen Pfad auf und generieren
			//    den absoluten Pfad. Aus ".\media\fanart\game.png" wird dann "E:\Roms\Amiga\media\fanart\game.png"

			if ( type == eMediaType.Unknown ) 
				return (null, null);

			if (type == eMediaType.Manual || type == eMediaType.Map )
			{
				// TODO: Under Construction …
				return (null, null);
			}

			//////////////////////////////////////////////////////////////////////////////////
			// Fall 1: byte[] Array umwandeln und im Temp-Ordner erzeugen

			if (mediadata != null && mediadata.Any())
			{
				if (type == eMediaType.Video)
				{
					var tempfile = await FileTools.CopyToTempAsync(mediadata, contentType);
					if (!string.IsNullOrEmpty(tempfile))
					{
						if (videoWithPreviewImage)
						{
							var preview = await ImageTools.LoadVideoPreviewAsync(baseDir, tempfile, ct, byPassCache);
							if ( preview == null )
								return (null, tempfile);
							else if (preview.HasValue)
								return (preview.Value.overlay, preview.Value.videoAbsPath);
							else
								return (null, tempfile);
						}
						else
						{
							return (null, tempfile);
						}
					}
				}
				else
				{
					var tempfile = await FileTools.CopyToTempAsync(mediadata, contentType);
					if (tempfile != null)
						return (ImageTools.LoadBitmapNoLock(tempfile), tempfile);
				}

				return (null, null);
			}

			//////////////////////////////////////////////////////////////////////////////////
			// Fall 2: Image zurückgegeben, welches als relativer Pfad angegeben ist.

			if (string.IsNullOrEmpty(relPath) )
				return (null, null);

			var localfile = FileTools.ResolveMediaPath(baseDir, relPath);
			if (string.IsNullOrEmpty(localfile))
				return (null, null);

			if (File.Exists(localfile))
			{
				if (type == eMediaType.Video)
				{
					if (videoWithPreviewImage)
					{
						var preview = await ImageTools.LoadVideoPreviewAsync(baseDir, localfile, ct, byPassCache);
						if (preview == null )
							return (null, localfile);
						else if (preview.HasValue)
							return (preview.Value.overlay, preview.Value.videoAbsPath);
						else
							return (null, preview.Value.videoAbsPath);
					}
					else
					{
						return (null, localfile);
					}
				}
				else
				{
					var img = await ImageTools.LoadImageCachedAsync(baseDir, localfile, ct, byPassCache);
					return (img, localfile);
				}
			}
			else
			{
				// Wenn wir an dieser Stelle landen, gibt es das Image nicht, obwohl der relPfad da ist
				// Erzeuge das Fehlerbild
				return (ImageTools.CreateErrorImage(localfile), null);

			}
		}

		/// <summary>
		/// Speichert das Ergebnis eines Mediendownloads 
		/// und aktualisiert das GameEntry-Objekt threadsicher.
		/// Diese Methode MUSS innerhalb eines lock(game)-Blocks aufgerufen werden!
		/// </summary>
		private void ProcessAndSaveMedia(
				GameEntry game,
				eMediaType mediaType,
				string absolutPath,
				string baseDir,
				int iPerc,
				int iCurrent,
				IProgress<ProgressObj> progress)
		{
			progress.Report(new ProgressObj(iPerc, iCurrent, game.Name!,
					$"{Properties.Resources.Txt_Log_Scrap_New_Media} {mediaType.ToString()}."));

			var res = FileTools.MoveOrCopyScrapFileRom(true,
					game, absolutPath, baseDir, $"./media/{RetroScrapOptions.GetMediaSettings(mediaType)!.XmlFolderAndKey}/");

			if (res.ok && !string.IsNullOrEmpty(res.file))
			{
				// Update des GameEntry-Objekts (THREADSICHER durch lock)
				game.SetMediaPath(mediaType, res.file);
			}
			else
			{
				ProgressObj err = new ProgressObj(iPerc, iCurrent,
						game.Name!, $"{Properties.Resources.Txt_Log_Scrap_Media_Move_Fail} {mediaType.ToString()}!");
				err.Typ = ProgressObj.eTyp.Error;
				progress.Report(err);
			}
		}
		

		private (string s1, string s2) getDD()
		{
			string? s1, s2;
			if (!_devVault.TryLoad(out s1, out s2))
				throw new ApplicationException("No Developer Login Data found.");
			return (s1!, s2!);
		}
	}

	public class MediaDownloadJob
	{
		// Die asynchrone Funktion, die die Arbeit ausführt
		public Func<Task> ExecuteAsync { get; init; } = null!;

		// Kontextdaten für das Logging
		public eMediaType MediaType { get; init; }
		public string? GameName { get; init; }
	}

	public class ScrapGameApiResponse
	{
		public bool Ok { get; set; } = false;
		public GameScrap? ScrapGameResult { get; set; }
		public string? Error { get; set; }
		public int? HttpCode { get; set; }

	}

	public class ScrapRechercheApiResponse
	{
		public bool Ok { get; set; } = false;
		public List<GameDataRecherce> RechercheResult { get; set; } = new();
		public string? Error { get; set; }
		public int? HttpCode { get; set; }

	}
}

	public class SystemeRoot
	{
		public SystemeResponse? response { get; set; }
	}

	public class SystemeResponse
	{
		public Systeme[]? systemes { get; set; }
		public SsStatus? ssstatus { get; set; }   // optionales Statusfeld der API
	}

	public class SsStatus
	{
		public string? code { get; set; }         // z.B. "OK" oder Fehlercode/Zahl
		public string? message { get; set; }      // Fehlermeldungstext
	}

	public class Systeme
	{
		public int id { get; set; }
		public SystemNoms? noms { get; set; }
		public string? extensions { get; set; }
		public string? compagnie { get; set; }
		public string? type { get; set; }
		public string? datedebut { get; set; }
		public string? datefin { get; set; }
		public string? romtype { get; set; }
		public string? supporttype { get; set; }
		public SystemMediaEntry[]? medias { get; set; }

		[System.Text.Json.Serialization.JsonIgnore]
		public string? Name_eu => noms?.nom_eu;
		
		[System.Text.Json.Serialization.JsonIgnore]
		public string? Name_us => noms?.nom_us;

		[System.Text.Json.Serialization.JsonIgnore]
		public string? SystemRom => noms?.nom_recalbox;

		// Hilfseigenschaft für Logo/Wheel
		[System.Text.Json.Serialization.JsonIgnore]
		public SystemMedia Media => new SystemMedia
		{
			icon = medias?.FirstOrDefault(static m => m.type != null && m.type.StartsWith("icon"))?.url,
			wheel = medias?.FirstOrDefault(static m => m.type != null && m.type.StartsWith("wheel-steel"))?.url
		};
	}

	public class SystemNoms
	{
		public string? nom_eu { get; set; }
		public string? nom_us { get; set; }
		public string? nom_recalbox { get; set; }     // meist passend zu Batocera
		public string? nom_retropie { get; set; }     // oft mehrere, komma-getrennt
		public string? nom_launchbox { get; set; }
		public string? nom_hyperspin { get; set; }
		public string? noms_commun { get; set; }      // Synonyme, nicht für Ordner geeignet

	}

	public class SystemMediaEntry
	{
		public string? type { get; set; }
		public string? url { get; set; }
	}

	public class SystemMedia
	{
		public string? icon { get; set; }
		public string? wheel { get; set; }
	}

	public static class BatoceraFolders
	{
		// Quelle: https://wiki.batocera.org/systems (System shortname == ROM-Ordner, "meistens")
		// Stand: 2025-07-07
		public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
		{
			// Arcade
			"mame","fbneo","dice","daphne","singe","model2","model3","naomi","naomi2",
			"namco2x6","triforce","atomiswave","lindbergh","model1", // model1 existiert als eigenes System

			// Home console – 1./2. Gen
			"channelf","atari2600","odyssey2","astrocde","apfm1000","vc4000",
			"intellivision","atari5200","colecovision","advision","vectrex","crvision","arcadia",
			// 3. Gen
			"nes","sg1000","multivision","videopacplus","pv1000","scv","mastersystem",
			"fds","atari7800","socrates","snes_msu-1",
			// 4. Gen
			"pcengine","megadrive","pcenginecd","supergrafx","snes","neogeo","cdi","amigacdtv",
			"gx4000","segacd","snes_msu-1","pico","sgb","supracan",
			// 5. Gen
			"jaguar","3do","amigacd32","sega32x","psx","pcfx","neogeocd","saturn",
			"virtualboy","satellaview","jaguarcd","sufami","n64",
			// 6. Gen
			"dreamcast","n64dd","ps2","gamecube","xbox","vsmile",
			// 7. Gen
			"xbox360","wii","ps3",
			// 8. Gen
			"wiiu","ps4",

			// Fantasy consoles (home console Abschnitt)
			"uzebox","voxatron","pico8","tic80","lowresnx","wasm4","pyxel","vircon32",

			// Portable game console
			// Handheld LCD
			"gameandwatch","lcdgames","gamepock",
			// 4. Gen Handheld
			"gb","gb2players","lynx","gamegear","gamate","gmaster","supervision","megaduck",
			// 5. Gen Handheld
			"gamecom","gbc","gbc2players","ngp","ngpc","wswan","wswanc",
			// 6. Gen Handheld
			"gba","pokemini","gp32",
			// 7. Gen Handheld
			"nds","psp",
			// 8. Gen Handheld
			"3ds","psvita",
			// Fantasy console (portable)
			"arduboy",

			// Home computer
			"pdp1","apple2","pet","atari800","atom","ti99","c20","coco","pc88","zx81","bbc","x1",
			"zxspectrum","c64","pc98","fm7","tutor","electron","camplynx","msx1","adam",
			"spectravideo","amstradcpc","macintosh","thomson","cplus4","laser310","oricatmos",
			"atarist","msx2","c128","apple2gs","archimedes","xegs","amiga500","x68000","msx2+",
			"fmtowns","samcoupe","amiga1200","vis","msxturbor",

			// Ports (eigene Systeme/Ordner)
			"ports","abuse","cannonball","cavestory","cdogs","devilutionx","dxx-rebirth","easyrpg",
			"ecwolf","eduke32","fallout1-ce","fallout2-ce","fpinball","fury","gzdoom","hcl",
			"hurrican","ikemen","lutro","mrboom","mugen","openbor","openjazz","prboom","pygame",
			"raze","scummvm","sdlpop","solarus","sonicretro","superbroswar","tyrquake","vpinball",
			"xash3d_fwgs","xrick",

			// Flatpak
			"flatpak","steam",

			// Miscellaneous
			"dos","flash","moonlight","plugnplay","vgmplay","windows","windows_installers"
		};

		// Bekannte Alias-Korrekturen/Fallspezifika zwischen Ökosystemen:
		private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
		{
			// Nintendo
			["Nintendo 3DS"] = "3ds",
			["3DS"] = "3ds",
			["Super Nintendo MSU-1"] = "snes_msu-1",
			
			// Bally
			["Astrocade"] = "astrocde",
			["Bally Astrocade"] = "astrocde",
			["Bally Professional Arcade"] = "astrocde",

			// BBC
			["BBC Micro"] = "bbc",

			// Bandai
			["WonderSwan"] = "wswan",
			["WonderSwan Color"] = "wswanc",

			["Camputers Lynx"] = "camplynx",
			["Mega Duck"] = "megaduck",
			["Arcadia 2001"] = "arcadia",
			["Game Pocket Computer"] = "gamepock",
			["FM-7"] = "fm7",
			["Super A'can"] = "supracan",
			["Game Master"] = "gmaster",
			["V.Smile"] = "vsmile",
			["Game.com"] = "gamecom",
			["Oric 1 / Atmos"] = "oricatmos",
			["CD-i"] = "cdi",
			["Thomson MO/TO"] = "thomson",
			["Linux"] = "flatpak",
			["Visual Pinball"] = "vpinball",
			["Future Pinball"] = "fpinball",
			["Watara Supervision"] = "supervision",
			["FM Towns"] = "fmtowns",
			["WASM-4"] = "wasm4",
			["VC 4000"] = "vc4000",

			// Sony
			["PlayStation 4"] = "ps4",
			["PS4"] = "ps4",
			["PS Vita"] = "psvita",
			["PlayStation Vita"] = "psvita",

			// Atari
			["Atari 2600 Supercharger"] = "atari2600",
			["Jaguar CD"] = "jaguarcd",

			// Commodore Amiga – getrennte Systeme
			["Amiga"] = "amiga500",
			["Amiga 500"] = "amiga500",
			["Amiga 1200"] = "amiga1200",
			["Amiga CD32"] = "amigacd32",
			["Amiga CDTV"] = "amigacdtv",
			["Plus/4"] = "cplus4",

			// Batocera 42+ nutzt "megacd" als Ordner für Sega CD; Systeme-Seite führt "segacd".
			// Wir mappen "segacd" -> "megacd" UND erlauben beide in All.
			["segacd"] = "megacd",
			["megacd"] = "megacd",
			["Mega-CD"] = "megacd",
			["Mega CD"] = "megacd",
			["Sega CD"] = "megacd", 
			["Sega Pico"] = "pico",
			
			// Manche Auflistungen nennen "mame/model1" – Ordner heißt "model1".
			["mame/model1"] = "model1",

			// NEO GEO
			["Neo-Geo MVS"] = "neogeo",

			// RetroPie nennt teils "genesis" – in Batocera heißt das "megadrive".
			["genesis"] = "megadrive",

			// Häufige Schreibvarianten:
			["pc-engine"] = "pcengine",
			["PC Engine"] = "pcengine",
			["TurboGrafx-16"] = "pcengine",
			["TurboGrafx16"] = "pcengine",
			["pcengine-cd"] = "pcenginecd",
			["super-grafx"] = "supergrafx",
			["supergrafx"] = "supergrafx",

			// Epoch / Casio / SNK / Entex
			["Super Cassette Vision"] = "scv",
			["PV-1000"] = "pv1000",
			["Neo-Geo MVS"] = "neogeo", 
			["Adventure Vision"] = "advision",

			// Microsoft
			["Xbox 360"] = "xbox360",
			["PC Dos"] = "dos",
			["PC Win3.xx"] = "windows",
			["PC Win9X"] = "windows",
			["PC Windows"] = "windows",
		};

		/// <summary>
		/// Wählt den passenden Batocera-ROM-Ordner (Shortname) basierend auf ScreenScraper-Namen.
		/// Reihenfolge: nom_recalbox → nom_retropie (split) → Aliases.
		/// Gibt null zurück, wenn nichts passt.
		/// </summary>
		public static string? MapToBatoceraFolder(SystemNoms? noms)
		{
			if (noms == null)
				return null;

			// 1) Recalbox ist meist 1:1 verwendbar
			if (TryNormalize(noms.nom_recalbox, out var hit))
				return hit;

			// 2) RetroPie: komma-getrennt, wir nehmen den ersten gültigen Treffer
			if (!string.IsNullOrWhiteSpace(noms.nom_retropie))
			{
				foreach (var cand in noms.nom_retropie.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
					if (TryNormalize(cand, out hit))
						return hit;
			}

			// 3) Direkter Alias-Versuch auf anderen Feldern (falls jemand dort Shortnames trägt)
			if (TryNormalize(noms.nom_eu, out hit) || TryNormalize(noms.nom_us, out hit))
				return hit;

			// 4) kein Match
			return null;
		}

		private static bool TryNormalize(string? value, out string? normalized)
		{
			normalized = null;
			if (string.IsNullOrWhiteSpace(value)) return false;

			// Alias-Tabelle zuerst
			if (Aliases.TryGetValue(value, out var mapped))
			{
				// Auch Alias-Ziel muss existieren oder aufgenommen werden
				if (All.Contains(mapped) || Aliases.ContainsKey(mapped))
				{
					normalized = mapped;
					return true;
				}
			}

			// Direkter Treffer?
			if (All.Contains(value))
			{
				normalized = value;
				return true;
			}

			// Kein Treffer
			return false;
		}
	}

public class GameRechercheRoot { public GameRechercheResponse? response { get; set; } }
public class GameRechercheResponse
{
	public object? serveurs { get; set; }
	public SsUser? ssuser { get; set; }
	public GameDataRecherce[]? jeux { get; set; }
}


public class GameRoot { public GameResponse? response { get; set; } }
public class GameResponse
{
	public SsUser? ssuser { get; set; }
	public GameData? jeu { get; set; }
}

public class GameDataBase
{
	public string? id { get; set; }
	public RegTxtObj[]? noms { get; set; }
	public IdText? editeur { get; set; }
	public IdText? developpeur { get; set; }
	public TxtObj? joueurs { get; set; }
	public LangTextObj[]? synopsis { get; set; }
	public RegTxtObj[]? dates { get; set; }
	public Genre[]? genres { get; set; }
	public Medium[]? medias { get; set; }

	public string? GetName(RetroScrapOptions opt)
	{
		if (noms != null)
		{
			var rrname = noms.FirstOrDefault(x => x.region != null && x.region == opt.Region);
			if (rrname == null)
				rrname = noms.FirstOrDefault(x => x.region != null && x.region.ToLower() == "wor");
			if (rrname == null)
				rrname = noms[0];

			return rrname.text;
		}

		return null;
	}

	public string? GetReleaseDate(RetroScrapOptions opt)
	{
		if (dates == null || dates.Count() <= 1)
			return null;
		var date = dates.FirstOrDefault(x => x.region == opt.Region);
		if (date == null)
		{
			date = dates.FirstOrDefault(x => x.region == "eu");
			if (date == null)
				return dates[0].text;
		}
		return date.text;
	}

	public string? GetDesc(RetroScrapOptions opt)
	{
		string? retVal = null;
		if (synopsis != null && synopsis.Length > 0)
		{
			var desc = synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == opt.GetLanguageShortCode());
			if (desc != null)
			{
				retVal = desc.text;
			}
			else
			{
				// Default ist englisch
				desc = synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == "en");
				if (desc != null)
					retVal = desc.text;
			}
		}
		return retVal;
	}

	public string? GetGenre(RetroScrapOptions opt)
	{
		LangTextObj? retVal = null;
		if (genres != null && genres.Length > 0)
		{
			foreach (var g in genres)
			{
				retVal = g.noms?.FirstOrDefault(x => x.langue != null && x.langue == opt.GetLanguageShortCode());
				if (retVal == null)
					retVal = g.noms?.FirstOrDefault(x => x.langue != null && x.langue == "en");
				if (retVal != null)
					break;
			}
		}

		return retVal?.text;
	}

}

public class GameData : GameDataBase
{
	public string? romid { get; set; }
	public TxtObj? note { get; set; } 
	public ApiRom[]? roms { get; set; }
	public ApiRom? rom { get; set; }
}

public class GameDataRecherce : GameDataBase
{
	public IdText? systeme { get; set; }
	public string? topstaff { get; set; }
	public string? rotation { get; set; }
	public string? controles { get; set; }
	public string? couleurs { get; set; }
	public Family[]? familles { get; set; }

}

public class IdText { public string? id { get; set; } public string? text { get; set; } }
public class RegTxtObj { public string? region { get; set; } public string? text { get; set; } }
public class TxtObj { public string? text { get; set; } }
public class Genre { public string? id { get; set; } public string? principale { get; set; } public LangTextObj[]? noms { get; set; } }
public class Medium { public string? type { get; set; } public string? url { get; set; } public string? parent { get; set; } public string? region { get; set; } }
public class Family { public string? id { get; set; } public string? nomcourt { get; set; } public string? principale { get; set; } public string? parentid { get; set; } public List<RegTxtObj>? noms { get; set; } }
public class LangTextObj { public string? langue { get; set; } public string? text { get; set; } }

public class ApiRom
{
	public string? id { get; set; }
	public string? romsize { get; set; }
	public string? romfilename { get; set; }
	public string? romnumsupport { get; set; }
	public string? romtotalsupport { get; set; }
	public string? romcloneof { get; set; }
	public string? romcrc { get; set; }
	public string? rommd5 { get; set; }
	public string? romsha1 { get; set; }
	public string? beta { get; set; }
	public string? demo { get; set; }
	public string? proto { get; set; }
	public string? trad { get; set; }
	public string? hack { get; set; }
	public string? unl { get; set; }
	public string? alt { get; set; }
	public string? best { get; set; }
	public string? netplay { get; set; }
	public string? nbscrap { get; set; }
	
	[JsonIgnore]
	public FileTools.Checksums CheckSums 
	{
		get
		{
			return new FileTools.Checksums(romfilename)
			{
				SHA1 = this.romsha1 ?? "",
				MD5 = this.rommd5 ?? "",
				CRC32 = this.romcrc ?? ""
			};
		}
	}

}

