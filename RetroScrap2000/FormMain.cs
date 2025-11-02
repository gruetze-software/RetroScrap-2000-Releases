using RetroScrap2000.Tools;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RetroScrap2000
{

	public partial class FormMain : Form
	{
		RetroScrapOptions _options = new RetroScrapOptions();

		ScraperManager _scraper = new ScraperManager();

		RetroSystems _systems = new RetroSystems();
		GameManager _gameManager = new GameManager();

		List<GameEntry>? _selectedRoms = null;
		RetroSystem? _selectedSystem = null;

		private System.Timers.Timer _loadTimer = new();
		private int _loadTimerTicks = 0;

		private int _romSortCol = -1;
		private SortOrder _romSortOrder = SortOrder.None;

		private int _systemSortCol = -1;
		private SortOrder _systemSortOrder = SortOrder.None;

		// Images caching damit nicht immer von Platte gelesen werden muss
		private readonly Dictionary<string, Image> _imageCache = new(StringComparer.OrdinalIgnoreCase);

		public StarRatingControl starRatingControlRom { get; set; } = new StarRatingControl();
		public FormMain(RetroScrapOptions options)
		{
			InitializeComponent();

			starRatingControlRom = new StarRatingControl()
			{
				AllowHalfStars = true,
				EmptyColor = Color.LightGray,
				FilledColor = Color.Red,
				Name = "starRatingControlRom",
				OutlineColor = Color.Black,
				Rating = 0D,
				StarCount = 5,
				StarSpacing = 4,
				Dock = DockStyle.Fill
			};
			tableLayoutPanelRomDetails.Controls.Add(starRatingControlRom, 3, 0);

			_options = options;

			_loadTimer.Interval = 500;
			_loadTimer.Elapsed += LoadTimer_Elapsed;
			_loadTimer.Enabled = false;


			// DoubleBuffering für flüssiges Zeichnen
			typeof(System.Windows.Forms.ListView).GetProperty("DoubleBuffered",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					?.SetValue(listViewSystems, true);

			typeof(System.Windows.Forms.ListView).GetProperty("DoubleBuffered",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					?.SetValue(listViewRoms, true);

			_gameManager.LoadXmlActionStart += _gameManager_LoadXmlActionStart;
			_gameManager.LoadXmlActionEnde += _gameManager_LoadXmlActionEnde;
		}

		private void _gameManager_LoadXmlActionEnde(object? sender, LoadXmlActionEventArgs e)
		{
			Log.Debug("End Loading: " + e.System?.Name_eu);
		}

		private void _gameManager_LoadXmlActionStart(object? sender, LoadXmlActionEventArgs e)
		{
			Log.Information($"Scan {e.System?.Name_eu} (\"{e.System?.RomFolderName}\")");
			Splash.UpdateSplashScreenStatus($"Scan {e.System?.Name_eu} (\"{e.System?.RomFolderName}\")");
		}

		#region Form-Events
		private void LoadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			_loadTimerTicks++;

			if (_loadTimerTicks == 20)
			{
				_loadTimer.Enabled = false;
				_loadTimer.Stop();
				CheckForUpdate();
			}
			else if (_loadTimerTicks == 1)
			{
				// Den gesamten UI-Code-Block auf dem Haupt-UI-Thread ausführen
				this.Invoke(new Action(async () =>
				{
					Splash.ShowSplashScreen();
					// Warte, bis der Splashscreen vollständig initialisiert ist
					await Splash.WaitForSplashScreenAsync();

					_systems = new RetroSystems();
					_systems.Load();
					if (_systems.SystemList.Count == 0 || _systems.IsTooOld)
					{
						await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Splash_Initializing, 100);
						await _systems.SetSystemsFromApiAsync(_scraper);
						_systems.Save();
					}
					_options.Systems = _systems;

					// Statusmeldungen mit Wartezeit
					await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Splash_LoadingSettings, 500);
					if (_options != null && _options.Secret != null
					 && _options!.Secret!.TryLoad(out string? pwd)
					 && !string.IsNullOrEmpty(pwd)
					 && !string.IsNullOrEmpty(_options.ApiUser))
					{
						_scraper.RefreshSecrets(_options!.ApiUser!, pwd!);
						
						// Einlesen der Roms der Systeme, wenn gewünscht
						if ( _options.ScanRomStartup == true 
							&& !string.IsNullOrEmpty(_options.RomPath)
							&& Directory.Exists(_options.RomPath))
							await LoadRomsAsync();

						Splash.CloseSplashScreen();
					}
					else
					{
						Splash.CloseSplashScreen();
						MyMsgBox.Show(Properties.Resources.Txt_Msg_StartAppNoSsUser);
						OpenOptionsFrm(1);
					}
				}));
			}
		}

		private async void CheckForUpdate()
		{
			UpdateChecker check = new UpdateChecker();
			var checkUpd = await check.CheckForNewVersion();
			if (checkUpd.update)
			{
				FormUpdate frm = new FormUpdate(
					checkUpd.newversion!,
					Utils.GetAppInfo().ProductVersion);
				frm.ShowDialog();
			}
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
			SetTitleMainForm();
			SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Ready);
			// TODO: Das könnte man in einer eigenen Anwendung packen,
			// damit kann man Icons scrappen über die API.
			// Die Icons habe ich aber als Ressource mit in die Anwendung gepackt.

			//var l = await _scrapper.GetSystemsAsync();
			//if (l.ok)
			//{
			//	await ScrapperManager.DownloadSystemImagesAsync(l.data);
			//}


			// Images und Icons in ListViews und MenuItems setzen
			/////////////////////////////////////////////////////////////////////////////////////////////////

			var imgListSystems = new ImageList { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
			var baseDir = RetroSystems.FolderIcons;
			if (Directory.Exists(baseDir))
			{
				foreach (var file in Directory.GetFiles(baseDir, "*.png"))
				{
					var filename = Path.GetFileName(file);
					var key = filename.Substring(0, filename.IndexOf(".")).ToLower();
					using var img = Image.FromFile(file);
					imgListSystems.Images.Add(key, new Bitmap(img)); // Kopie -> Datei wird nicht gelockt
				}
			}
			listViewSystems.SmallImageList = imgListSystems;

			ImageList imglistRoms = new ImageList() { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
			imglistRoms.Images.Add("check", Properties.Resources.check16);
			imglistRoms.Images.Add("fail", Properties.Resources.fail16);
			imglistRoms.Images.Add("fav", Properties.Resources.favorite16);
			listViewRoms.SmallImageList = imglistRoms;
			this.listViewRoms.DrawColumnHeader += (sender, e) => e.DrawDefault = true;

			SystemAlleRomsScrapenToolStripMenuItem.Image = Properties.Resources.joystick16;
			SystemCleanToolStripMenuItem.Image = Properties.Resources.clear16;
			RomscrapToolStripMenuItem.Image = Properties.Resources.joystick16;
			RomDeleteToolStripMenuItem.Image = Properties.Resources.delete16;
			RomDetailsToolStripMenuItem.Image = Properties.Resources.info16;
			SystemDetailsToolStripMenuItem.Image = Properties.Resources.info16;
			RomfavoriteToolStripMenuItem.Image = Properties.Resources.favorite16;
			RomGrouptoolStripMenuItem.Image = Properties.Resources.group16;

			_loadTimer.Enabled = true;
			_loadTimer.Start();
		}

		private void SetTitleMainForm()
		{
			var info = Utils.GetAppInfo();
			this.Text = $"{info.ProductName} by {info.Company} - Version {info.ProductVersion}";
			if (!string.IsNullOrEmpty(_options.RomPath))
				this.Text += "    Roms: \"" + _options.RomPath + "\"";
		}

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			Log.Information("FormMain_FormClosed");
			// Image Cache aufräumen
			foreach (var kv in _imageCache)
			{
				kv.Value.Dispose(); // gibt Ressourcen frei
			}
			_imageCache.Clear();
		}

		#endregion

		private void buttonRomPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fldFrm = new FolderBrowserDialog();
			if (!string.IsNullOrEmpty(_options.RomPath))
				fldFrm.SelectedPath = _options.RomPath;
			else
				fldFrm.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

			if (fldFrm.ShowDialog() == DialogResult.OK)
			{
				_options.RomPath = fldFrm.SelectedPath;
				_options.Save();
				SetTitleMainForm();
			}
		}

		private async void buttonRomsRead_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(_options.RomPath))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Rom_WrongPath);
				return;
			}

			await LoadRomsAsync();
		}

		private void buttonOptions_Click(object sender, EventArgs e)
		{
			OpenOptionsFrm();
		}

		private CancellationTokenSource? _sysSelCts;

		private async void listViewSystems_SelectedIndexChanged(object sender, EventArgs e)
		{
			await SetRomListSync();
		}

		private CancellationTokenSource? _romSelCts;
		private async void listViewRoms_SelectedIndexChanged(object sender, EventArgs e)
		{
			// laufende Jobs abbrechen & neuen Token erzeugen
			_romSelCts?.Cancel();
			_romSelCts = new CancellationTokenSource();
			var ct = _romSelCts.Token;

			_selectedRoms = new List<GameEntry>();
			foreach (var item in listViewRoms.SelectedItems)
				_selectedRoms.Add((GameEntry)((ListViewItem)item).Tag!);

			try
			{
				// alles weitere (Busy, Bilder/Videos laden, Textfelder setzen)
				// passiert in SetRomOnGuiAsync
				await SetRomOnGuiAsync(_selectedRoms.Count == 1 ? _selectedRoms[0] : null, ct);
			}
			catch (OperationCanceledException)
			{
				// ok – Auswahl wurde schnell gewechselt
			}
		}

		private void listViewSystems_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortListview(listViewSystems, e.Column, ref _systemSortCol, ref _systemSortOrder);
		}

		private void listViewRoms_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortListview(listViewRoms, e.Column, ref _romSortCol, ref _romSortOrder);
		}

		private async Task<bool> ScrapRomAsync(GameScrap? gamescrap = null)
		{
			// Mehres Roms ? GoTo Autoscrap Else ScrapRom Hashes --> No Entry? --> Recherche
			var sysFolder = GetSelectedRomPath();
			if (_selectedRoms == null || _selectedRoms.Count == 0 || string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return false;
			}

			/////////////////////////////////////////////////////////////////////////////////////
			// Mehrere Roms sind zu scrapen, da starten wir den Automatic-Modus
			////////////////////////////////////////////////////////////////////////////////////
			if (_selectedRoms.Count > 1)
			{
				FormScrapAuto frm = new FormScrapAuto(new GameList() { Games = _selectedRoms, RetroSys = _selectedSystem! },
					_scraper, GetRomPath(_selectedSystem!.Id)!, _selectedSystem!, _options);
				frm.ShowDialog();
				if (frm.ScrapWasStarting == false)
					return false;

				await SetRomListSync();
				if (listViewRoms.Items.Count > 0)
				{
					listViewRoms.Items[0].Selected = true;
					listViewRoms.SelectedItems[0].EnsureVisible();
				}
				return true;
			}
			else
			{
				/////////////////////////////////////////////////////////////////////////////////////
				// Ein Rom ist zu scrapen
				////////////////////////////////////////////////////////////////////////////////////
				GameEntry rom = _selectedRoms[0];

				Log.Information($"Scrap Rom starting: Name: {rom}, Id: {rom.Id}, Release: {rom.ReleaseDate?.ToString() ?? ""}");
				bool fail = false;
				if (string.IsNullOrEmpty(rom.Name) && string.IsNullOrEmpty(rom.FileName))
				{
					MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_WrongRomFile);
					return false;
				}

				buttonRomScrap.Enabled = false;
				GameScrap? sc = null;
				try
				{
					Splash.ShowSplashScreen();
					await Splash.WaitForSplashScreenAsync();
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Scrap_Running);
					await Splash.ShowStatusWithDelayAsync(string.Format(
						Properties.Resources.Txt_Status_Label_Scrap_Running, rom.FileName), 100);

					// Rom scrappen

						FileInfo romFile = new FileInfo(FileTools.ResolveMediaPath(sysFolder, rom.Path)!);
						Log.Information($"await GetGameAsync \"{romFile.Name}\"");
						var result = await _scraper.GetGameAsync(rom, romFile,
							rom.RetroSystemId, _options);
					
					if (!result.resp.Ok)
					{
						// Bei 404 starten wir einen Recherche-Dialog indem man differenzierter suchen kann
						if (result.resp.HttpCode == 404)
						{
							Splash.CloseSplashScreen();
							FormScrapRomResearch formrecherche = new FormScrapRomResearch(romFile.FullName, _scraper,
								_selectedSystem!, _options);
							if (formrecherche.ShowDialog() == DialogResult.OK)
							{
								// Es wurde tatsächlich zu der Datei ein Eintrag gefunden.
								// Sollte der Anwender sein Einverständnis geben, laden wir
								// Informationen über die Api an die Community
								if (formrecherche.SelectedGame != null)
								{
									sc = new GameScrap();
									sc = sc.CopyFrom(formrecherche.SelectedGame, _options);
									if (sc.Id == null || !int.TryParse(sc.Id, out int gameid) )
									{
										fail = true;
									}
									else
									{
										Splash.ShowSplashScreen();
										await Splash.WaitForSplashScreenAsync();
										SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Scrap_Running);
										await Splash.ShowStatusWithDelayAsync(string.Format(
											Properties.Resources.Txt_Status_Label_Scrap_Running, rom.FileName), 100);

										// Die Medien sind hier im Gegensatz zu getGameAsync nicht enthalten bei Rechercher-Objekten
										// Diese holen wir uns jetzt aber
										Log.Information($"await GetGameAsync with GameId \"{romFile.Name}\"");
										result = await _scraper.GetGameAsync(rom, gameid, rom.RetroSystemId, romFile, _options, CancellationToken.None);
										if (!result.resp.Ok)
										{
											sc = null;
											fail = true;
										}
										else
										{
											sc = result.resp.ScrapGameResult;
											fail = false;
										}
									}
								}
								else
								{
									Debug.Assert(false, "FormScrapRomResearch ok, but no selected Entry.");
									fail = true;
								}
							} 
							else
							{
								// Recherche-Dialog abgebrochen
								fail = true;
							}
						} // Error 404 Ende
						else
						{
							// Other Http-Error
							fail = true;
						}
					} // Result Not Okay Ende
					else
					{
						// Result is ok
						sc = result.resp.ScrapGameResult;
					}

					if (sc != null)
					{
						fail = false;
						Splash.UpdateSplashScreenStatus(Properties.Resources.Txt_Splash_LoadingMedia);
						await _scraper.ScrapMedienParallelAsync(result.mediaJobs, null, 0, 0, sc.Name, CancellationToken.None);
					}
					else
					{
						fail = true;
					}
					Splash.CloseSplashScreen();

					if (fail || sc == null )
					{
						string err = result.resp.Error ?? Properties.Resources.Txt_Msg_Scrap_Fail;
						Log.Error(err);
						MyMsgBox.ShowErr(err);
						SetStatusToolStripLabel(Properties.Resources.Txt_Msg_Scrap_Fail);
						return false;
					}
					/////////////////////////////////////////////////////////////////////////////////////
					// Scrapdaten übernehmen
					////////////////////////////////////////////////////////////////////////////////////
					
					Debug.Assert(sc != null);
					sc.Source = "ScreenScraper.fr";
					Log.Information($"Scrap Infos: {sc.Name}, Id: {sc.Id}, Release: {sc.ReleaseDate?.ToString() ?? ""}");

					// Dialog zum Übernehmen der Daten
					Splash.CloseSplashScreen();
					using var dlg = new FormScrapRom(sysFolder, rom, sc, _options, _scraper);
					if (dlg.ShowDialog(this) != DialogResult.OK)
					{
						rom.State = eState.None;
						SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Ready);
						return false;
					}
					var sel = dlg.Selection;
					var d = sel.NewData!;
					// Felder übernehmen
					if (!string.IsNullOrEmpty(d.Id) && int.TryParse(d.Id, out int id) && id != rom.Id)
						rom.Id = id;
					if (string.IsNullOrEmpty(d.Source) || d.Source != rom.Source)
						rom.Source = d.Source;
					if (sel.TakeName) rom.Name = d.Name;
					if (sel.TakeDesc) rom.Description = d.Description;
					if (sel.TakeGenre) rom.Genre = d.Genre;
					if (sel.TakePlayers) rom.Players = d.Players;
					if (sel.TakeDev) rom.Developer = d.Developer;
					if (sel.TakePub) rom.Publisher = d.Publisher;
					if (sel.TakeRelease) rom.ReleaseDate = d.ReleaseDate;
					if (sel.TakeRating) rom.Rating = d.RatingNormalized.HasValue ? d.RatingNormalized.Value : 0.0;


					foreach (var med in sel.MediaTempPaths)
					{
						if (sel.MediaTempPaths.TryGetValue(med.Key, out (string tempPath, bool takeit) val))
						{
							if (val.takeit && !string.IsNullOrEmpty(val.tempPath))
							{
								(bool ok, string? file) = CopyOrMoveMediaFileToRom(true, val.tempPath,
									$"./media/{RetroScrapOptions.GetMediaSettings(med.Key)!.XmlFolderAndKey}/", sysFolder);
								if (ok && !string.IsNullOrEmpty(file))
									rom.SetMediaPath(med.Key, file);
							}
						}
					}

					rom.State = eState.Scraped;
					// UI aktualisieren
					await SetRomOnGuiAsync(rom, CancellationToken.None);
					await SaveRomAsync();

					
					return true;
				}
				catch (Exception ex)
				{
					Splash.CloseSplashScreen();
					Log.Error(Utils.GetExcMsg(ex));
					MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
					SetStatusToolStripLabel(Properties.Resources.Txt_Msg_Scrap_Fail);
					return false;
				}
				finally
				{
					Splash.CloseSplashScreen();
					buttonRomScrap.Enabled = true;
				}
			}
		}

		private async void buttonRomScrap_Click(object sender, EventArgs e)
		{
			await ScrapRomAsync();
		}

		private async void buttonRomSave_Click(object sender, EventArgs e)
		{
			await SaveRomAsync();
		}

		private void SystemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var gamelist = GetSeletedGameList();
			if (gamelist != null)
			{
				FormSystemDetails frm = new FormSystemDetails(gamelist.RetroSys);
				if (frm.ShowDialog() == DialogResult.OK)
				{
					if (!string.IsNullOrEmpty(gamelist.RetroSys.Description))
						_systems.Save();
				}
			}
		}

		private async void SystemAllRomsScrapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedSystem == null)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			var gamelist = GetSeletedGameList();
			if (gamelist == null)
				return;

			FormScrapAuto frm = new FormScrapAuto(gamelist, _scraper, baseDir, _selectedSystem, _options);
			frm.ShowDialog();
			if (frm.ScrapWasStarting == false)
				return;

			await SetRomListSync();
			if (listViewRoms.Items.Count > 0)
			{
				listViewRoms.Items[0].Selected = true;
				listViewRoms.SelectedItems[0].EnsureVisible();
			}
		}

		private void RomDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRoms == null || _selectedRoms.Count != 1)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			GameEntry rom = _selectedRoms[0];
			var romFileName = Path.GetFileName(rom.Path ?? rom.Name ?? "");
			if (string.IsNullOrEmpty(romFileName))
				return;

			FormRomDetails frm = new FormRomDetails(rom, Path.Combine(baseDir, romFileName));
			frm.ShowDialog();
		}

		private async void RomLöschenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRoms == null || _selectedRoms.Count == 0)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			StringBuilder fileMsg = new StringBuilder();
			int anzmsg = 0;
			foreach (var item in _selectedRoms)
			{
				anzmsg++;
				if (anzmsg == 5 && _selectedRoms.Count > 6)
				{
					fileMsg.AppendLine("- ...");
					break;
				}
				fileMsg.AppendLine("-   " + item.Name!);
			}

			if (MyMsgBox.ShowQuestion(
				Properties.Resources.Txt_Msg_Qestion_DeleteRom + "\r\n\r\n" + fileMsg.ToString())
					!= DialogResult.Yes)
				return;

			Splash.ShowSplashScreen();
			await Splash.WaitForSplashScreenAsync();
			await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_PleaseWait, 100);
			int listviewselindexrom = listViewRoms.SelectedItems[0].Index;
			fileMsg.Clear();
			foreach (var rom in _selectedRoms)
			{

				var romFileName = Path.GetFileName(rom.Path ?? rom.Name ?? "");
				if (string.IsNullOrEmpty(romFileName))
					return;

				var file = Path.Combine(baseDir, romFileName);

				bool deletefile = true;
				bool deletefromxml = true;
				bool deleteAllRefs = false;
				int numberofentries = GameListLoader.GetNumbersOfEntriesInXml(Path.Combine(baseDir, "gamelist.xml"), rom);
				if (numberofentries > 1)
				{
					Splash.CloseSplashScreen();
					var decision = MyMsgBox.ShowQuestion(string.Format(Properties.Resources.Txt_Msg_Question_DeleteRomMultipleEntries, numberofentries));
					Splash.ShowSplashScreen();
					await Splash.WaitForSplashScreenAsync();
					await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_PleaseWait, 100);
					if (decision == DialogResult.Yes)
					{
						// Alles löschen
						deletefile = true;
						deletefromxml = true;
						deleteAllRefs = true;
					}
					else if (decision == DialogResult.No)
					{
						// Nur den Eintrag löschen
						deletefile = false;
						deletefromxml = true;
						deleteAllRefs = false;
					}
					else
					{
						// Nichts löschen
						deletefile = false;
						deletefromxml = false;
						deleteAllRefs = false;
					}
				}

				if (deletefromxml)
				{
					if (!GameListLoader.DeleteGame(
						xmlp: baseDir,
						rom: rom,
						deleteAllReferences: deleteAllRefs))
					{
						Log.Error("Failed to delete from gamelist.xml " + rom.FileName!);
					}
				}

				if (deletefile)
				{
					if (File.Exists(file))
					{
						try
						{
							Log.Information($"Delete Rom-File \"{file}\" ...");
							File.Delete(file);
							Log.Information("... Rom-File deleted.");
						}
						catch (Exception ex)
						{
							Log.Error(Utils.GetExcMsg(ex));
							fileMsg.AppendLine("-  " + rom.FileName! + " " + ex.Message);
						}
					}
					else
					{
						fileMsg.AppendLine("-  " + rom.FileName! + " not exist");
						Log.Information($"Rom-Datei \"{file}\" not exist.");
					}
				}
			} // Next Rom

			//if ( fileMsg.Length > 0 ) 
			// MyMsgBox.ShowErr() // TODO:
			// Liste neu laden
			GetSeletedGameList()?.Games.Clear();
			var loadres = _gameManager.Loader.Load(Path.Combine(baseDir, "gamelist.xml"),
				GetSeletedGameList()?.RetroSys);
			// Liste neu setzen
			GetSeletedGameList()?.Games.AddRange(loadres.Games.Games);
			// UI neu setzen
			await SetRomListSync(listviewselindexrom);

			Splash.CloseSplashScreen();
		}



		private async void RomScrapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await ScrapRomAsync();
		}


		private void OpenOptionsFrm(int tabpageindex = 0)
		{
			FormOptions frm = new FormOptions(_options, _scraper, tabpageindex, _systems.SystemList);
			if (frm.ShowDialog() == DialogResult.OK)
			{
				_options.Save();
			}
		}

		private GameList? GetSeletedGameList()
		{
			return (listViewSystems.SelectedItems.Count == 1)
						? listViewSystems.SelectedItems[0].Tag as GameList
						: null;
		}

		private ListViewItem GetLVRom(GameEntry g)
		{
			var it = new ListViewItem(g.Name ?? Path.GetFileName(g.Path ?? ""));

			it.SubItems.Add(g.ReleaseDate?.ToString("yyyy") ?? "");
			it.SubItems.Add(g.Genre);
			it.SubItems.Add(g.Players);
			it.SubItems.Add(g.RatingStars > 0.0 ? g.RatingStars.ToString("0.0") : "");
			it.SubItems.Add(Path.GetFileName(g.Path ?? ""));
			if (g.Favorite)
				it.ImageKey = "fav";
			else if (g.State == eState.Error)
				it.ImageKey = "fail";
			else if (g.State == eState.Scraped)
				it.ImageKey = "check";

			it.Tag = g;

			return it;
		}

		private async Task SetRomListSync(int selromindex = 0)
		{
			// Selektion lesen: GameList des Systems aus Tag
			var roms = GetSeletedGameList();
			_selectedSystem = roms?.RetroSys;

			string? romsPath = _selectedSystem != null ? GetRomPath(_selectedSystem.Id) : null;
			if (string.IsNullOrEmpty(romsPath) || !Directory.Exists(romsPath))
			{
				listViewRoms.Items.Clear();
				_selectedRoms = null;
				await SetRomOnGuiAsync(null, CancellationToken.None); // Details rechts leeren
				return;
			}

			// laufende System-Ladevorgänge abbrechen
			_sysSelCts?.Cancel();
			_sysSelCts = new CancellationTokenSource();
			var sysCt = _sysSelCts.Token;

			this.Cursor = Cursors.WaitCursor;
			// UI: Systemanzeige aktualisieren (Banner + Infos)
			try
			{
				await SetSystemOnGuiAsync(_selectedSystem, sysCt);
			}
			catch (OperationCanceledException) { /* ok */ }

			// ROM-Liste neu füllen (gebatcht, kein Async nötig – nur UI)
			SetStatusToolStripLabel(string.Format(Properties.Resources.Txt_Status_Label_ReadSystem,
				_selectedSystem!.Name_eu));

			try
			{
				listViewRoms.Items.Clear();
				_selectedRoms = null;
				await SetRomOnGuiAsync(null, CancellationToken.None); // Details rechts leeren

				if (roms == null)
					return;

				var items = new List<ListViewItem>(roms.Games.Count);
				foreach (var g in roms.Games)
				{
					ListViewItem it = GetLVRom(g);
					items.Add(it);
				}

				listViewRoms.Items.AddRange(items.ToArray());
			}
			finally
			{
				listViewRoms.EndUpdate();

				if (selromindex < 0 || selromindex >= listViewRoms.Items.Count)
					selromindex = 0;
				if (listViewRoms.Items.Count > 0)
				{
					listViewRoms.Items[selromindex].Selected = true;
					listViewRoms.Items[selromindex].Focused = true;
					listViewRoms.EnsureVisible(selromindex);
				}
				SetStatusToolStripLabel(string.Format(Properties.Resources.Txt_Status_Label_AnzRomsLoad,
					listViewRoms.Items.Count));

				this.Cursor = Cursors.Default;
			}
		}


		private async Task SaveRomAsync()
		{
			if (_selectedSystem == null)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoSystem);
				return;
			}

			if (_selectedRoms == null || _selectedRoms.Count == 0)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return;
			}

			if (_selectedRoms.Count != 1)
				return;

			var rom = _selectedRoms[0];

			var sysFolder = GetSelectedRomPath();
			if (string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return;
			}

			// GUI -> Model (nur Felder, die du im UI editieren lässt)
			rom.Name = textBoxRomName.Text?.Trim();
			rom.Description = textBoxRomDesc.Text?.Trim();
			rom.Genre = textBoxRomDetailsGenre.Text?.Trim();
			rom.Players = textBoxRomDetailsAnzPlayer.Text?.Trim();
			rom.Developer = textBoxRomDetailsDeveloper.Text?.Trim();
			rom.Publisher = textBoxRomDetailsPublisher.Text?.Trim();
			// RatingStars ist read-only? Falls editierbar: (0..5) -> 0..1
			rom.Rating = Math.Clamp(starRatingControlRom.Rating / 5.0, 0.0, 1.0);

			// Releasedate: nimm, was in der GUI steht (optional)
			if (DateTime.TryParse(textBoxRomDetailsReleaseDate.Text, out var rd))
				rom.ReleaseDate = rd;

			try
			{
				buttonRomSave.Enabled = false;
				SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomSaving);
				Splash.ShowSplashScreen();
				await Splash.WaitForSplashScreenAsync();
				await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Status_Label_RomSaving, 100);
				bool ok = await Task.Run(() =>
						_selectedSystem!.SaveRomToGamelistXml(
								romPath: sysFolder,
								rom: rom
						)
				);
				if (ok)
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomSave);

					var tempLv = GetLVRom(rom);
					var selitem = listViewRoms.SelectedItems[0];
					for (int i = 0; i < selitem.SubItems.Count; i++)
						selitem.SubItems[i].Text = tempLv.SubItems[i].Text;
					if (rom.State == eState.Scraped)
						selitem.ImageKey = "check";
					else if (rom.State == eState.Error)
						selitem.ImageKey = "fail";
					else if (rom.Favorite)
						selitem.ImageKey = "fav";
				}
				else
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomNotSaved);
				}

			}
			catch (Exception ex)
			{
				Splash.CloseSplashScreen();
				Log.Error(Properties.Resources.Txt_Status_Label_RomNotSaved + " " + Utils.GetExcMsg(ex));
				MyMsgBox.ShowErr(Properties.Resources.Txt_Status_Label_RomNotSaved + "\r\n\r\n" + Utils.GetExcMsg(ex));
				SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomNotSaved);
			}
			finally
			{
				buttonRomSave.Enabled = true;
				Splash.CloseSplashScreen();
			}
		}

		public async Task RefreshGameList()
		{
			if (_selectedSystem == null || GetSelectedRomPath() == null)
				return;

			Splash.ShowSplashScreen();
			await Splash.WaitForSplashScreenAsync();
			Splash.UpdateSplashScreenStatus($"{Properties.Resources.Txt_Splash_Loading} \"{_selectedSystem.RomFolderName}\"...");
			var gl = await Task.Run(() =>
				_gameManager.LoadSystem(Path.Combine(GetSelectedRomPath()!, "gamelist.xml"), _selectedSystem));
			listViewSystems.SelectedItems[0].SubItems[4].Text = gl.Games.Count.ToString();
			listViewSystems.SelectedItems[0].Tag = gl;
			await SetRomListSync();
			Splash.CloseSplashScreen();
		}

		private async Task LoadRomsAsync()
		{
			if (!Directory.Exists(_options.RomPath))
				return;
			Log.Information("LoadRomsAsync()");
			SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_ReadRoms);
			listViewSystems.BeginUpdate();
			listViewRoms.BeginUpdate();
			listViewSystems.Items.Clear();
			listViewRoms.Items.Clear();
			_selectedRoms = null;
			_selectedSystem = null;
			await SetRomOnGuiAsync(null, CancellationToken.None);

			Splash.ShowSplashScreen();
			await Splash.WaitForSplashScreenAsync();
			await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Splash_LoadingSystems, 500);
			try
			{
				await Task.Run(() => _gameManager.Load(_options.RomPath, _systems));

				int total = _gameManager.SystemList.Count;
				var items = new List<ListViewItem>(total);
				for (int i = 0; i < total; i++)
				{
					var kv = _gameManager.SystemList.ElementAt(i);
					Splash.UpdateSplashScreenStatus($"{Properties.Resources.Txt_Splash_Loading} \"{kv.Key.ToUpper()}\"... ({i + 1}/{total})");
					Splash.UpdateSplashScreenProgress(Utils.CalculatePercentage(i + 1, total));
					Thread.Sleep(50);
					try
					{
						var roms = kv.Value;
						var sys = roms.RetroSys;
						if (sys == null) continue;

						var it = new ListViewItem(sys.Name_eu)
						{
							Tag = roms,
							ImageKey = kv.Key
						};
						it.SubItems.Add(sys.Hersteller);
						it.SubItems.Add(sys.Debut > 0 ? sys.Debut.ToString() : "");
						it.SubItems.Add(sys.Ende > 0 ? sys.Ende.ToString() : "");
						it.SubItems.Add(roms.Games.Count.ToString());
						items.Add(it);
					}
					catch (Exception ex)
					{
						Log.Error($"Fail LoadRomsAsync \"{kv.Value.RetroSys}\": " + Utils.GetExcMsg(ex));
						continue;
					}
				}
				listViewSystems.Items.AddRange(items.ToArray());
			}
			catch (Exception ex)
			{
				Log.Error("Fail LoadRomsAsync: " + Utils.GetExcMsg(ex));
			}
			finally
			{
				listViewSystems.EndUpdate();
				listViewRoms.EndUpdate();
				if (listViewSystems.Items.Count > 0)
				{
					// Das erste Element auswählen
					listViewSystems.Items[0].Selected = true;
					listViewSystems.Items[0].Focused = true;
				}
				SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Ready);
				Splash.CloseSplashScreen();
			}
		}

		private (bool ok, string? file) CopyOrMoveMediaFileToRom(bool movefile, string sourcefile,
			string relMediaRomPath, string basedir)
		{
			(bool ok, string? file) result = (false, null);
			if (_selectedRoms == null || _selectedRoms.Count > 1)
				return result;

			if (!string.IsNullOrEmpty(sourcefile) && File.Exists(sourcefile))
			{
				result = FileTools.MoveOrCopyScrapFileRom(
					move: movefile,
					rom: _selectedRoms[0],
					sourcefile: sourcefile,
					destbasedir: basedir,
					destrelpath: relMediaRomPath);
			}
			return result;

		}

		private void SortListview(System.Windows.Forms.ListView lst, int newsortcolumn, ref int aktsortcolumn,
			ref SortOrder sortOrder)
		{
			if (newsortcolumn != aktsortcolumn)
			{
				aktsortcolumn = newsortcolumn;
				sortOrder = SortOrder.Ascending;
			}
			else
			{
				sortOrder = sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			}

			lst.ListViewItemSorter = new SmartListViewComparer(aktsortcolumn, sortOrder);
			lst.Sort();
			ListViewSortIcons.SetSortIcon(lst, aktsortcolumn, sortOrder);
		}

		private async Task SetSystemOnGuiAsync(RetroSystem? system, CancellationToken ct)
		{
			Log.Information("SetSystemOnGuiAsync(): " + system ?? "Null");
			if (system == null)
			{
				pictureBoxRomSystem.Image = null;
				listBoxSystem.Items.Clear();
				return;
			}

			// Textinfos sofort
			listBoxSystem.BeginUpdate();
			listBoxSystem.Items.Clear();
			listBoxSystem.Items.Add(system.Name_eu);
			listBoxSystem.Items.Add($"{Properties.Resources.Txt_System_Manufacturer}: " + (system.Hersteller ?? ""));
			if (system.Debut > 0 && system.Ende > 0)
				listBoxSystem.Items.Add($"{system.Debut} - {system.Ende}");
			listBoxSystem.EndUpdate();

			// Banner/Logo laden (busy + cache)
			UiTools.ShowBusyPreview(pictureBoxRomSystem, "System …");

			try
			{
				Image? banner = null;
				if (!string.IsNullOrWhiteSpace(system.FileBanner) && File.Exists(system.FileBanner))
				{
					// 64px/Panelgröße je nach UI; hier voll in PictureBox
					banner = await Task.Run(() =>
					{
						ct.ThrowIfCancellationRequested();
						var bmp = ImageTools.LoadBitmapNoLock(system.FileBanner);
						return (Image?)bmp;
					}, ct);
				}

				if (!ct.IsCancellationRequested)
				{
					UiTools.HideBusyPreview(pictureBoxRomSystem);
					pictureBoxRomSystem.Image = banner; // null ist ok → bleibt leer
				}
			}
			catch (OperationCanceledException) { /* ok */ }
		}

		private async Task UpdateMediaPanelAsync(GameEntry? rom, string baseDir, CancellationToken ct)
		{
			
			// Alle vorhandenen TabPages entfernen, um eine saubere Ansicht zu gewährleisten
			flowLayoutPanelMedia.Controls.Clear();
			if (rom == null)
				return;

			Log.Information($"Update MediaPanelAsync: \"{rom?.FileName ?? "Null"}\"");

			// Das Kontextmenü für die MediaPreviewControls setzen
			List<ToolStripMenuItem> addTypeMenuItems = new List<ToolStripMenuItem>()
			{
				addType1ToolStripMenuItem,
				addType2ToolStripMenuItem,
				addType3ToolStripMenuItem,
				addType4ToolStripMenuItem,
				addType5ToolStripMenuItem,
				addType6ToolStripMenuItem,
				addType7ToolStripMenuItem,
				addType8ToolStripMenuItem,
				addType9ToolStripMenuItem,
				addType10ToolStripMenuItem,
				addType11ToolStripMenuItem,
				addType12ToolStripMenuItem
			};
			// Zunächst alle für neue Medienarten ausblenden
			foreach (var item in addTypeMenuItems)
				item.Visible = false;

			// Durch die Map iterieren und Tabs nur für vorhandene Medien erstellen
			int index = 0;
			foreach (var kvp in rom!.MediaTypeDictionary)
			{
				string? mediaPath = kvp.Value;
				// Nur wenn der Pfad vorhanden (nicht null, nicht leer) ist, einen Tab erstellen
				if (!string.IsNullOrEmpty(mediaPath))
				{
					var control = new MediaPreviewControl();
					control.MediaType = kvp.Key;
					control.DisplayMode = MediaPreviewControl.ControlDisplayMode.Buttons;
					control.ContextMenuStrip = contextMenuStripMedia;

					// Events der Buttons abonnieren
					control.ViewMediaClicked += MediaControl_ViewMediaClicked;
					control.NewMediaClicked += MediaControl_NewMediaClicked;
					control.DeleteMediaClicked += MediaControl_DeleteMediaClicked;

					await control.LoadMediaAsync(mediaPath, baseDir, null, null, ct, _scraper, true);
					Log.Information("Adding media control for " + kvp.Key);
					flowLayoutPanelMedia.Controls.Add(control);
				}
				else
				{
					addTypeMenuItems[index].Visible = true;
					addTypeMenuItems[index].Text = string.Format(Properties.Resources.Txt_Menu_AddMedium, kvp.Key.ToString());
					addTypeMenuItems[index].Tag = kvp.Key;
					index++;
				}
			}
			Utils.ForceHorizontalScrollForMediaPreviewControls(flowLayoutPanelMedia);
		}

		private async void addMediaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRoms == null || _selectedRoms.Count != 1)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			eMediaType type = (eMediaType)(sender as ToolStripMenuItem)!.Tag!;
			OpenFileDialog ofd = new OpenFileDialog();
			if (type == eMediaType.Video)
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Video;
				ofd.Filter = "MP4 (*.mp4)|*.mp4";
			}
			else
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Image;
				ofd.Filter = "Png (*.png)|*.png|Jpeg (*.jpg)|*.jpg";
			}
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				await SetNewFile(type, ofd.FileName);
			}

		}

		private async Task DeleteMediaFile(eMediaType type, string? absolutPath, bool noqestion, bool deleteonlyxml = false)
		{
			if (string.IsNullOrEmpty(absolutPath) || !File.Exists(absolutPath) || _selectedRoms == null || _selectedRoms.Count != 1)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			bool deleteOnlyXmlInfo = deleteonlyxml;
			// Frage, ob nur der XML-Eintrag oder auch die Datei gelöscht werden soll
			if (noqestion == false)
			{
				var userChoice = MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Question_DeleteMedia);
				if (userChoice == DialogResult.Cancel)
					return;

				deleteOnlyXmlInfo = userChoice == DialogResult.No;
			}

			bool deleted = false;
			if (!deleteOnlyXmlInfo)
			{
				// Datei physikalisch löschen
				deleted = FileTools.DeleteScrapFile(baseDir, _selectedRoms[0].MediaTypeDictionary[type]!);
			}

			// Immer den XML-Eintrag löschen
			_selectedRoms[0].SetMediaPath(type, null);
			await SaveRomAsync();
			await SetRomOnGuiAsync(_selectedRoms[0], CancellationToken.None);
		}

		private void viewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Hier brauche ich das MediaPreviewControl, von dem aus das Kontextmenü geöffnet wurde			
			var menuItem = sender as ToolStripItem;
			if (menuItem == null) return;
			var contextMenu = menuItem.Owner as ContextMenuStrip;
			if (contextMenu == null) return;
			var control = contextMenu.SourceControl as MediaPreviewControl;

			if (control != null)
				UiTools.OpenPicBoxTagFile(control.PicBox);
		}

		private async void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Hier brauche ich das MediaPreviewControl, von dem aus das Kontextmenü geöffnet wurde			
			var menuItem = sender as ToolStripItem;
			if (menuItem == null) return;
			var contextMenu = menuItem.Owner as ContextMenuStrip;
			if (contextMenu == null) return;
			var control = contextMenu.SourceControl as MediaPreviewControl;

			if (control == null)
			{
				Debug.Assert(false);
				return;
			}

			if (MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Question_NewMedia) != DialogResult.Yes)
				return;

			OpenFileDialog ofd = new OpenFileDialog();
			if (control.MediaType == eMediaType.Video)
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Video;
				ofd.Filter = "MP4 (*.mp4)|*.mp4";
			}
			else
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Image;
				ofd.Filter = "Png (*.png)|*.png|Jpeg (*.jpg)|*.jpg";
			}
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				await SetNewFile(control.MediaType, ofd.FileName);
			}
		}

		private async void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Hier brauche ich das MediaPreviewControl, von dem aus das Kontextmenü geöffnet wurde			
			var menuItem = sender as ToolStripItem;
			if (menuItem == null) return;
			var contextMenu = menuItem.Owner as ContextMenuStrip;
			if (contextMenu == null) return;
			var control = contextMenu.SourceControl as MediaPreviewControl;
			if (control == null)
			{
				Debug.Assert(false);
				return;
			}

			await DeleteMediaFile(control.MediaType, control.AbsolutPath, noqestion: false);
		}

		private async void MediaControl_DeleteMediaClicked(object? sender, MediaDeleteActionEventArgs e)
		{
			await DeleteMediaFile(e.MediaType, e.AbsolutPath, noqestion: true, e.DeleteOnlyXmlInfo);
		}

		private async Task SetNewFile(eMediaType type, string? sourcefile)
		{
			if (_selectedRoms == null || _selectedRoms.Count != 1 || string.IsNullOrEmpty(sourcefile) || !File.Exists(sourcefile))
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			var result = CopyOrMoveMediaFileToRom(false, sourcefile,
				$"./media/{RetroScrapOptions.GetMediaSettings(type)!.XmlFolderAndKey}/", baseDir);
			if (result.ok && !string.IsNullOrEmpty(result.file))
			{
				_selectedRoms[0].SetMediaPath(type, result.file);
				Log.Information($"SetNewFile() success: Rom: {_selectedRoms[0].FileName}, Type: {type.ToString()}, Mediafile: {result.file}");
				await SaveRomAsync();
				await SetRomOnGuiAsync(_selectedRoms[0], CancellationToken.None);
			}
			else
			{
				Log.Error($"SetNewFile() fail: Rom: {_selectedRoms[0].FileName}, Type: {type.ToString()}, Mediafile: {sourcefile}");
				MyMsgBox.ShowErr(Properties.Resources.Txt_Log_Scrap_Media_Move_Fail + " " + sourcefile);
			}
		}

		private async void MediaControl_NewMediaClicked(object? sender, MediaActionEventArgs e)
		{
			await SetNewFile(e.MediaType, e.AbsolutPath);
		}

		private void MediaControl_ViewMediaClicked(object? sender, MediaActionEventArgs e)
		{
			UiTools.OpenFileWithDefaultApp(e.AbsolutPath);
		}

		private async Task SetRomOnGuiAsync(GameEntry? rom, CancellationToken ct)
		{
			Log.Information($"SetRomOnGuiAsync: \"{rom?.FileName ?? "Null"}\"");
			if (rom == null)
			{
				flowLayoutPanelMedia.Controls.Clear();
				textBoxRomName.Text = string.Empty;
				textBoxRomDesc.Text = string.Empty;
				textBoxRomDetailsReleaseDate.Text = string.Empty;
				textBoxRomDetailsDeveloper.Text = string.Empty;
				textBoxRomDetailsPublisher.Text = string.Empty;
				textBoxRomDetailsGenre.Text = string.Empty;
				textBoxRomDetailsAnzPlayer.Text = string.Empty;
				starRatingControlRom.Rating = 0.0;
				buttonRomSave.Enabled = false;
				buttonRomScrap.Enabled = false;
				RomfavoriteToolStripMenuItem.Enabled = false;
				RomDetailsToolStripMenuItem.Enabled = false;
				return;
			}

			// Sofort Texte setzen
			textBoxRomName.Text = rom.Name ?? "";
			textBoxRomDesc.Text = rom.Description ?? "";
			textBoxRomDetailsReleaseDate.Text = rom.ReleaseDate?.ToShortDateString() ?? "";
			textBoxRomDetailsDeveloper.Text = rom.Developer ?? "";
			textBoxRomDetailsPublisher.Text = rom.Publisher ?? "";
			textBoxRomDetailsGenre.Text = rom.Genre ?? "";
			textBoxRomDetailsAnzPlayer.Text = rom.Players ?? "";
			starRatingControlRom.Rating = rom.RatingStars;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			buttonRomSave.Enabled = true;
			buttonRomScrap.Enabled = true;
			RomfavoriteToolStripMenuItem.Enabled = true;
			RomDetailsToolStripMenuItem.Enabled = true;

			// Medien im LayoutPanel initialisieren
			await UpdateMediaPanelAsync(rom, baseDir, ct);

		}

		private void SetStatusToolStripLabel(string text)
		{
			this.toolStripStatusLabelMain.Text = text;
		}

		private string? GetRomPath(int systemid)
		{
			if (string.IsNullOrEmpty(_gameManager.RomPath))
				return null;

			var systemFolder = _systems.GetRomFolder(systemid);
			if (_gameManager.RomPath.ToLower().EndsWith(systemFolder.ToLower()))
				return _gameManager.RomPath;

			var baseDir = Path.Combine(_gameManager.RomPath, systemFolder);
			return baseDir;
		}

		private string? GetSelectedRomPath()
		{
			if (_selectedSystem == null)
				return null;

			return GetRomPath(_selectedSystem.Id);
		}

		private async void cleanToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedSystem == null)
				return;

			if (MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Question_ClearSystem) == DialogResult.Yes)
			{
				Splash.ShowSplashScreen();
				await Splash.WaitForSplashScreenAsync();
				Splash.UpdateSplashScreenStatus(Properties.Resources.Txt_PleaseWait);
				var result = GameListLoader.CleanGamelistXmlByExistence(Path.Combine(GetSelectedRomPath()!, "gamelist.xml"));
				Splash.CloseSplashScreen();
				if (result.anzRomDelete > 0 || result.anzMediaDelete > 0)
				{
					MyMsgBox.Show($"{Properties.Resources.Txt_Msg_Info_CleaningFinish}\r\nRoms: {result.anzRomDelete}, Media: {result.anzMediaDelete}");
					await RefreshGameList();
				}
			}

		}

		private async void RomfavoriteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedSystem == null || GetSelectedRomPath() == null)
				return;
			if (_selectedRoms != null && _selectedRoms.Count > 0)
			{
				foreach (var rom in _selectedRoms)
				{
					if (rom.Favorite)
						rom.Favorite = false;
					else
						rom.Favorite = true;
					await Task.Run(() => _selectedSystem.SaveRomToGamelistXml(GetSelectedRomPath()!, rom));
				}
				await SetRomListSync();
			}
		}

		private async void RomGrouptoolStripMenuItem_Click(object sender, EventArgs e)
		{
			int selcount = listViewRoms.SelectedItems.Count;
			if (selcount > 1)
			{
				string nachfrage = Properties.Resources.Txt_Msg_Question_GroupFilesM3U + Environment.NewLine + Environment.NewLine;
				for (int i = 0; i < selcount; i++)
					nachfrage += $"- {((GameEntry)listViewRoms.SelectedItems[i].Tag!).FileName}{Environment.NewLine}";
				nachfrage += Environment.NewLine;
				nachfrage += Properties.Resources.Txt_Msg_Question_GroupFilesM3U_2;
				if (MyMsgBox.ShowQuestion(nachfrage) != DialogResult.Yes)
					return;

				var rompath = GetSelectedRomPath()!;
				int newindexentry = listViewRoms.SelectedItems[selcount - 1].Index + 1 - selcount;
				GameEntry? newEntry = GetSeletedGameList()!.GenerateM3uForSelectedGames(rompath, _selectedRoms!);
				if (newEntry != null )
				{
					foreach (var g in _selectedRoms!)
					{
						GameListLoader.DeleteGame(
								xmlp: rompath,
								rom: g,
								deleteAllReferences: true);
					}

					_selectedSystem!.SaveRomToGamelistXml(
								romPath: rompath,
								rom: newEntry);

					await SetRomListSync(newindexentry);
				}
			}
		}
	}

}

