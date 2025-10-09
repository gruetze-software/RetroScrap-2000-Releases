using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RetroScrap2000
{
	public partial class FormScrapAuto : Form
	{
		private CancellationTokenSource? _scrapCts;
		private ScrapperManager _scrapper;
		private RetroScrapOptions _options;
		private RetroSystem _system = new();
		private string _basedir;
		public GameList Roms { get; set; }
		public bool ScrapWasStarting { get; set; } = false;
		private bool _isrunning = false;

		public FormScrapAuto(GameList roms, ScrapperManager scrapper,
			string basedir, RetroSystem system, RetroScrapOptions options)
		{
			InitializeComponent();
			Roms = roms;
			_scrapper = scrapper;
			_basedir = basedir;
			_system = system;
			_options = options;
		}

		private void SetTitle(int index, int total)
		{
			this.Text = string.Format(Properties.Resources.Txt_Scrap_Auto_Title_FromSystem,
				index, total) + $" \"{Roms.RetroSys.Name_eu}\" ";
		}

		private void FormScrapAuto_Load(object sender, EventArgs e)
		{
			SetTitle(0, Roms.Games.Count);
			colMsg.Text = Properties.Resources.Txt_Column_ScrapAuto_Message;
			colTime.Text = Properties.Resources.Txt_Column_ScrapAuto_Time;
			colTyp.Text = Properties.Resources.Txt_Column_ScrapAuto_Type;


		}

		private async void buttonStart_Click(object sender, EventArgs e)
		{
			if (_isrunning)
			{
				_scrapCts?.Cancel();
				buttonStart.Text = "Start";
				checkBoxOnlyLocal.Enabled = true;
				_isrunning = false;
				return;
			}

			this.pictureBoxAniWait.Image = Properties.Resources.joystickani;
			buttonStart.Text = "Stop";
			checkBoxOnlyLocal.Enabled = false;
			_isrunning = true;

			_scrapCts?.Cancel();
			_scrapCts = new CancellationTokenSource();
			var ct = _scrapCts.Token;

			listViewMonitor.Items.Clear();
			this.pictureBoxAniWait.Image = Properties.Resources.joystickani;
			// Erstellen des Progress-Objekts. Die Action-Methode wird auf dem UI-Thread ausgeführt.
			var progressHandler = new Progress<ProgressObj>(report =>
			{
				if (report.ProgressPerc > 0 && report.ProgressPerc <= 100)
					progressBarScrap.Value = report.ProgressPerc;
				if (!string.IsNullOrEmpty(report.MessageText))
					AddProtokollItem(report);
			});

			try
			{
				ScrapWasStarting = true;

				// 1. Aufruf der asynchronen Methode und Übergabe des Progress-Objekts
				await _scrapper.ScrapGamesAsync(Roms, checkBoxOnlyLocal.Checked, Roms.RetroSys.Id, _basedir,
						progressHandler, _options, ct);

				// ----------------------------------------------------------------------
				// 2. KONSOLIDIERTES SPEICHERN (EINMALIGER AUFRUF) 💾
				// ----------------------------------------------------------------------

				// Prüfen, ob der Scraping-Vorgang abgebrochen wurde.
				// Obwohl der Catch-Block die TaskCanceledException fängt, 
				// wird die Prüfung hier sichergestellt, falls der Abbruch direkt nach dem await erfolgt.
				ct.ThrowIfCancellationRequested();

				AddProtokollItem(ProgressObj.eTyp.Info, Properties.Resources.Txt_Log_Scrap_SaveGameList);
				bool ok = false;

				// Führe die einmalige Speicherung asynchron aus
				ok = await Task.Run(() =>
				{
					// Interne Prüfung im Task, falls der Abbruch während des Wartens/Speicherns erfolgt.
					ct.ThrowIfCancellationRequested();

					// Aufruf der effizienten ALL-IN-ONE-Speichermethode
					return _system.SaveAllScrapedRomsToGamelistXml(_basedir, Roms.Games);

				}, ct); // WICHTIG: ct an Task.Run übergeben

				if (ok)
				{
					AddProtokollItem(ProgressObj.eTyp.Info, Properties.Resources.Txt_Log_Scrap_SaveGamelist_success);
				}
				else
				{
					// Sollte nur bei einem Fehler in SaveAllScrapedRomsToGamelistXml erreicht werden, 
					// die keine Exception wirft (was nicht ideal ist, aber die Logik hier abfängt).
					AddProtokollItem(ProgressObj.eTyp.Error, Properties.Resources.Txt_Log_Scrap_SaveGamelist_fail);
				}

			}
			catch (TaskCanceledException) // <-- FÄNGT DEN ABBRUCH AB
			{
				// Dies ist der erwartete Abbruch, wenn der Benutzer auf "Stop" klickt.
				AddProtokollItem(ProgressObj.eTyp.Warning, Properties.Resources.Txt_Log_Scrap_CancelRequest);
			}
			catch (Exception ex)
			{
				// Fängt alle anderen Fehler (z.B. IO-Fehler beim Speichern, wenn nicht als Tce abgefangen)
				MyMsgBox.ShowErr($"{Utils.GetExcMsg(ex)}");
			}
			finally
			{
				progressBarScrap.Value = 0;
				this.pictureBoxAniWait.Image = null;
				this.buttonStart.Text = "Start";
				// WICHTIG: Sicherstellen, dass die UI-Elemente auch hier zurückgesetzt werden, 
				// da das 'return' am Anfang nur den Start verhindert.
				checkBoxOnlyLocal.Enabled = true;
				_isrunning = false;
			}
		}

		private void AddProtokollItem(ProgressObj report)
		{
			ListViewItem it = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
			it.SubItems.Add(report.Typ.ToString());
			it.SubItems.Add(report.RomNumber.ToString());
			it.SubItems.Add(report.RomName);
			it.SubItems.Add(report.MessageText);
			it.ImageKey = report.Typ.ToString();
			if (report.Typ == ProgressObj.eTyp.Error)
				it.ForeColor = Color.Red;
			else if (report.Typ == ProgressObj.eTyp.Warning)
				it.ForeColor = Color.Orange;
			listViewMonitor.Items.Add(it);
			listViewMonitor.EnsureVisible(listViewMonitor.Items.Count - 1);
			SetTitle(report.RomNumber, Roms.Games.Count);
		}

		private void AddProtokollItem(ProgressObj.eTyp typ, string message)
		{
			AddProtokollItem(new ProgressObj(typ, -1, message));
		}

		private void buttonStopp_Click(object sender, EventArgs e)
		{
			_scrapCts?.Cancel();
			_scrapCts = new CancellationTokenSource();
		}

		private void FormScrapAuto_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				UseWaitCursor = true;
				e.Cancel = true;
				_scrapCts?.CancelAsync().Wait(2000);
				e.Cancel = false;
				UseWaitCursor = false;
			}
		}
	}
}
