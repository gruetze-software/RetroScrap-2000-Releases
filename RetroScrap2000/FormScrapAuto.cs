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
		private RetroSystems _systems = new();
		private string _basedir;
		public GameList Roms { get; set; }
		public bool ScrapWasStarting { get; set; } = false;
		private bool _isrunning = false;
		public FormScrapAuto(GameList roms, ScrapperManager scrapper, 
			string basedir, RetroSystems systems, RetroScrapOptions options)
		{
			InitializeComponent();
			Roms = roms;
			_scrapper = scrapper;
			_basedir = basedir;
			_systems = systems;
			_options = options;
		}

		private void FormScrapAuto_Load(object sender, EventArgs e)
		{
			this.Text = string.Format(Properties.Resources.Txt_Scrap_Auto_Title_FromSystem,
				Roms.Games.Count) + $" \"{Roms.RetroSys.Name}\".";

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
				_isrunning = false;
				return;
			}

			this.pictureBox1.Image = Properties.Resources.joystickani;
			buttonStart.Text = "Stop";
			_isrunning = true;

			_scrapCts?.Cancel();
			_scrapCts = new CancellationTokenSource();
			var ct = _scrapCts.Token;

			listViewMonitor.Items.Clear();
			this.pictureBox1.Image = Properties.Resources.joystickani;
			// Erstellen des Progress-Objekts. Die Action-Methode wird auf dem UI-Thread ausgeführt.
			var progressHandler = new Progress<ProgressObj>(report =>
			{
				if (report.ProgressPerc > 0 && report.ProgressPerc <= 100 )
					progressBarScrap.Value = report.ProgressPerc;
				if (!string.IsNullOrEmpty(report.MessageText))
					AddProtokollItem(report);
			});

			try
			{
				ScrapWasStarting = true;
				// Aufruf der asynchronen Methode und Übergabe des Progress-Objekts
				await _scrapper.ScrapGamesAsync(Roms, Roms.RetroSys.Id, _basedir, 
					progressHandler, _options.GetLanguageShortCode(), ct);
				// Speichern
				AddProtokollItem(ProgressObj.eTyp.Info, Properties.Resources.Txt_Log_Scrap_SaveGameList);
				bool ok = await Task.Run(() =>
						_systems.SaveAllRomsToGamelistXml(
								baseDir: _basedir,
								roms: Roms.Games
						)
				);
				if (ok)
				{
					AddProtokollItem(ProgressObj.eTyp.Info, Properties.Resources.Txt_Log_Scrap_SaveGamelist_success);
				}
				else
				{
					AddProtokollItem(ProgressObj.eTyp.Error, Properties.Resources.Txt_Log_Scrap_SaveGamelist_fail);
				}
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr($"{Utils.GetExcMsg(ex)}");
			}
			finally
			{
				progressBarScrap.Value = 0;
				this.pictureBox1.Image = null;
			}


		}

		private void AddProtokollItem(ProgressObj report)
		{
			ListViewItem it = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
			it.SubItems.Add(report.Typ.ToString());
			it.SubItems.Add(report.RomNumberAndTotal);
			it.SubItems.Add(report.RomName);
			it.SubItems.Add(report.MessageText);
			it.ImageKey = report.Typ.ToString();
			if ( report.Typ == ProgressObj.eTyp.Error )
				it.ForeColor = Color.Red;
			else if (report.Typ == ProgressObj.eTyp.Warning)
				it.ForeColor = Color.Orange;
			listViewMonitor.Items.Add(it);
			listViewMonitor.EnsureVisible(listViewMonitor.Items.Count - 1);
		}

		private void AddProtokollItem(ProgressObj.eTyp typ, string message)
		{
			AddProtokollItem(new ProgressObj(typ, -1, message));
		}

		private void buttonStopp_Click(object sender, EventArgs e)
		{
			_scrapCts?.Cancel();
		}
	}
}
