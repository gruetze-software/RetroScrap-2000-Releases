using RetroScrap2000.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;

namespace RetroScrap2000
{
	public partial class FormMain : Form
	{
		RetroScrapOptions _options = new RetroScrapOptions();

		ScrapperManager _scrapper = new ScrapperManager();

		RetroSystems _systems = new RetroSystems();
		GameManager _gameManager = new GameManager();
		GameEntry? _selectedRom = null;

		private System.Timers.Timer _loadTimer = new();
		private int _romSortCol = -1;
		private SortOrder _romSortOrder = SortOrder.None;

		private int _systemSortCol = -1;
		private SortOrder _systemSortOrder = SortOrder.None;

		// Images caching damit nicht immer von Platte gelesen werden muss
		private readonly Dictionary<string, Image> _imageCache = new(StringComparer.OrdinalIgnoreCase);

		public FormMain(RetroScrapOptions options)
		{
			InitializeComponent();
			_options = options;

			_loadTimer.Interval = 500;
			_loadTimer.Elapsed += LoadTimer_Elapsed;
			_loadTimer.Enabled = false;

			// DoubleBuffering für flüssiges Zeichnen
			typeof(ListView).GetProperty("DoubleBuffered",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					?.SetValue(listViewSystems, true);

			typeof(ListView).GetProperty("DoubleBuffered",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					?.SetValue(listViewRoms, true);
		}

		// Change the event handler signature for System.Timers.Timer.Elapsed to match the expected delegate type.
		// The Elapsed event expects a method with the following signature:
		// void Handler(object? sender, System.Timers.ElapsedEventArgs e)

		private void LoadTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			_loadTimer.Enabled = false;
			_loadTimer.Stop();

			// Den gesamten UI-Code-Block auf dem Haupt-UI-Thread ausführen
			this.Invoke(new Action(async () =>
			{
				Splash.ShowSplashScreen();
				// Warte, bis der Splashscreen vollständig initialisiert ist
				await Splash.WaitForSplashScreenAsync();
				// Statusmeldungen mit Wartezeit
				await Splash.ShowStatusWithDelayAsync("Lade Einstellungen...", 1000);
				if (_options != null && _options.Secret != null
				 && _options!.Secret!.TryLoad(out string? pwd)
				 && !string.IsNullOrEmpty(pwd)
				 && !string.IsNullOrEmpty(_options.ApiUser))
				{
					_scrapper.RefreshSecrets(_options!.ApiUser!, pwd!);
					if (!string.IsNullOrEmpty(_options.RomPath)
					&& Directory.Exists(_options.RomPath))
						await LoadRomsAsync();
				}
				else
				{
					Splash.CloseSplashScreen();
					MyMsgBox.Show(Properties.Resources.Txt_Msg_StartAppNoSsUser);
					OpenOptionsFrm();
				}
			}));
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
			_systems = RetroSystems.Load();
			//await _systems.SetSystemsFromApiAsync(_scrapper);
			//_systems.Save();

			var imgList = new ImageList { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
			var baseDir = RetroSystems.FolderIcons;
			if (Directory.Exists(baseDir))
			{
				foreach (var file in Directory.GetFiles(baseDir, "*.png"))
				{
					var filename = Path.GetFileName(file);
					var key = filename.Substring(0, filename.IndexOf(".")).ToLower();
					using var img = Image.FromFile(file);
					imgList.Images.Add(key, new Bitmap(img)); // Kopie -> Datei wird nicht gelockt
				}
			}
			listViewSystems.SmallImageList = imgList;

			_loadTimer.Enabled = true;
			_loadTimer.Start();
		}

		private void SetTitleMainForm()
		{
			this.Text = Assembly.GetExecutingAssembly().GetName().Name + String.Format(" - Version: {0}",
					Assembly.GetExecutingAssembly().GetName().Version);

			this.Text = this.Text += "    Roms: \"" + _options.RomPath + "\"";
		}

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

		// Diese Methode ist asynchron und gibt einen Task zurück
		private async Task LoadRomsAsync()
		{
			if (!Directory.Exists(_options.RomPath))
				return;

			SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_ReadRoms);
			listViewSystems.BeginUpdate();
			listViewRoms.BeginUpdate();
			listViewSystems.Items.Clear();
			listViewRoms.Items.Clear();
			_selectedRom = null;
			await SetRomOnGuiAsync(null, CancellationToken.None);

			Splash.ShowSplashScreen();
			await Splash.WaitForSplashScreenAsync();
			await Splash.ShowStatusWithDelayAsync("Lade Systeme...", 500);
			try
			{
				await Task.Run(() => _gameManager.Load(_options.RomPath, _systems));

				int total = _gameManager.SystemList.Count;
				var items = new List<ListViewItem>(total);
				for (int i = 0; i < total; i++)
				{
					var kv = _gameManager.SystemList.ElementAt(i);
					Splash.UpdateSplashScreenStatus($"Lade \"{kv.Key.ToUpper()}\"... ({i + 1}/{total})");
					Splash.UpdateSplashScreenProgress(Utils.CalculatePercentage(i + 1, total));
					Thread.Sleep(100);

					var roms = kv.Value;
					var sys = roms.RetroSys;
					if (sys == null) continue;

					var it = new ListViewItem(sys.Name)
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
				listViewSystems.Items.AddRange(items.ToArray());
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

		private async void buttonRomsRead_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(_options.RomPath))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Rom_WrongPath);
				return;
			}

			await LoadRomsAsync();
		}

		private void alleRomsAktualisierenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
				return;

			var systemFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
			var baseDir = Path.Combine(_gameManager.RomPath!, systemFolder);
			var gamelist = GetSeletedGameList();
			if (gamelist == null)
				return;

			FormScrapAuto frm = new FormScrapAuto(gamelist, _scrapper, baseDir, _systems, _options);
			frm.ShowDialog();
		}

		private void OpenOptionsFrm()
		{
			FormOptions frm = new FormOptions(_options, _scrapper);
			if (frm.ShowDialog() == DialogResult.OK)
			{
				_options.Save();
			}
		}

		private void buttonOptions_Click(object sender, EventArgs e)
		{
			OpenOptionsFrm();
		}

		private GameList? GetSeletedGameList()
		{
			return (listViewSystems.SelectedItems.Count == 1)
						? listViewSystems.SelectedItems[0].Tag as GameList
						: null;
		}
		
		private CancellationTokenSource? _sysSelCts;
		private async Task SetRomListFromSelectedSystem()
		{
			// Selektion lesen: GameList des Systems aus Tag
			var roms = GetSeletedGameList();
			var system = roms?.RetroSys;

			// laufende System-Ladevorgänge abbrechen
			_sysSelCts?.Cancel();
			_sysSelCts = new CancellationTokenSource();
			var sysCt = _sysSelCts.Token;

			// UI: Systemanzeige aktualisieren (Banner + Infos)
			try
			{
				await SetSystemOnGuiAsync(system, sysCt);
			}
			catch (OperationCanceledException) { /* ok */ }

			// ROM-Liste neu füllen (gebatcht, kein Async nötig – nur UI)
			SetStatusToolStripLabel(string.Format(Properties.Resources.Txt_Status_Label_ReadSystem, system?.Name));
			listViewRoms.BeginUpdate();
			try
			{
				listViewRoms.Items.Clear();
				_selectedRom = null;
				await SetRomOnGuiAsync(null, CancellationToken.None); // Details rechts leeren

				if (roms == null) return;

				var items = new List<ListViewItem>(roms.Games.Count);
				foreach (var g in roms.Games)
				{
					var it = new ListViewItem(g.Name ?? Path.GetFileName(g.Path ?? ""));
					it.SubItems.Add(g.ReleaseDate?.ToString("yyyy") ?? "");
					it.SubItems.Add(g.Genre);
					it.SubItems.Add(g.Players);
					it.SubItems.Add(g.RatingStars.ToString("0.0"));
					it.SubItems.Add(Path.GetFileName(g.Path ?? ""));
					it.Tag = g;
					items.Add(it);
				}
				listViewRoms.Items.AddRange(items.ToArray());
			}
			finally
			{
				listViewRoms.EndUpdate();
				// **Hier wird der erste Eintrag der Rom-Liste ausgewählt**
				// Warten Sie, bis die Liste gefüllt wurde
				if (listViewRoms.Items.Count > 0)
				{
					listViewRoms.Items[0].Selected = true;
					listViewRoms.Items[0].Focused = true;
				}
				SetStatusToolStripLabel(string.Format(Properties.Resources.Txt_Status_Label_AnzRomsLoad,
					roms?.Games.Count));
			}
		}

		
		private async void listViewSystems_SelectedIndexChanged(object sender, EventArgs e)
		{
			await SetRomListFromSelectedSystem();
		}

		private CancellationTokenSource? _romSelCts;
		private async void listViewRoms_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Selektion auslesen
			var rom = (listViewRoms.SelectedItems.Count == 1)
					? listViewRoms.SelectedItems[0].Tag as GameEntry
					: null;

			// laufende Jobs abbrechen & neuen Token erzeugen
			_romSelCts?.Cancel();
			_romSelCts = new CancellationTokenSource();
			var ct = _romSelCts.Token;

			_selectedRom = rom;

			try
			{
				// alles weitere (Busy, Bilder/Videos laden, Textfelder setzen)
				// passiert in SetRomOnGuiAsync
				await SetRomOnGuiAsync(rom, ct);
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

		private void pictureBoxImgVideo_Click(object sender, EventArgs e)
		{
			UiTools.OpenPicBoxTagFile((PictureBox)sender);
		}

		private async void buttonRomScrap_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return;
			}

			var sysFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
			if (string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoSystem);
				return;
			}

			// Rom-Basisname als romnom
			var romFileName = Path.GetFileName(_selectedRom.Name ?? _selectedRom.Path ?? "");
			if (string.IsNullOrWhiteSpace(romFileName))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_WrongRomFile);
				return;
			}

			buttonRomScrap.Enabled = false;
			try
			{
				Splash.ShowSplashScreen();
				await Splash.WaitForSplashScreenAsync();

				SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Scrap_Running);
				await Splash.ShowStatusWithDelayAsync(string.Format(
					Properties.Resources.Txt_Status_Label_Scrap_Running, _selectedRom.Name), 300);

				var (ok, data, err) = await _scrapper.GetGameAsync(romFileName, _selectedRom.RetroSystemId, _options.GetLanguageShortCode());
				if (!ok || data == null)
				{
					Splash.CloseSplashScreen();
					MyMsgBox.ShowErr($"\"{romFileName}\": " + Properties.Resources.Txt_Msg_Scrap_Fail + "\r\n\r\n" + err);
					SetStatusToolStripLabel($"\"{_selectedRom.Name}\": {err}.");
					return;
				}

				var systemFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
				var baseDir = Path.Combine(_gameManager.RomPath!, systemFolder);

				// Dialog zum Übernehmen der Daten
				Splash.CloseSplashScreen();
				using var dlg = new FormScrapRom(baseDir, _selectedRom, data);
				if (dlg.ShowDialog(this) != DialogResult.OK)
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Ready);
					return;
				}
				var sel = dlg.Selection;
				var d = sel.NewData!;
				// Felder übernehmen
				if (sel.TakeName) _selectedRom.Name = d.Name;
				if (sel.TakeDesc) _selectedRom.Description = d.Description;
				if (sel.TakeGenre) _selectedRom.Genre = d.Genre;
				if (sel.TakePlayers) _selectedRom.Players = d.Players;
				if (sel.TakeDev) _selectedRom.Developer = d.Developer;
				if (sel.TakePub) _selectedRom.Publisher = d.Publisher;
				if (sel.TakeRelease) _selectedRom.ReleaseDate = d.ReleaseDate;
				if (sel.TakeRating) _selectedRom.Rating = d.RatingNormalized.HasValue ? d.RatingNormalized.Value : 0.0;


				if (sel.TakeMediaBox)
				{
					MoveScrapImageCoverRom(sel, baseDir);
				}
				if (sel.TakeMediaScreen)
				{
					MoveScrapImageScreenshotRom(sel, baseDir);
				}
				if (sel.TakeMediaVideo)
				{
					MoveScrapVideoRom(sel, baseDir);
				}

				// UI aktualisieren
				await SetRomOnGuiAsync(_selectedRom, CancellationToken.None);
				await SaveRom();
			}
			catch (Exception ex)
			{
				Splash.CloseSplashScreen();
				MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
				SetStatusToolStripLabel(Properties.Resources.Txt_Msg_Scrap_Fail);
			}
			finally
			{
				Splash.CloseSplashScreen();
				buttonRomScrap.Enabled = true;
			}
		}

		private async Task SaveRom()
		{
			if (_selectedRom == null)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return;
			}

			// System-Folder ermitteln (z. B. "amiga500")
			var sysFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);

			if (string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoSystem);
				return;
			}

			// GUI -> Model (nur Felder, die du im UI editieren lässt)
			_selectedRom.Name = textBoxRomName.Text?.Trim();
			_selectedRom.Description = textBoxRomDesc.Text?.Trim();
			_selectedRom.Genre = textBoxRomDetailsGenre.Text?.Trim();
			_selectedRom.Players = textBoxRomDetailsAnzPlayer.Text?.Trim();
			_selectedRom.Developer = textBoxRomDetailsDeveloper.Text?.Trim();
			_selectedRom.Publisher = textBoxRomDetailsPublisher.Text?.Trim();
			// RatingStars ist read-only? Falls editierbar: (0..5) -> 0..1
			_selectedRom.Rating = Math.Clamp(starRatingControlRom.Rating / 5.0, 0.0, 1.0);

			// Releasedate: nimm, was in der GUI steht (optional)
			if (DateTime.TryParse(textBoxRomDetailsReleaseDate.Text, out var rd))
				_selectedRom.ReleaseDate = rd;

			try
			{
				buttonRomSave.Enabled = false;

				bool ok = await Task.Run(() =>
						_systems.SaveRomToGamelistXml(
								romRoot: _gameManager.RomPath!,
								systemFolder: sysFolder,
								rom: _selectedRom
						)
				);
				if (ok)
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomSave);
				else
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomNotSaved);

			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Status_Label_RomNotSaved + "\r\n\r\n" + Utils.GetExcMsg(ex));
				SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomNotSaved);
			}
			finally
			{
				buttonRomSave.Enabled = true;
			}
		}

		private async void buttonRomSave_Click(object sender, EventArgs e)
		{
			await SaveRom();
		}

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Image Cache aufräumen
			foreach (var kv in _imageCache)
			{
				kv.Value.Dispose(); // gibt Ressourcen frei
			}
			_imageCache.Clear();
		}

		private void MoveScrapImageCoverRom(ScrapeSelection sel, string basedir)
		{
			if (_selectedRom != null && sel != null)
			{
				var result = FileTools.MoveScrapFileRom(_selectedRom.Name,
					sel.MediaBoxTempPath,
					basedir,
					"./media/box2dfront/",
					".png");

				if (result.ok && !string.IsNullOrEmpty(result.file))
					_selectedRom.MediaCoverPath = result.file;
			}
		}

		private void MoveScrapImageScreenshotRom(ScrapeSelection sel, string basedir)
		{
			if (_selectedRom != null && sel != null)
			{
				var result = FileTools.MoveScrapFileRom(_selectedRom.Name,
					sel.MediaScreenTempPath,
					basedir,
					"./media/images/",
					".png");

				if (result.ok && !string.IsNullOrEmpty(result.file))
					_selectedRom.MediaScreenshotPath = result.file;
			}
		}

		private void MoveScrapVideoRom(ScrapeSelection sel, string basedir)
		{
			if (_selectedRom != null && sel != null)
			{
				var result = FileTools.MoveScrapFileRom(_selectedRom.Name,
					sel.MediaVideoTempPath,
					basedir,
					"./media/videos/",
					".mp4");

				if (result.ok && !string.IsNullOrEmpty(result.file))
					_selectedRom.MediaVideoPath = result.file;
			}
		}

		private void SortListview(ListView lst, int newsortcolumn, ref int aktsortcolumn,
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
			if (system == null)
			{
				pictureBoxRomSystem.Image = null;
				listBoxSystem.Items.Clear();
				return;
			}

			// Textinfos sofort
			listBoxSystem.BeginUpdate();
			listBoxSystem.Items.Clear();
			listBoxSystem.Items.Add(system.Name);
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

		private async Task SetRomOnGuiAsync(GameEntry? rom, CancellationToken ct)
		{
			pictureBoxImgBox.Image = null;
			pictureBoxImgBox.Tag = null;
			pictureBoxImgScreenshot.Image = null;
			pictureBoxImgScreenshot.Tag = null;
			pictureBoxImgVideo.Image = null;
			pictureBoxImgVideo.Tag = null;

			if (rom == null)
			{
				textBoxRomName.Text = string.Empty;
				textBoxRomDesc.Text = string.Empty;
				textBoxRomDetailsReleaseDate.Text = string.Empty;
				textBoxRomDetailsDeveloper.Text = string.Empty;
				textBoxRomDetailsPublisher.Text = string.Empty;
				textBoxRomDetailsGenre.Text = string.Empty;
				textBoxRomDetailsAnzPlayer.Text = string.Empty;
				starRatingControlRom.Rating = 0.0;
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

			// Busy-Platzhalter anzeigen
			UiTools.ShowBusyPreview(pictureBoxImgBox, "Cover …");
			UiTools.ShowBusyPreview(pictureBoxImgScreenshot, "Screenshot …");
			UiTools.ShowBusyPreview(pictureBoxImgVideo, "Video-Vorschau …");

			var systemFolder = _systems.GetRomFolder(rom.RetroSystemId);
			var baseDir = Path.Combine(_gameManager.RomPath!, systemFolder);

			try
			{
				// asynchron + gecached
				var coverTask = ImageTools.LoadImageCachedAsync(baseDir, rom.MediaCoverPath, ct);
				var shotTask = ImageTools.LoadImageCachedAsync(baseDir, rom.MediaScreenshotPath, ct);
				var prevTask = ImageTools.LoadVideoPreviewAsync(baseDir, rom, ct);

				var cover = await coverTask;
				if (!ct.IsCancellationRequested)
				{
					UiTools.HideBusyPreview(pictureBoxImgBox);
					pictureBoxImgBox.Image = cover;
					pictureBoxImgBox.Tag = FileTools.ResolveMediaPath(baseDir, rom.MediaCoverPath);
				}

				var shot = await shotTask;
				if (!ct.IsCancellationRequested)
				{
					UiTools.HideBusyPreview(pictureBoxImgScreenshot);
					pictureBoxImgScreenshot.Image = shot;
					pictureBoxImgScreenshot.Tag = FileTools.ResolveMediaPath(baseDir, rom.MediaScreenshotPath);
				}

				var prev = await prevTask;
				if (!ct.IsCancellationRequested)
				{
					UiTools.HideBusyPreview(pictureBoxImgVideo);
					pictureBoxImgVideo.Image = prev?.overlay;
					pictureBoxImgVideo.Tag = prev?.videoAbsPath; // Klick → Standardplayer
					pictureBoxImgVideo.Cursor = (prev?.overlay != null) ? Cursors.Hand : Cursors.Default;
				}
			}
			catch (OperationCanceledException)
			{
				// okay – wird vom Aufrufer gefangen
				throw;
			}
		}

		private void SetStatusToolStripLabel(string text)
		{
			this.toolStripStatusLabelMain.Text = text;
		}

		private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
				return;
			var sysFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
			if (string.IsNullOrEmpty(sysFolder))
				return;

			var baseDir = Path.Combine(_gameManager.RomPath!, sysFolder);
			var romFileName = Path.GetFileName(_selectedRom.Path ?? _selectedRom.Name ?? "");
			if (string.IsNullOrEmpty(romFileName))
				return;

			FormRomDetails frm = new FormRomDetails(_selectedRom, Path.Combine(baseDir, romFileName));
			frm.ShowDialog();
		}

		private async void löschenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
				return;

			var sysFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
			if (string.IsNullOrEmpty(sysFolder))
				return;

			var baseDir = Path.Combine(_gameManager.RomPath!, sysFolder);
			var romFileName = Path.GetFileName(_selectedRom.Path ?? _selectedRom.Name ?? "");
			if (string.IsNullOrEmpty(romFileName))
				return;

			var file = Path.Combine(baseDir, romFileName);
			if (MyMsgBox.ShowQuestion(string.Format(
				"Soll die Datei \"{0}\" mit allen Informationen dazu wirklich gelöscht werden?",
				file)) != DialogResult.Yes)
				return;

			try
			{
				Trace.WriteLine($"Lösche Rom-Datei \"{file}\" ...");
				File.Delete(file);
				Trace.WriteLine("... Rom-Datei gelöscht.");
				Trace.WriteLine("Gamelist.xml updaten ...");
				GameListLoader.DeleteGameByPath(
					xmlPath: Path.Combine(baseDir, "gamelist.xml"),
					romRelPath: _selectedRom.Path ?? romFileName
					);
				Trace.WriteLine("... Gamelist.xml upgedated.");
				// Liste neu laden
				GetSeletedGameList()?.Games.Remove(_selectedRom);
				var games = GameListLoader.Load(Path.Combine(baseDir, "gamelist.xml"),
					GetSeletedGameList()?.RetroSys);
				// Liste neu setzen
				GetSeletedGameList()?.Games.AddRange( games?.Games);
				// UI neu setzen
				await SetRomListFromSelectedSystem();
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr("Fehler beim Löschen der Rom-Datei:\r\n\r\n" + Utils.GetExcMsg(ex));
			}
		}
	}
}
