using RetroScrap2000;
using RetroScrap2000.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace RetroScrap2000
{
	/// <summary>
	/// ScrapperManager für ScreenScraper.fr
	/// </summary>
	public class ScrapperManager
	{
		private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
		private DeveloperVault _devVault = new DeveloperVault();
		private AppInfo _appinfo = Utils.GetAppInfo();
		private string? _ssid, _sspw;

		private const string BaseUrl = "https://api.screenscraper.fr/api2/";
		private static readonly JsonSerializerOptions _jsonOpts = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public ScrapperManager()
		{
			if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
				_http.DefaultRequestHeaders.Add("User-Agent", $"{_appinfo.ProductName!.Replace(" ", "")}");
		}

		public void RefreshSecrets(string ssid, string ssPassword)
		{
			_ssid = ssid; 
			_sspw = ssPassword;
		}

		private string BuildAuthQuery() => $"devid={getDD().s1}&devpassword={getDD().s2}&softname={_appinfo.ProductName!.Replace(" ", "")}&ssid={_ssid}&sspassword={_sspw}&output=json";
		private string BuildAuthQueryWithOutSs() => $"devid={getDD().s1}&devpassword={getDD().s2}&softname={_appinfo.ProductName!.Replace(" ", "")}&output=json";

		public async Task<SsUserInfosResponse> FetchSsUserInfosAsync()
		{
			var url = $"{BaseUrl}ssuserInfos.php?{BuildAuthQuery()}";
			Trace.WriteLine(url);
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

		
		/// <summary>
		/// Liefert (ok, daten, fehler). Bei ok=false ist daten leer und fehler enthält den Grund.
		/// </summary>
		public async Task<(bool ok, Systeme[] data, string? error)> GetSystemsAsync(CancellationToken ct = default)
		{
			var url = $"{BaseUrl}systemesListe.php?{BuildAuthQueryWithOutSs()}";
			Trace.WriteLine(url);
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
					Trace.WriteLine("[ScrapperManager::DownloadSystemImagesAsync()] Skip " + sys.Name_eu);
					continue;
				}
				// beide Typen (icon, wheel) sichern
				if (sys.Media.icon != null)
				{
					string file = Path.Combine(diricons.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Dowmload Icon \"{batfoldername}\"");
						await DownloadOne(sys.Media.icon, file);
					}
					else
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Skip Icon \"{batfoldername}\" (Always exist)");
					}
				}
				if (sys.Media.wheel != null)
				{
					string file = Path.Combine(dirwheels.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Dowmload Wheel \"{batfoldername}\"");
						await DownloadOne(sys.Media.wheel, file);
					}
					else
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name_eu}: Skip Wheel \"{batfoldername}\" (Always exist)");
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
				var data = await _http.GetByteArrayAsync(url);
				await File.WriteAllBytesAsync(path, data);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Fail Download {url}: {Utils.GetExcMsg(ex)}");
			}
		}


		public async Task<(bool ok, List<GameDataRecherce>? possibleGames, string? error)> GetGameRechercheListAsync(
			string? romFileName, int systemId, RetroScrapOptions opt, CancellationToken ct = default)
		{
			if (string.IsNullOrEmpty(romFileName))
				return (false, null, Properties.Resources.Txt_Log_Scrap_NoName);

			var url = $"{BaseUrl}jeuRecherche.php?{BuildAuthQuery()}&systemeid={systemId}&recherche={Uri.EscapeDataString(romFileName)}";
			Trace.WriteLine(url);
			using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
			var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			if (!resp.IsSuccessStatusCode)
			{
				return (false, null, $"HTTP {(int)resp.StatusCode}: {body}");
			}
			// JSON?
			var head = body.TrimStart();
			if (!(head.StartsWith("{") || head.StartsWith("[")))
				return (false, null, $"{Properties.Resources.Txt_Log_Scrap_NoJson}: {head[..Math.Min(120, head.Length)]}");

			var root = System.Text.Json.JsonSerializer.Deserialize<GameRechercheRoot>(body, _jsonOpts);
			var jeux = root?.response?.jeux;
			if (jeux == null) return (false, null, Properties.Resources.Txt_Msg_Scrap_NoDataFound);

			return (ok: true, possibleGames: jeux.ToList(), null);
		}


		// Dient dazu, Loops zu vermeiden
		private static int _countScrapGame = 0;
		/// <summary>
		/// Scrapt ein Spiel
		/// </summary>
		/// <param name="romFileName">Name des Spiels</param>
		/// <param name="systemId">Die Id des Systems</param>
		/// <param name="ct"></param>
		/// <returns></returns>
		public async Task<(bool ok, ScrapeGame? data, List<GameDataRecherce>? gameRechercheList, string? error)> GetGameAsync(
			string? romFileName, int systemId, RetroScrapOptions opt, CancellationToken ct = default, bool UseRecherche = false)
		{
			if (string.IsNullOrEmpty(romFileName))
				return (ok: false, data: null, gameRechercheList: null, error: Properties.Resources.Txt_Log_Scrap_NoName);

			if (UseRecherche)
			{
				Trace.WriteLine($"Recherche nach \"{romFileName}\" ...");
				_countScrapGame++;
				var recherche = await GetGameRechercheListAsync(romFileName, systemId, opt, ct);
				if (recherche.ok)
				{
					if (recherche.possibleGames == null || recherche.possibleGames.Count == 0)
						return (true, null, null, Properties.Resources.Txt_Msg_Scrap_NoDataFound);
					// Es gab nur einen Treffer, den ziehen wir uns
					if (recherche.possibleGames.Count == 1)
					{
						GameDataRecherce r = recherche.possibleGames[0];
						string? name = r.GetName(opt);
						if (string.IsNullOrEmpty(name))
						{
							return (false, null, null, Properties.Resources.Txt_Msg_Scrap_NoDataFound);
						}
						else
						{
							// Aufruf 3 oder 4
							///////////////////////////////////////////////////////////////////////
							// Rekursiver Aufruf, aber nicht mehr als Recherche
							return await GetGameAsync(name, systemId, opt, ct, UseRecherche: false);
						}
					}
					else
					{
						// Mehr als ein Treffer, wir liefern alle Treffer zurück
						return ( ok: true, data: null, gameRechercheList: recherche.possibleGames, error: null);
					}

				}
				else
				{
					return (ok: false, data: null, gameRechercheList: null, recherche.error);
				}
			}

			// Aufruf 1
			///////////////////////////////////////////////////////////////////////
			
			// Kein Recherche Aufruf, sondern direkte Nutzung mit RomName
			_countScrapGame++;
			// Loop verhindern
			if (_countScrapGame > 4) // Max. vier Aufrufe: 1: romName direkt, 2: romname angepasst, 3: Recherche mit einem Treffer, 4: der ein Treffer
			{
				_countScrapGame++;
				return (true, null, null, Properties.Resources.Txt_Msg_Scrap_NoDataFound); 
			}
			var url = $"{BaseUrl}jeuInfos.php?{BuildAuthQuery()}&systemeid={systemId}&romnom={Uri.EscapeDataString(romFileName)}";
			Trace.WriteLine(url);
			using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
			var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

			if (!resp.IsSuccessStatusCode)
			{
				// Nicht gefunden, wir versuchen einen anderen Namen, und dann rufen wir uns selbst als Recherche-Funktion auf.
				if (((int)resp.StatusCode) == 404)
				{
					string romname = CleanPrefix(romFileName);
					if (string.Compare(romname, romFileName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						Trace.WriteLine($"Unter dem Namen \"{romFileName}\" wurde kein Spiel gefunden. Versuche Name \"{romname}\"...");
						// Aufruf 2
						///////////////////////////////////////////////////////////////////////
						return await GetGameAsync(romname, systemId, opt, ct);
					}
					else
					{
						// Aufruf 2 oder 3
						///////////////////////////////////////////////////////////////////////
						return await GetGameAsync(romFileName, systemId, opt, ct, UseRecherche: true);
					}
				}
				else
				{
					_countScrapGame = 0;
					return (ok: false, data: null, gameRechercheList: null, $"HTTP {(int)resp.StatusCode}: {body}");
				}
			}

			// JSON?
			var head = body.TrimStart();
			if (!(head.StartsWith("{") || head.StartsWith("[")))
				return (false, null, null, $"{Properties.Resources.Txt_Log_Scrap_NoJson}: {head[..Math.Min(120, head.Length)]}");

			var root = System.Text.Json.JsonSerializer.Deserialize<GameRoot>(body, _jsonOpts);
			var jeu = root?.response?.jeu;
			if (jeu == null) return (false, null, null, Properties.Resources.Txt_Log_Scrap_NoName);

			var sg = new ScrapeGame();
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
					if ( desc != null )
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
				NumberStyles.Float, CultureInfo.InvariantCulture, out var note) && note > 0.0 )
				sg.RatingNormalized = note / 20.0;	// Rating hier zwischen 1 und 20

			// Releasedatum
			if (jeu.dates != null && jeu.dates.Length > 0)
			{
				var dd = jeu.dates.FirstOrDefault(x => x.region != null && x.region == opt.Region);
				if ( dd == null )
					dd = jeu.dates.FirstOrDefault(x => x.region != null);
				sg.ReleaseDateRaw = dd?.text;
			}
			////////////////////////////////////////////////////////////////////////
			// Medien
			///////////////////////////////////////////////////////////////////////
			SetMedia(jeu, sg, opt);
			_countScrapGame = 0;
			return (ok: true, data: sg, gameRechercheList: null, null);
		}

		private void SetMedia(GameData jeu, ScrapeGame sg, RetroScrapOptions opt)
		{
			string? box = string.Empty, media = string.Empty;
			foreach (var kvp in sg.MediaUrls)
			{
				switch (kvp.Key)
				{
					case eMediaType.BoxImage:
						box = GetMediumUrl(jeu.medias, "box-2d", opt);
						if (string.IsNullOrEmpty(box))
							sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "mixrbv1", opt);
						else
							sg.MediaUrls[kvp.Key] = box;
						break;

					case eMediaType.Screenshot:
						media = GetMediumUrl(jeu.medias, "ss", opt);
						if (string.IsNullOrEmpty(media) && !string.IsNullOrEmpty(box)) // Wenn Box gesetzt ist, aber Screenshoz leer, nehmen wir mix
							media = GetMediumUrl(jeu.medias, "mixrbv1", opt);
						sg.MediaUrls[kvp.Key] = media;
						break;

					case eMediaType.Video:
						sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "video", opt);
						break;

					case eMediaType.Marquee:
						// Wenn Marquee leer ist, nehmen wir wheel
						media = GetMediumUrl(jeu.medias, "marquee", opt);
						if (string.IsNullOrEmpty(media))
							media = GetMediumUrl(jeu.medias, "wheel", opt);
						sg.MediaUrls[kvp.Key] = media;
						break;

					case eMediaType.Fanart:
						sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "fanart", opt);
						break;

					case eMediaType.Map:
						sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "maps", opt);
						break;

					case eMediaType.Manual:
						sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "manuel", opt);
						break;

					case eMediaType.Wheel: // TODO: Es gibt noch wheel-carbon, wheel-steel, wheel-hd
						media = GetMediumUrl(jeu.medias, "marquee", opt); // Wenn Marquee leer war, wurde dafür schon wheel genommen
						if (!string.IsNullOrEmpty(media))
							sg.MediaUrls[kvp.Key] = GetMediumUrl(jeu.medias, "wheel", opt);
						break;
				}
			}
		}

		public string? GetMediumUrl(Medium[]? medias, string type, RetroScrapOptions opt)
		{
			if (medias == null)
				return string.Empty;

			var medien = medias.Where(x => x.type != null && x.type.ToLower() == type);
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

		public string CleanPrefix(string gameTitle)
		{
			// *** 1. Entferne führende Ziffern, Leerzeichen, Punkte und Bindestriche ***
			// Muster: ^[\d\s\.\-–]+
			// Erkl.: Startet am Stringanfang (^) und matcht eine oder mehrere (+) Ziffern (\d), 
			//        Leerzeichen (\s), Punkte (\.), Bindestriche (\-) und Gedankenstriche (–).
			string step1_NoPrefix = Regex.Replace(gameTitle, @"^[\d\s\.\-–]+", "");

			// *** 2. Entferne alle Inhalte in runden, eckigen oder geschweiften Klammern ***
			// Muster: [\(\[\{].*?[\)\]\}]
			// Erkl.: Matcht alles, was zwischen Klammernpaaren liegt.
			string step2_NoBrackets = Regex.Replace(step1_NoPrefix, @"[\(\[\{].*?[\)\]\}]", "");

			// *** 3. Bereinige unnötige Leerzeichen ***
			// Entferne doppelte Leerzeichen und trimme den String am Ende.
			string step3_Final = Regex.Replace(step2_NoBrackets, @"\s+", " ").Trim();

			return step3_Final;
		}

		public async Task ScrapGamesAsync(GameList gameList, bool onlyLocalMedias, int systemid, string baseDir,
			IProgress<ProgressObj> progress, RetroScrapOptions opt, CancellationToken ct = default)
		{
			int iGesamt = gameList.Games.Count;
			int iPerc = 0;
			// Schleife über alle Roms
			for( int i = 0; i < iGesamt; ++i)
			{
				GameEntry game = gameList.Games[i];
				iPerc = Utils.CalculatePercentage(i + 1, iGesamt);

				if (ct.IsCancellationRequested)
				{
					progress.Report(new ProgressObj( ProgressObj.eTyp.Warning, iPerc, 
						Properties.Resources.Txt_Log_Scrap_CancelRequest));
					break;
				}

				progress.Report(new ProgressObj(iPerc, i + 1,
						game.Name!, Properties.Resources.Txt_Status_Label_Scrap_Running));

				try
				{
					// Rom scrappen (1. Versuch über Rom.Name)
					Trace.WriteLine($"--> await GetGameAsync \"{game.Name}\"");
					var result = await GetGameAsync(game.Name, gameList.RetroSys.Id, opt, ct);
					// Ergebnis prüfen
					if (!result.ok)
					{
						game.State = eState.Error;
						ProgressObj error = new ProgressObj(iPerc, i + 1,
							game.Name!, $"{result.error ?? "Error"} \"{game.Name}\".");
						error.Typ = ProgressObj.eTyp.Error;
						progress.Report(error);
						continue;
					}
					else if (result.data == null)
					{
						// 2. Versuch über Rom-Filename
						string filename = Utils.GetNameFromFile(game.FileName!);
						Trace.WriteLine($"--> await GetGameAsync \"{filename}\"");
						result = await GetGameAsync(filename, gameList.RetroSys.Id, opt, ct);
						// Ergebnis prüfen
						if (!result.ok || (result.data == null || result.gameRechercheList == null || result.gameRechercheList.Count == 0 ))
						{
							game.State = eState.NoData;
							ProgressObj warning = new ProgressObj(iPerc, i + 1,
								game.Name!, $"{Properties.Resources.Txt_Log_Scrap_NoDataFoundFor} \"{game.Name}\".");
							warning.Typ = ProgressObj.eTyp.Warning;
							progress.Report(warning);
							continue;
						}
					}
					// Daten prüfen und übernehmen
					if (result.gameRechercheList != null && result.gameRechercheList.Count > 1 )
					{
						game.State = eState.NoData;
						ProgressObj warning = new ProgressObj(iPerc, i + 1,
								game.Name!, Properties.Resources.Txt_Log_Scrap_MoreThenOneMatch);
						warning.Typ = ProgressObj.eTyp.Warning;
						progress.Report(warning);
						continue;
					}

					var scrapgame = result.data;
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
						// Images vom Scraped-Objekt temporär laden
						System.Drawing.Image? img = null;
						string? absolutPath = null;

						foreach (var kvp in scrapgame.MediaUrls)
						{
							// Wünscht der Anwender dieses Medium?
							if (!opt.IsMediaTypeEnabled(kvp.Key) || string.IsNullOrEmpty(kvp.Value))
								continue;
							progress.Report(new ProgressObj(iPerc, i + 1,
								game.Name!, Properties.Resources.Txt_Log_Scrap_Loading + $" {kvp.Key.ToString()}..."));

							if (ct.IsCancellationRequested)
							{
								progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
								Properties.Resources.Txt_Log_Scrap_CancelRequest));
								break;
							}

							var loadres = await Utils.LoadMediaAsync(kvp.Key, kvp.Value, baseDir, ct);
							img = loadres.img;
							absolutPath = loadres.absPath;
							if (img == null || string.IsNullOrEmpty(absolutPath) || !File.Exists(absolutPath))
							{
								ProgressObj err = new ProgressObj(iPerc, i + 1,
									game.Name!, $"Fail to load {kvp.Key.ToString()}!");
								err.Typ = ProgressObj.eTyp.Error;
								progress.Report(err);
								continue;
							}

							// Wenn es kein Original gibt, dann einfach speichern
							if (!game.MediaTypeDictionary.TryGetValue(kvp.Key, out string? oldRelPath)
								|| string.IsNullOrEmpty(oldRelPath))
							{
								progress.Report(new ProgressObj(iPerc, i + 1, game.Name!, $"{Properties.Resources.Txt_Log_Scrap_New_Media} {kvp.Key.ToString()}."));
								var res = FileTools.MoveOrCopyScrapFileRom(true,
									game, absolutPath, baseDir, $"./media/{RetroScrapOptions.GetStandardMediaFolderAndXmlTag(kvp.Key)}/");
								if (res.ok && !string.IsNullOrEmpty(res.file))
								{
									game.SetMediaPath(kvp.Key, res.file);
								}
								else
								{
									ProgressObj err = new ProgressObj(iPerc, i + 1,
										game.Name!, $"{Properties.Resources.Txt_Log_Scrap_Media_Move_Fail} {kvp.Key.ToString()}!");
									err.Typ = ProgressObj.eTyp.Error;
									progress.Report(err);
								}
							}
							else // Es gibt ein Original, also vergleichen
							{
								var identical = Utils.IsMediaIdentical(kvp.Key, absolutPath, oldRelPath, baseDir);
								if (identical == true)
								{
									ProgressObj info = new ProgressObj(iPerc, i + 1,
										game.Name!, $"{Properties.Resources.Txt_Log_Scrap_Identical_Skip} {kvp.Key.ToString()}.");
									info.Typ = ProgressObj.eTyp.Info;
									progress.Report(info);
									continue;
								}
								else if (identical == false)
								{
									ProgressObj info = new ProgressObj(iPerc, i + 1,
										game.Name!, $"{Properties.Resources.Txt_Log_Scrap_Different_Replace} {kvp.Key.ToString()}.");
									info.Typ = ProgressObj.eTyp.Info;
									progress.Report(info);

									var res = FileTools.MoveOrCopyScrapFileRom(true,
										game, absolutPath, baseDir, $"./media/{RetroScrapOptions.GetStandardMediaFolderAndXmlTag(kvp.Key)}/");
									if (res.ok && !string.IsNullOrEmpty(res.file))
									{
										game.SetMediaPath(kvp.Key, res.file);
									}
									else
									{
										ProgressObj err = new ProgressObj(iPerc, i + 1,
											game.Name!, $"{Properties.Resources.Txt_Log_Scrap_Media_Move_Fail} {kvp.Key.ToString()}!");
										err.Typ = ProgressObj.eTyp.Error;
										progress.Report(err);
									}
								}
								else
								{
									// Do ntohing
								}
							}
						}
					}
					else
					{
						// Medien lokal suchen und zuordnen
						var res = FileTools.SetLocalMediaFilesToGame(game, baseDir);
					}
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
			}
			progress.Report(new ProgressObj(0, Properties.Resources.Txt_Log_Scrap_End));
		}

		private (string s1, string s2) getDD()
		{
			string? s1, s2;
			if (!_devVault.TryLoad(out s1, out s2))
				throw new ApplicationException("No Developer Login Data found.");
			return (s1!, s2!);
		}
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

public sealed class ScrapeGame
{
	public string? Name { get; set; }
	public string? Id { get; set; }
	public string? Source { get; set; }
	public string? Description { get; set; }
	public string? Genre { get; set; }
	public string? Players { get; set; }
	public string? Developer { get; set; }
	public string? Publisher { get; set; }
	public double? RatingNormalized { get; set; }  // 0..1
	public string? ReleaseDateRaw { get; set; }    
	
	[JsonIgnore]
	public DateTime? ReleaseDate 
	{ 
		get
		{
			if (string.IsNullOrEmpty(ReleaseDateRaw))
				return null;
			if (DateTime.TryParseExact(
							ReleaseDateRaw,
							"yyyy-MM-dd",
							null,
							System.Globalization.DateTimeStyles.None,
							out var dt))
				return dt;
			if (int.TryParse(ReleaseDateRaw, out int year) && year > 1900 && year < 3000)
				return new DateTime(year, 1, 1);
			return null;
		} 
	}

	// Medien-URLs
	public Dictionary<eMediaType, string?> MediaUrls { get; } = new()
	{
		{ eMediaType.BoxImage, string.Empty },
		{ eMediaType.Map, string.Empty },
		{ eMediaType.Marquee, string.Empty },
		{ eMediaType.Manual, string.Empty },
		{ eMediaType.Fanart, string.Empty },
		{ eMediaType.Screenshot, string.Empty },
		{ eMediaType.Video, string.Empty },
		{ eMediaType.Wheel, string.Empty }
	};
}

public class GameRechercheRoot { public GameRechercheResponse? response { get; set; } }
public class GameRechercheResponse
{
	public object? serveurs { get; set; }
	public object? ssuser { get; set; }
	public GameDataRecherce[]? jeux { get; set; }
}

public class GameRoot { public GameResponse? response { get; set; } }
public class GameResponse
{
	public GameData? jeu { get; set; }
}
public class GameData
{
	public string? id { get; set; }
	public string? romid { get; set; }
	public RegTxtObj[]? noms { get; set; }
	public LangTextObj[]? synopsis { get; set; }
	public Genre[]? genres { get; set; }
	public TxtObj? joueurs { get; set; }
	public IdText? developpeur { get; set; }
	public IdText? editeur { get; set; }
	public TxtObj? note { get; set; } 
	public RegTxtObj[]? dates { get; set; } 
	public Medium[]? medias { get; set; }
}

public class GameDataRecherce
{
	public string? id { get; set; }
	public RegTxtObj[]? noms { get; set; }
	public IdText? systeme { get; set; }
	public IdText? editeur { get; set; }
	public IdText? developpeur { get; set; }
	public TxtObj? joueurs { get; set; }
	public string? topstaff { get; set; }
	public string? rotation { get; set; }
	public string? controles { get; set; }
	public string? couleurs { get; set; }
	public LangTextObj[]? synopsis { get; set; }
	public RegTxtObj[]? dates { get; set; }
	public Genre[]? genres { get; set; }
	public Family[]? familles { get; set; }
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

}

public class IdText { public string? id { get; set; } public string? text { get; set; } }
public class RegTxtObj { public string? region { get; set; } public string? text { get; set; } }
public class TxtObj { public string? text { get; set; } }
public class Genre { public string? id { get; set; } public string? principale { get; set; } public LangTextObj[]? noms { get; set; } }
public class Medium { public string? type { get; set; } public string? url { get; set; } public string? parent { get; set; } public string? region { get; set; } }
public class Family { public string? id { get; set; } public string? nomcourt { get; set; } public string? principale { get; set; } public string? parentid { get; set; } public List<RegTxtObj>? noms { get; set; } }
public class LangTextObj { public string? langue { get; set; } public string? text { get; set; } }



