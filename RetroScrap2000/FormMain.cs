using RetroScrap2000.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
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

		#region Form-Events
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
				
				_systems = RetroSystems.Load();
				if (_systems.SystemList.Count == 0)
				{
					await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Splash_Initializing, 100);
					await _systems.SetSystemsFromApiAsync(_scrapper);
					_systems.Save();
				}
				// Statusmeldungen mit Wartezeit
				await Splash.ShowStatusWithDelayAsync(Properties.Resources.Txt_Splash_LoadingSettings, 1000);
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
					OpenOptionsFrm(1);
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
			var info = Utils.GetAppInfo();
			this.Text = $"{info.product} by {info.company} - Version {info.version}";
			if ( !string.IsNullOrEmpty(_options.RomPath))
				this.Text += "    Roms: \"" + _options.RomPath + "\"";
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
			PictureBox pb = (PictureBox)sender;
			if (pb.Image == null || pb.Tag == null)
				NewMediaAsync(pb);
			else
				OpenMedia(pb);
		}

		private async Task ScrapRomAsync()
		{
			var sysFolder = GetSelectedRomPath();
			if (_selectedRom == null || string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
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
					if (err == "HTTP 404: Erreur : Rom/Iso/Dossier non trouvée !  ")
						err = Properties.Resources.Txt_Msg_Scrap_NoDataFound;
					MyMsgBox.ShowErr($"\"{romFileName}\": " + Properties.Resources.Txt_Msg_Scrap_Fail + "\r\n\r\n" + err);
					SetStatusToolStripLabel($"\"{_selectedRom.Name}\": {err}.");
					return;
				}


				data.Source = "ScreenScraper.fr";

				// Dialog zum Übernehmen der Daten
				Splash.CloseSplashScreen();
				using var dlg = new FormScrapRom(sysFolder, _selectedRom, data);
				if (dlg.ShowDialog(this) != DialogResult.OK)
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_Ready);
					return;
				}
				var sel = dlg.Selection;
				var d = sel.NewData!;
				// Felder übernehmen
				if (!string.IsNullOrEmpty(d.Id) && int.TryParse(d.Id, out int id) && id != _selectedRom.Id)
					_selectedRom.Id = id;
				if (string.IsNullOrEmpty(d.Source) || d.Source != _selectedRom.Source)
					_selectedRom.Source = d.Source;
				if (sel.TakeName) _selectedRom.Name = d.Name;
				if (sel.TakeDesc) _selectedRom.Description = d.Description;
				if (sel.TakeGenre) _selectedRom.Genre = d.Genre;
				if (sel.TakePlayers) _selectedRom.Players = d.Players;
				if (sel.TakeDev) _selectedRom.Developer = d.Developer;
				if (sel.TakePub) _selectedRom.Publisher = d.Publisher;
				if (sel.TakeRelease) _selectedRom.ReleaseDate = d.ReleaseDate;
				if (sel.TakeRating) _selectedRom.Rating = d.RatingNormalized.HasValue ? d.RatingNormalized.Value : 0.0;


				if (sel.TakeMediaBox && !string.IsNullOrEmpty(sel.MediaBoxTempPath))
				{
					MoveOrCopyScrapImageCoverRom(true, sel.MediaBoxTempPath, sysFolder);
				}
				if (sel.TakeMediaScreen && !string.IsNullOrEmpty(sel.MediaScreenTempPath))
				{
					MoveOrCopyScrapImageScreenshotRom(true, sel.MediaScreenTempPath, sysFolder);
				}
				if (sel.TakeMediaVideo && !string.IsNullOrEmpty(sel.MediaVideoTempPath))
				{
					MoveOrCopyScrapVideoRom(true, sel.MediaVideoTempPath, sysFolder);
				}

				// UI aktualisieren
				await SetRomOnGuiAsync(_selectedRom, CancellationToken.None);
				await SaveRomAsync();
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

		private async void buttonRomScrap_Click(object sender, EventArgs e)
		{
			await ScrapRomAsync();
		}

		private async void buttonRomSave_Click(object sender, EventArgs e)
		{
			await SaveRomAsync();
		}

		private async void SystemAllRomsScrapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
				return;

			var baseDir = GetSelectedRomPath();
			if ( string.IsNullOrEmpty(baseDir))
				return;

			var gamelist = GetSeletedGameList();
			if (gamelist == null)
				return;

			FormScrapAuto frm = new FormScrapAuto(gamelist, _scrapper, baseDir, _systems, _options);
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
			if (_selectedRom == null)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			var romFileName = Path.GetFileName(_selectedRom.Path ?? _selectedRom.Name ?? "");
			if (string.IsNullOrEmpty(romFileName))
				return;

			FormRomDetails frm = new FormRomDetails(_selectedRom, Path.Combine(baseDir, romFileName));
			frm.ShowDialog();
		}

		private async void RomLöschenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_selectedRom == null)
				return;

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

			var romFileName = Path.GetFileName(_selectedRom.Path ?? _selectedRom.Name ?? "");
			if (string.IsNullOrEmpty(romFileName))
				return;

			var file = Path.Combine(baseDir, romFileName);
			if (MyMsgBox.ShowQuestion(string.Format( Properties.Resources.Txt_Msg_Qestion_DeleteRom, file)) 
				!= DialogResult.Yes)
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
				var loadres = GameListLoader.Load(Path.Combine(baseDir, "gamelist.xml"),
					GetSeletedGameList()?.RetroSys);
				// Liste neu setzen
				GetSeletedGameList()?.Games.AddRange(loadres.Games.Games);
				// UI neu setzen
				await SetRomListSync();
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
			}
		}

		private async void RomScrapToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await ScrapRomAsync();
		}

		private PictureBox? GetPictureBoxFromMenuItem(object sender)
		{
			var menuItem = sender as ToolStripMenuItem;
			if (menuItem?.Owner is ContextMenuStrip contextMenu)
			{
				// Die SourceControl-Eigenschaft enthält die Picturebox
				return contextMenu.SourceControl as PictureBox;
			}
			return null;
		}

		private void MediaAnzeigenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pictureBox = GetPictureBoxFromMenuItem(sender);
			if (pictureBox != null)
				OpenMedia(pictureBox);
		}

		public void OpenMedia(PictureBox pb)
		{
			if (pb.Image == null || pb.Tag == null)
				return;
			UiTools.OpenPicBoxTagFile(pb);
		}

		public async void NewMediaAsync(PictureBox pb)
		{
			bool video = pb == pictureBoxImgVideo;
			OpenFileDialog ofd = new OpenFileDialog();
			if (video)
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Image;
				ofd.Filter = "MP4 (*.mp4)|*.mp4";
			}
			else
			{
				ofd.Title = Properties.Resources.Txt_Dlg_Select_Video;
				ofd.Filter = "Png (*.png)|*.png|Jpeg (*.jpg)|*.jpg";
			}
			if (ofd.ShowDialog() == DialogResult.OK && _selectedRom != null )
			{
				var baseDir = GetSelectedRomPath();
				if (string.IsNullOrEmpty(baseDir))
					return;

				if (pb == pictureBoxImgBox)
				{
					MoveOrCopyScrapImageCoverRom(false, ofd.FileName, baseDir);
					pb.Tag = FileTools.ResolveMediaPath(baseDir, _selectedRom.MediaCoverPath);
				}
				else if (pb == pictureBoxImgScreenshot)
				{
					MoveOrCopyScrapImageScreenshotRom(false, ofd.FileName, baseDir);
					pb.Tag = FileTools.ResolveMediaPath(baseDir, _selectedRom.MediaScreenshotPath);
				}
				else if (pb == pictureBoxImgVideo)
				{
					MoveOrCopyScrapVideoRom(false, ofd.FileName, baseDir);
					pb.Tag = FileTools.ResolveMediaPath(baseDir, _selectedRom.MediaVideoPath);
				}
				else
					Debug.Assert(false);

				await SaveRomAsync();
				await SetRomOnGuiAsync(_selectedRom, CancellationToken.None);
			}
		}
		
		private void MediaNeuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pictureBox = GetPictureBoxFromMenuItem(sender);
			if (pictureBox != null)
				NewMediaAsync(pictureBox);
		}

		private void MediaLöschenToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}
		
		private void OpenOptionsFrm(int tabpageindex = 0)
		{
			FormOptions frm = new FormOptions(_options, _scrapper, tabpageindex);
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

		private async Task SetRomListSync()
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

		private async Task SaveRomAsync()
		{
			if (_selectedRom == null)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
				return;
			}

			var sysFolder = GetSelectedRomPath();
			if (string.IsNullOrEmpty(sysFolder))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Scrap_NoRom);
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
								romPath: sysFolder,
								rom: _selectedRom
						)
				);
				if (ok)
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomSave);
					var selitem = listViewRoms.SelectedItems[0];
					selitem.SubItems[0].Text = _selectedRom.Name ?? Path.GetFileName(_selectedRom.Path ?? "");
					selitem.SubItems[1].Text = _selectedRom.ReleaseDate?.ToString("yyyy") ?? "";
					selitem.SubItems[2].Text = _selectedRom.Genre;
					selitem.SubItems[3].Text = _selectedRom.Players;
					selitem.SubItems[4].Text = _selectedRom.RatingStars.ToString("0.0");
					selitem.SubItems[5].Text = Path.GetFileName(_selectedRom.Path ?? "");
					selitem.Tag = _selectedRom;
				}
				else
				{
					SetStatusToolStripLabel(Properties.Resources.Txt_Status_Label_RomNotSaved);
				}

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

		private (bool ok, string? file) CopyOrMoveMediaFileToRom(bool movefile, string sourcefile,
			string relMediaRomPath, string basedir)
		{
			(bool ok, string? file) result = (false, null);

			if (_selectedRom != null && !string.IsNullOrEmpty(sourcefile) && File.Exists(sourcefile))
			{
				result = FileTools.MoveOrCopyScrapFileRom(
					move: movefile,
					romname: _selectedRom.Name,
					sourcefile: sourcefile,
					destbasedir: basedir,
					destrelpath: relMediaRomPath);
			}
			return result;

		}

		private void MoveOrCopyScrapImageCoverRom(bool movefile, string sourceFile, string basedir)
		{
			var result = CopyOrMoveMediaFileToRom(movefile, sourceFile, "./media/box2dfront/", basedir);

			if (result.ok && !string.IsNullOrEmpty(result.file))
				_selectedRom!.MediaCoverPath = result.file;
		}

		private void MoveOrCopyScrapImageScreenshotRom(bool movefile, string sourceFile, string basedir)
		{
			var result = CopyOrMoveMediaFileToRom(movefile, sourceFile, "./media/images/", basedir);

			if (result.ok && !string.IsNullOrEmpty(result.file))
				_selectedRom!.MediaScreenshotPath = result.file;
		}

		private void MoveOrCopyScrapVideoRom(bool movefile, string sourceFile, string basedir)
		{
			var result = CopyOrMoveMediaFileToRom(movefile, sourceFile, "./media/videos/", basedir);

			if (result.ok && !string.IsNullOrEmpty(result.file))
				_selectedRom!.MediaVideoPath = result.file;
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
			UiTools.ShowBusyPreview(pictureBoxImgVideo, "Video …");

			var baseDir = GetSelectedRomPath();
			if (string.IsNullOrEmpty(baseDir))
				return;

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

		private string? GetSelectedRomPath()
		{
			if ( _selectedRom == null || string.IsNullOrEmpty(_gameManager.RomPath))
				return null;

			var systemFolder = _systems.GetRomFolder(_selectedRom.RetroSystemId);
			var baseDir = Path.Combine(_gameManager.RomPath, systemFolder);

			return baseDir;
		}
		
	}
}
