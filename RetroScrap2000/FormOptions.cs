using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroScrap2000
{

	public partial class FormOptions : Form
	{
		private readonly List<(string DisplayName, string CultureCode, Image Flag)> _languages =
		[
				("English", "en-US", Properties.Resources.flag_en), // Add your flag images to resources
        ("German", "de-DE", Properties.Resources.flag_de),
				("French", "fr-FR", Properties.Resources.flag_fr),
				("Spanish", "es-ES", Properties.Resources.flag_es),
        // Add more as needed
    ];

		private readonly ScrapperManager? _manager;
		public RetroScrapOptions Options { get; set; }
		public FormOptions(RetroScrapOptions opt, ScrapperManager man)
		{
			InitializeComponent();

			_manager = man;
			Options = opt;
			comboBoxLanguage.DrawMode = DrawMode.OwnerDrawFixed;
			comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxLanguage.ItemHeight = 24; // Adjust as needed

			foreach (var lang in _languages)
				comboBoxLanguage.Items.Add(lang);

			comboBoxLanguage.DrawItem += ComboBoxLanguage_DrawItem;

			radioButtonBatocera.Checked = true;
		}

		private void FormOptions_Load(object sender, EventArgs e)
		{
			// Language
			var l = _languages.FindIndex(x => x.CultureCode == Options.Language);
			if (l >= 0)
				comboBoxLanguage.SelectedIndex = l;
			else
				comboBoxLanguage.SelectedIndex = 0; // Default to first language if not found

			// Scrap
			textBoxApiLogin.Text = Options.ApiUser ?? "";
			if (Options.Secret != null)
			{
				if (Options.Secret.TryLoad(out string? pwd))
					textBoxApiPwd.Text = pwd ?? "";
			}

			// Info
			pictureBoxAppIcon.Image = Properties.Resources.RetroScrap2000_256;
			pictureBoxCompany.Image = Properties.Resources.grützesoftware_icon;
			pictureBoxScrap.Image = Properties.Resources.screenscraper_banner;
			pictureBoxDonation.Image = Properties.Resources.tip32;
			var info = Utils.GetAppInfo();
			labelInfoProduct.Text = info.product + " - Freeware";
			labelInfoVersion.Text = info.version;
			labelInfoCompany.Text = info.company;
			labelInfoCopyright.Text = info.copyright;

			linkLabel1.Text = "API from https://www.screenscraper.fr/";
			linkLabel1.LinkArea = new LinkArea(9, linkLabel1.Text.Length - 9);
			linkLabel1.LinkClicked += LinkLabelInfo_LinkClicked;

			linkLabel2.Text = "Icons from https://www.flaticon.com/";
			linkLabel2.LinkArea = new LinkArea(11, linkLabel2.Text.Length - 11);
			linkLabel2.LinkClicked += LinkLabelInfo_LinkClicked;

			linkLabel3.Text = "Flags from https://www.countryflags.com/";
			linkLabel3.LinkArea = new LinkArea(11, linkLabel3.Text.Length - 11);
			linkLabel3.LinkClicked += LinkLabelInfo_LinkClicked;

			linkLabel4.Text = "ffmpeg from https://ffmpeg.org/";
			linkLabel4.LinkArea = new LinkArea(12, linkLabel4.Text.Length - 12);
			linkLabel4.LinkClicked += LinkLabelInfo_LinkClicked;
		}

		private void LinkLabelInfo_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
		{
			// Link im Standard-Browser öffnen
			if ( sender == null || e.Link == null) 
				return;

			try
			{
				LinkLabel? clickedLabel = sender as LinkLabel;
				if ( clickedLabel == null )
					return;

				string linkUrl = clickedLabel.Text.Substring(e.Link.Start, e.Link.Length);
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = linkUrl,
					UseShellExecute = true // Wichtig: Hiermit wird der Standard-Browser verwendet
				};
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
			}
		}

		private void ComboBoxLanguage_DrawItem(object? sender, DrawItemEventArgs e)
		{
			if (e.Index < 0) return;
			e.DrawBackground();

			// Safely cast and check for null
			if (comboBoxLanguage.Items[e.Index] is ValueTuple<string, string, Image> item)
			{
				var (name, _, flag) = item;
				int imgSize = 20;
				int offset = 2;

				if (flag != null)
					e.Graphics.DrawImage(flag, e.Bounds.Left + offset, e.Bounds.Top + offset, imgSize, imgSize);

				using var brush = new SolidBrush(e.ForeColor);
				// Ensure e.Font is not null before using it
				var font = e.Font ?? SystemFonts.DefaultFont;
				e.Graphics.DrawString(name, font, brush, e.Bounds.Left + imgSize + 2 * offset, e.Bounds.Top + offset);
			}

			e.DrawFocusRectangle();
		}

		private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxLanguage.SelectedItem is ValueTuple<string, string, Image> selectedLang)
			{
				var newlang = selectedLang.Item2;
				if (Options.Language != newlang)
				{
					if (MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Opt_Language_Change) == DialogResult.Yes)
					{
						Options.Language = newlang;
						Options.Save();
						Application.Restart();
						Environment.Exit(0);
					}
					else
					{
						comboBoxLanguage.SelectedIndex = _languages.FindIndex(x => x.CultureCode == Options.Language);
					}
				}
			}
		}

		private async void buttonApiTest_Click(object sender, EventArgs e)
		{
			SaveSecrets();
			_manager!.RefreshSecrets(Options.ApiUser!, textBoxApiPwd.Text);

			listBoxApiTest.Items.Clear();
			listBoxApiTest.Items.Add(Properties.Resources.Txt_PleaseWait);
			try
			{
				var testdata = await _manager.FetchSsUserInfosAsync();
				if (testdata == null || testdata.response == null || testdata.response.ssuser == null)
				{
					listBoxApiTest.Items.Clear();
					listBoxApiTest.Items.AddRange(
						[ Properties.Resources.Txt_Api_Err_NoResponse,
							Properties.Resources.Txt_Api_Err_CheckInternet ]);
				}
				else
				{
					listBoxApiTest.Items.Clear();
					var user = testdata.response.ssuser;
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Name, user.id ?? ""));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Level, user.niveau != null ? user.niveau.Value : "-"));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_LastLogin, user.LastVisit() != null ? user.LastVisit()!.Value.ToString() : "-"));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Visits, user.visites != null ? user.visites.Value : "-"));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_Region, user.favregion ?? ""));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_MaxThreads, user.maxthreads != null ? user.maxthreads : "-"));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_DownloadKBS, user.maxdownloadspeed != null ? user.maxdownloadspeed : "-"));
					listBoxApiTest.Items.Add(string.Format(Properties.Resources.Txt_Api_SsUser_RequestPerDay,
						user.requeststoday != null ? user.requeststoday : "-",
						user.maxrequestsperday != null ? user.maxrequestsperday : "-",
						user.UsedTodayPercent()));
				}
			}
			catch (Exception ex)
			{
				listBoxApiTest.Items.Clear();
				listBoxApiTest.Items.Add("Fail!");
				listBoxApiTest.Items.Add(ex.Message);
			}

		}

		private void SaveSecrets()
		{
			Options.ApiUser = textBoxApiLogin.Text;
			if (string.IsNullOrEmpty(textBoxApiPwd.Text))
			{
				Trace.WriteLine($"Delete secret-file...");
				Options.Secret?.Delete();
			}
			else
			{
				Trace.WriteLine($"Save secret-file...");
				Options.Secret?.Save(textBoxApiPwd.Text);
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			SaveSecrets();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void pictureBoxCompany_Click(object sender, EventArgs e)
		{
			Process.Start("https://github.com/gruetze-software");
		}

		private void pictureBoxAppIcon_Click(object sender, EventArgs e)
		{
			Process.Start("https://github.com/gruetze-software/RetroScrap-2000-Releases");
		}

		private void pictureBoxScrap_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.screenscraper.fr/");
		}

		private void buttonDonations_Click(object sender, EventArgs e)
		{
			// Ihr Ko-fi-Link
			string url = "https://ko-fi.com/gruetzesoftware";

			// Öffnet den Standard-Webbrowser
			try
			{
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Der Spenden-Link konnte nicht geöffnet werden: {Utils.GetExcMsg(ex)}",
												"Fehler",
												MessageBoxButtons.OK,
												MessageBoxIcon.Error);
			}
		}

		private void SetRetroDist()
		{
			// Dateistruktur und Pfade

			//								Batocera																RetroArch
			// Roms						/userdata/roms/<systemname>/						
			// Gamelist.xml		/userdata/system/<systemname>/					Nein, lpl-Files
			// Relative Path?	Yes
			// Box-Image			/userdata/roms/<systemname>/images/			/opt/retropie/configs/all/emulationstation/downloaded_media/<systemname>/boxart
			// Marquees-Image	/userdata/roms/<systemname>/Marquees/		/opt/retropie/configs/all/emulationstation/downloaded_media/<systemname>/marquee
			// Videos					/userdata/roms/<systemname>/videos/			/opt/retropie/configs/all/emulationstation/downloaded_media/<systemname>/video
		}

		private void buttonInfoRetroPie_Click(object sender, EventArgs e)
		{
			MyMsgBox.Show(Properties.Resources.Txt_Msg_Opt_RetroPie_Info);
		}
	}
}
