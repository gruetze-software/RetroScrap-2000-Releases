using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroScrap2000
{
	public partial class FormScrapRomResearch : Form
	{
		private CancellationTokenSource? _scrapCts;
		private ScraperManager _scrapper;
		private RetroScrapOptions _options;
		private RetroSystem _system;
		private string _romfile;
		private bool _isrunning = false;
		public GameDataRecherce? SelectedGame { get; set; }

		public FormScrapRomResearch(string romfile, ScraperManager man, RetroSystem sys,
			RetroScrapOptions opt)
		{
			InitializeComponent();
			_options = opt;
			_romfile = romfile;
			_system = sys;
			_scrapper = man;
			buttonOK.Enabled = false;
		}

		private void FormScrapRomResearch_Load(object sender, EventArgs e)
		{
			listViewData.Items.Clear();
			textBoxSearchName.Text = Tools.FileTools.CleanSearchName(
				Path.GetFileNameWithoutExtension(_romfile));
		}

		private void listViewData_SelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedGame = null;
			if (listViewData.SelectedItems.Count == 1)
			{
				if (listViewData.SelectedItems[0].Tag is GameDataRecherce)
					SelectedGame = (GameDataRecherce)listViewData.SelectedItems[0].Tag!;
				buttonOK.Enabled = true;
			}
			else
			{
				buttonOK.Enabled = false;
			}
		}
		private async void buttonStartStop_Click(object sender, EventArgs e)
		{
			if (_isrunning)
			{
				_scrapCts?.Cancel();
				buttonStartStop.Text = "Start";
				_isrunning = false;
				this.pictureBoxAniJoystick.Image = null;
				return;
			}

			if (string.IsNullOrEmpty(textBoxSearchName.Text))
			{
				MyMsgBox.ShowWarn(Properties.Resources.Txt_Msg_Scrap_Research_TermMissing);
				return;
			}

			//this.pictureBoxAniWait.Image = Properties.Resources.joystickani;
			buttonStartStop.Text = "Stop";
			_isrunning = true;

			_scrapCts?.Cancel();
			_scrapCts = new CancellationTokenSource();
			var ct = _scrapCts.Token;
			this.pictureBoxAniJoystick.Image = Properties.Resources.joystickani;
			try
			{
				listViewData.Items.Clear();
				var result = await _scrapper.GetGameRechercheListAsync(textBoxSearchName.Text,
					_system.Id, _options, ct);

				if (result.HttpCode == 404)
				{
					MyMsgBox.Show(Properties.Resources.Txt_Msg_Scrap_Research_NoSuccess);
					return;
				}
				if (!result.Ok)
				{
					MyMsgBox.ShowErr(result.Error!);
					return;
				}

				if (result.RechercheResult != null && result.RechercheResult.Count > 0)
				{
					foreach (var g in result.RechercheResult)
					{
						if (string.IsNullOrEmpty(g.GetName(_options)))
							continue;

						ListViewItem it = new ListViewItem(g.GetName(_options));
						it.SubItems.Add(g.GetGenre(_options));
						it.SubItems.Add(g.developpeur?.text);
						it.SubItems.Add(g.editeur?.text);
						it.SubItems.Add(g.GetDesc(_options));
						it.Tag = g;
						listViewData.Items.Add(it);
					}
				}
				else
				{
					MyMsgBox.Show(Properties.Resources.Txt_Msg_Scrap_Research_NoSuccess);
					return;
				}
			}
			catch (OperationCanceledException) // <-- FÄNGT DEN ABBRUCH AB
			{

			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr($"{Utils.GetExcMsg(ex)}");
			}
			finally
			{
				this.pictureBoxAniJoystick.Image = null;
				this.buttonStartStop.Text = "Start";
				_isrunning = false;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (listViewData.SelectedItems.Count == 1)
			{
				if (listViewData.SelectedItems[0].Tag is GameDataRecherce)
					SelectedGame = (GameDataRecherce)listViewData.SelectedItems[0].Tag!;
			}
			else
			{
				SelectedGame = null;
			}
		}

		private void FormScrapRomResearch_FormClosing(object sender, FormClosingEventArgs e)
		{
			_scrapCts?.Cancel();
		}

	}
}
