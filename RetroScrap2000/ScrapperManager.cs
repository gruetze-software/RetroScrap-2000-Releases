using RetroScrap2000.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RetroScrap2000
{
	public class ScrapperManager
	{
		private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };
		private string _devId, _devPw, _soft;
		private string? _ssid, _sspw;

		private const string BaseUrl = "https://api.screenscraper.fr/api2/";
		private static readonly JsonSerializerOptions _jsonOpts = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public ScrapperManager()
		{
			_devId = "gruetze99"; _devPw = "nmAkw40JxtR"; _soft = "RetroScrap2000"; 
			if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
				_http.DefaultRequestHeaders.Add("User-Agent", $"{_soft}");
		}

		public void RefreshSecrets(string ssid, string ssPassword)
		{
			_ssid = ssid; 
			_sspw = ssPassword;
		}

		private string BuildAuthQuery() =>
			$"devid={_devId}&devpassword={_devPw}&softname={_soft}&ssid={_ssid}&sspassword={_sspw}&output=json";

		private string BuildAuthQueryWithOutSs() =>
		$"devid={_devId}&devpassword={_devPw}&softname={_soft}&output=json";

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
					Trace.WriteLine("[ScrapperManager::DownloadSystemImagesAsync()] Skip " + sys.Name);
					continue;
				}
				// beide Typen (icon, wheel) sichern
				if (sys.Media.icon != null)
				{
					string file = Path.Combine(diricons.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name}: Dowmload Icon \"{batfoldername}\"");
						await DownloadOne(sys.Media.icon, file);
					}
					else
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name}: Skip Icon \"{batfoldername}\" (Always exist)");
					}
				}
				if (sys.Media.wheel != null)
				{
					string file = Path.Combine(dirwheels.FullName, $"{batfoldername}.png");
					if (!File.Exists(file))
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name}: Dowmload Wheel \"{batfoldername}\"");
						await DownloadOne(sys.Media.wheel, file);
					}
					else
					{
						Trace.WriteLine($"[ScrapperManager::DownloadSystemImagesAsync()] {sys.Name}: Skip Wheel \"{batfoldername}\" (Always exist)");
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

		private static int _countScrapGame = 0;
		/// <summary>
		/// Scrapt ein Spiel
		/// </summary>
		/// <param name="romFileName">Name des Spiels</param>
		/// <param name="systemId">Die Id des Systems</param>
		/// <param name="ct"></param>
		/// <returns></returns>
		public async Task<(bool ok, ScrapeGame? data, string? error)> GetGameAsync(
			string? romFileName, int systemId, string languageShortCode, CancellationToken ct = default)
		{
			if ( string.IsNullOrEmpty(romFileName) )
				return (false, null, Properties.Resources.Txt_Log_Scrap_NoName);

			var url = $"{BaseUrl}jeuInfos.php?{BuildAuthQuery()}&systemeid={systemId}&romnom={Uri.EscapeDataString(romFileName)}";
			Trace.WriteLine(url);
			using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
			var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

			if (!resp.IsSuccessStatusCode)
			{
				if (((int)resp.StatusCode) == 404 && body == "Erreur : Rom/Iso/Dossier non trouvée !  ")
				{
					_countScrapGame++;
					if (_countScrapGame >= 2)
					{
						_countScrapGame = 0;
						return (false, null, Properties.Resources.Txt_Msg_Scrap_NoDataFound);
					}
					else
					{
						Trace.WriteLine("Unter dem Namen \"" + romFileName + "\" wurde kein Spiel gefunden. Versuche anderen Namen...");
						string romname = CleanPrefix(romFileName);
						return await GetGameAsync(romname, systemId, languageShortCode, ct);
					}
				}
			}
			// JSON?
			var head = body.TrimStart();
			if (!(head.StartsWith("{") || head.StartsWith("[")))
				return (false, null, $"{Properties.Resources.Txt_Log_Scrap_NoJson}: {head[..Math.Min(120, head.Length)]}");

			var root = System.Text.Json.JsonSerializer.Deserialize<GameRoot>(body, _jsonOpts);
			var jeu = root?.response?.jeu;
			if (jeu == null) return (false, null, Properties.Resources.Txt_Log_Scrap_NoName);

			var sg = new ScrapeGame();
			sg.Id = jeu.romid;
			
			// Name 
			if (jeu.noms != null && jeu.noms.Length > 0)
			{
				var euname = jeu.noms.FirstOrDefault(x => x.region != null && x.region == "eu");
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
				var desc = jeu.synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == languageShortCode);
				if (desc != null)
				{
					sg.Description = desc.text;
				}
				else
				{
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
					var degen = gen.noms?.FirstOrDefault(x => x.langue == languageShortCode);
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
				var dd = jeu.dates.FirstOrDefault(x => x.region != null && x.region == "eu");
				if ( dd == null )
					dd = jeu.dates.FirstOrDefault(x => x.region != null);
				sg.ReleaseDateRaw = dd?.text;
			}
			// 2D-Box
			var boximgs = jeu.medias?.Where(x => x.type != null && x.type.ToLower() == "box-2d");
			if (boximgs != null && boximgs.Any())
			{
				var boximg = boximgs.FirstOrDefault(x => x.region != null && x.region == "eu");
				if (boximg == null)
					boximg = boximgs.First();
				sg.Box2DUrl = boximg?.url;
			}
			// Screenshot (Wir nehmen die Mixbox, bestehend aus Screenshot, Box und Title)
			var screenimgs = jeu.medias?.Where(x => x.type != null && x.type.ToLower() == "mixrbv1");
			if (screenimgs != null && screenimgs.Any())
			{
				var screenimg = screenimgs.FirstOrDefault(x => x.region != null && x.region == "eu");
				if (screenimg == null)
					screenimg = screenimgs.First();
				sg.ImageUrl = screenimg?.url;
			}
			// Video
			var videos = jeu.medias?.Where(x => x.type != null && x.type.ToLower() == "video");
			if (videos != null && videos.Any())
			{
				var video = videos.FirstOrDefault(x => x.region != null && x.region == "eu");
				if (video == null)
					video = videos.First();
				sg.VideoUrl = video?.url;
			}

			return (true, sg, null);
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

		public async Task ScrapGamesAsync(GameList gameList, int systemid, string baseDir,
			IProgress<ProgressObj> progress, string languageShortCode, CancellationToken ct = default)
		{
			int iGesamt = gameList.Games.Count;
			double dPerc = 0.0;
			int iPerc = 0;
			// Schleife über alle Roms
			for( int i = 0; i < iGesamt; ++i)
			{
				GameEntry game = gameList.Games[i];
				dPerc = (double)(i + 1) / iGesamt * 100;
				iPerc = (int)Math.Round(dPerc);

				if (ct.IsCancellationRequested)
				{
					progress.Report(new ProgressObj( ProgressObj.eTyp.Warning, iPerc, 
						Properties.Resources.Txt_Log_Scrap_CancelRequest));
					break;
				}

				progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, Properties.Resources.Txt_Status_Label_Scrap_Running));

				try
				{
					// Rom scrappen
					var result = await GetGameAsync(game.Name, gameList.RetroSys.Id, languageShortCode, ct);
					// Ergebnis prüfen
					if (!result.ok)
					{
						ProgressObj err = new ProgressObj(iPerc, iGesamt, i + 1,
							game.Name!, $"{result.error}.");
						err.Typ = ProgressObj.eTyp.Error;
						progress.Report(err);
						continue;
					}
					if (result.data == null)
					{
						ProgressObj warning = new ProgressObj(iPerc, iGesamt, i + 1,
							game.Name!, $"{Properties.Resources.Txt_Log_Scrap_NoDataFoundFor} \"{game.Name}\".");
						warning.Typ = ProgressObj.eTyp.Warning;
						progress.Report(warning);
						continue;
					}
					// Daten prüfen und übernehmen
					progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, $"{Properties.Resources.Txt_Log_Scrap_CheckDataFrom} \"{game.Name}\"."));
					if (!string.IsNullOrEmpty(result.data.Id) && int.TryParse(result.data.Id, out int id) && id > 0)
						game.Id = id;
					game.Source = result.data.Source ?? "screenscraper.fr";
					if (!string.IsNullOrEmpty(result.data.Description))
						game.Description = Utils.DecodeTextFromApi(result.data.Description);
					if (!string.IsNullOrEmpty(result.data.Developer))
						game.Developer = result.data.Developer;
					if (!string.IsNullOrEmpty(result.data.Genre))
						game.Genre = result.data.Genre;
					if (!string.IsNullOrEmpty(result.data.Name))
						game.Name = result.data.Name;
					if (!string.IsNullOrEmpty(result.data.Players))
						game.Players = result.data.Players;
					if (!string.IsNullOrEmpty(result.data.Publisher))
						game.Publisher = result.data.Publisher;
					if (result.data.RatingNormalized.HasValue && game.Rating <= 0)
						game.Rating = result.data.RatingNormalized.Value;
					if ( result.data.ReleaseDate != null && result.data.ReleaseDate != DateTime.MinValue )
						game.ReleaseDate = result.data.ReleaseDate;

					// Images vom Scraped-Objekt temporär laden
					progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, Properties.Resources.Txt_Log_Scrap_Loading + " Box2D..."));
					var coverTasksc = await ImageTools.LoadImageFromUrlCachedAsync(result.data.Box2DUrl, ct);
					if (ct.IsCancellationRequested)
					{
						progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
						break;
					}
					var coverscPath = FileTools.SaveImageToTempFile(coverTasksc);
					progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, Properties.Resources.Txt_Log_Scrap_Loading + " Screenshot..."));
					if (ct.IsCancellationRequested)
					{
						progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
						break;
					}
					var shotTasksc = await ImageTools.LoadImageFromUrlCachedAsync(result.data.ImageUrl, ct);
					var shotscPath = FileTools.SaveImageToTempFile(shotTasksc);
					progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, Properties.Resources.Txt_Log_Scrap_Loading + " Video..."));
					var prevTasksc = await VideoTools.LoadVideoPreviewFromUrlAsync(result.data.VideoUrl, ct);

					if (ct.IsCancellationRequested)
					{
						progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
						break;
					}

					// Theoretisch könnte es sein, dass das alte Screenshot und Box in ein und denselben Verzeichnis liegen
					// Das räumen wir auf und setzen neue Bilder in getrennten Ordnern
					bool forceimage = false;
					if (!string.IsNullOrEmpty(game.MediaScreenshotPath) && !string.IsNullOrEmpty(game.MediaCoverPath)
						&& game.MediaCoverPath == game.MediaScreenshotPath)
						forceimage = true;

					string? currentmedia = FileTools.ResolveMediaPath(baseDir, game.MediaCoverPath);
					if ( coverscPath != null && 
						 ( forceimage 
							|| string.IsNullOrEmpty(currentmedia)
							|| !File.Exists(currentmedia)
							|| ImageTools.ImagesAreDifferent(currentmedia, coverscPath)) )
					{
						progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
								game.Name!, "Set Box2D..."));

						var res = FileTools.MoveOrCopyScrapFileRom(true, game.Name,
							coverscPath,
							baseDir,
							"./media/box2dfront/");

						if (res.ok && !string.IsNullOrEmpty(res.file))
						{
							game.MediaCoverPath = res.file;
						}
						else
						{
							ProgressObj err = new ProgressObj(iPerc, iGesamt, i + 1,
								game.Name!, "Fail to move Box2D!");
							err.Typ = ProgressObj.eTyp.Error;
							progress.Report(err);
						}
					}

					if (ct.IsCancellationRequested)
					{
						progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
						break;
					}

					currentmedia = FileTools.ResolveMediaPath(baseDir, game.MediaScreenshotPath);
					if ( shotscPath != null && 
						 ( forceimage
						|| string.IsNullOrEmpty(currentmedia)
						|| !File.Exists(currentmedia)
						|| ImageTools.ImagesAreDifferent(currentmedia, shotscPath)))
					{
						progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
							game.Name!, "Set Screenshot..."));
						var res = FileTools.MoveOrCopyScrapFileRom(true, game.Name,
							shotscPath,
							baseDir,
							"./media/images/" );

						if (res.ok && !string.IsNullOrEmpty(res.file))
						{
							game.MediaScreenshotPath = res.file;
						}
						else
						{
							ProgressObj err = new ProgressObj(iPerc, iGesamt, i + 1,
								game.Name!, $"Fail to move Screenshot!");
							err.Typ = ProgressObj.eTyp.Error;
							progress.Report(err);
						}
					}

					if (ct.IsCancellationRequested)
					{
						progress.Report(new ProgressObj(ProgressObj.eTyp.Warning, iPerc,
							Properties.Resources.Txt_Log_Scrap_CancelRequest));
						break;
					}

					currentmedia = FileTools.ResolveMediaPath(baseDir, game.MediaVideoPreviewImagePath);

					if (prevTasksc != null && !string.IsNullOrEmpty(prevTasksc.Value.videoAbsPath) 
					&& ( string.IsNullOrEmpty(game.MediaVideoPath )
						|| !File.Exists(FileTools.ResolveMediaPath(baseDir, game.MediaVideoPath))
						|| ImageTools.ImagesAreDifferent(currentmedia, prevTasksc.Value.videoPreviewAbsPath)))
					{
						progress.Report(new ProgressObj(iPerc, iGesamt, i + 1,
							game.Name!, "Set Video..."));
						var res = FileTools.MoveOrCopyScrapFileRom(true, game.Name,
							prevTasksc.Value.videoAbsPath,
							baseDir,
							"./media/videos/" );

						if (res.ok && !string.IsNullOrEmpty(res.file))
							game.MediaVideoPath = res.file;
					}
				}
				catch (Exception e)
				{
					ProgressObj err = new ProgressObj(iPerc, iGesamt, i + 1,
						game.Name!, $"Error! {Utils.GetExcMsg(e)}");
					err.Typ = ProgressObj.eTyp.Error;
					progress.Report(err);
					continue;
				}
			}
			progress.Report(new ProgressObj(0, Properties.Resources.Txt_Log_Scrap_End));
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
		public string? compagnie { get; set; }
		public string? type { get; set; }
		public string? datedebut { get; set; }
		public string? datefin { get; set; }
		public SystemMediaEntry[]? medias { get; set; }

		[System.Text.Json.Serialization.JsonIgnore]
		public string? Name => noms?.nom_eu;

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
	public string? ImageUrl { get; set; }      // Screenshot/cover/snap 
	public string? VideoUrl { get; set; }      // Video
	public string? Box2DUrl { get; set; }      // klassisches 2D Boxart
}

// stark vereinfachtes DTO fürs Parsen
file sealed class GameRoot { public GameResponse? response { get; set; } }
file sealed class GameResponse
{
	public GameData? jeu { get; set; }
}
file sealed class GameData
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
file sealed class IdText { public string? id { get; set; } public string? text { get; set; } }
file sealed class RegTxtObj { public string? region { get; set; } public string? text { get; set; } }
file sealed class TxtObj { public string? text { get; set; } }
file sealed class Genre { public string? id { get; set; } public string? principale { get; set; } public LangTextObj[]? noms { get; set; } }
file sealed class Medium { public string? type { get; set; } public string? url { get; set; } public string? parent { get; set; } public string? region { get; set; } }
file sealed class LangTextObj { public string? langue { get; set; } public string? text { get; set; } }



