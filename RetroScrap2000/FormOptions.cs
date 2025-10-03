using RetroScrap2000.Tools;
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
using System.Xml.Serialization;

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
		public List<RetroSystem> RetroSystemList { get; set; }
		public FormOptions(RetroScrapOptions opt, ScrapperManager man, int tabpageindex, List<RetroSystem> retroSystemList)
		{
			InitializeComponent();
			tabControlOptions.SelectedIndex = tabpageindex;

			_manager = man;
			Options = opt;
			comboBoxLanguage.DrawMode = DrawMode.OwnerDrawFixed;
			comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxLanguage.ItemHeight = 24; // Adjust as needed

			foreach (var lang in _languages)
				comboBoxLanguage.Items.Add(lang);

			comboBoxLanguage.DrawItem += ComboBoxLanguage_DrawItem;
			if (string.IsNullOrEmpty(Options.Region) || Options.Region == "eu")
				comboBoxLangRegion.SelectedIndex = 0;
			else if (Options.Region == "us")
				comboBoxLangRegion.SelectedIndex = 1;
			else if (Options.Region == "jp")
				comboBoxLangRegion.SelectedIndex = 2;
			else
				Debug.Assert(false);

			radioButtonBatocera.Checked = true;

			ImageList imgTab = new ImageList() { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
			imgTab.Images.Add("general", Properties.Resources.general32); // Allgemein
			imgTab.Images.Add("scrapuser", Properties.Resources.user32); // Scrap
			imgTab.Images.Add("scrapdata", Properties.Resources.media32); // Scrap-Daten
			imgTab.Images.Add("custom", Properties.Resources.custom32); // Info
			imgTab.Images.Add("info", Properties.Resources.info32); // Info
			tabControlOptions.ImageList = imgTab;
			tabControlOptions.TabPages[0].ImageKey = "general";
			tabControlOptions.TabPages[1].ImageKey = "scrapuser";
			tabControlOptions.TabPages[2].ImageKey = "scrapdata";
			tabControlOptions.TabPages[3].ImageKey = "custom";
			tabControlOptions.TabPages[4].ImageKey = "info";
			RetroSystemList = retroSystemList;
		}

		private void FormOptions_Load(object sender, EventArgs e)
		{
			// Allgemein
			var l = _languages.FindIndex(x => x.CultureCode == Options.Language);
			if (l >= 0)
				comboBoxLanguage.SelectedIndex = l;
			else
				comboBoxLanguage.SelectedIndex = 0; // Default to first language if not found
			pictureBoxDonation.Image = Properties.Resources.donate48;
			linkLabelDonate.LinkClicked += LinkLabelInfo_LinkClicked;
			// Scrap User
			textBoxApiLogin.Text = Options.ApiUser ?? "";
			if (Options.Secret != null)
			{
				if (Options.Secret.TryLoad(out string? pwd))
					textBoxApiPwd.Text = pwd ?? "";
			}
			// Scrap Data
			checkBoxMediaFanart.Checked = Options.MediaFanart == true;
			checkBoxMediaImageBox.Checked = Options.MediaBoxImage == true;
			checkBoxMediaManual.Checked = Options.MediaManual == true;
			checkBoxMediaMap.Checked = Options.MediaMap == true;
			checkBoxMediaScreenshot.Checked = Options.MediaScreenshot == true;
			checkBoxMediaVideo.Checked = Options.MediaVideo == true;
			checkBoxMediaWheel.Checked = Options.MediaWheel == true;
			checkBoxMediaMarquee.Checked = Options.MediaMarquee == true;

			// Media Manual
			RetroSystemList.Sort();
			comboBoxOptMMSystems.Items.AddRange(RetroSystemList.ToArray());
			radioButtonOptMMAllSystems.Checked = true;
			FillMMList();

			// Info
			pictureBoxAppIcon.Image = Properties.Resources.RetroScrap2000_256;
			pictureBoxCompany.Image = Properties.Resources.grützesoftware_icon;
			pictureBoxScrap.Image = Properties.Resources.screenscraper_banner;

			var info = Utils.GetAppInfo();
			labelInfoProduct.Text = info.ProductName + " - Freeware";
			labelInfoVersion.Text = info.ProductVersion;
			labelInfoCompany.Text = info.Company;
			labelInfoCopyright.Text = info.Copyright;

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
			if (sender == null || e.Link == null)
				return;

			LinkLabel? clickedLabel = sender as LinkLabel;
			if (clickedLabel == null)
				return;

			string linkUrl = clickedLabel.Text.Substring(e.Link.Start, e.Link.Length);
			StartProc(linkUrl);
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
			if (comboBoxLangRegion.SelectedIndex == 0)
				Options.Region = "eu";
			else if (comboBoxLangRegion.SelectedIndex == 1)
				Options.Region = "us";
			else if (comboBoxLangRegion.SelectedIndex == 2)
				Options.Region = "jp";
			else
				Options.Region = "eu"; // default

			Options.MediaFanart = checkBoxMediaFanart.Checked;
			Options.MediaBoxImage = checkBoxMediaImageBox.Checked;
			Options.MediaManual = checkBoxMediaManual.Checked;
			Options.MediaMap = checkBoxMediaMap.Checked;
			Options.MediaScreenshot = checkBoxMediaScreenshot.Checked;
			Options.MediaVideo = checkBoxMediaVideo.Checked;
			Options.MediaWheel = checkBoxMediaWheel.Checked;
			Options.MediaMarquee = checkBoxMediaMarquee.Checked;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void pictureBoxCompany_Click(object sender, EventArgs e)
		{
			StartProc("https://github.com/gruetze-software");
		}

		private void pictureBoxAppIcon_Click(object sender, EventArgs e)
		{
			StartProc("https://github.com/gruetze-software/RetroScrap-2000-Releases");
		}

		private void pictureBoxScrap_Click(object sender, EventArgs e)
		{
			StartProc("https://www.screenscraper.fr/");
		}

		private void buttonInfoRetroPie_Click(object sender, EventArgs e)
		{
			MyMsgBox.Show(
				Properties.Resources.Txt_Msg_Opt_RetroPie_Info1 + "\r\n\r\n" +
				Properties.Resources.Txt_Msg_Opt_RetroPie_Info2 + "\r\n\r\n" +
				Properties.Resources.Txt_Msg_Opt_RetroPie_Info3);
		}

		private void StartProc(string file)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = file,
					UseShellExecute = true // Wichtig: Hiermit wird der Standard-Browser verwendet
				};
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
			}
		}

		private void buttonOptMMPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.MyComputer,
				ShowNewFolderButton = true
			};

			if (fbd.ShowDialog() == DialogResult.OK)
			{
				textBoxOptMMPath.Text = fbd.SelectedPath;
			}
		}

		public static string NormalizeFilePatterns(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "*.*"; // Standard: Alle Dateien

			// 1. Muster am Semikolon trennen und bereinigen
			var patterns = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
													.Select(p => p.Trim());

			// 2. Jedes Muster korrigieren (Sicherstellen, dass es mit * oder *.) beginnt)
			var normalizedPatterns = patterns.Select(p =>
			{
				// Wenn Muster mit Punkt (.) beginnt, * davor setzen (z.B. .pdf -> *.pdf)
				if (p.StartsWith("."))
				{
					return "*" + p;
				}
				// Wenn es nicht mit * beginnt, *.* davor setzen (z.B. pdf -> *pdf)
				if (!p.StartsWith("*"))
				{
					// Setzt *. davor, falls keine Wildcard enthalten ist
					if (!p.Contains('*'))
					{
						return "*." + p;
					}
				}
				return p;
			}).Distinct(StringComparer.OrdinalIgnoreCase);

			// 3. Neu zusammensetzen
			return string.Join(";", normalizedPatterns);
		}

		public static bool IsValidXmlKeyName(string keyName)
		{
			if (string.IsNullOrWhiteSpace(keyName))
				return false;

			// 1. Prüfen auf ungültige XML-Zeichen und Whitespace
			if (keyName.Any(c => Char.IsWhiteSpace(c) || System.Xml.XmlConvert.IsXmlChar(c) == false))
			{
				return false;
			}

			// 2. Prüfen auf Zeichen, die in XML-Tagnamen verboten sind (z.B. <>&"' /)
			if (keyName.Contains('<') || keyName.Contains('>') || keyName.Contains('&'))
			{
				return false;
			}

			// 3. Prüfen, ob der Name mit einem gültigen Zeichen beginnt (keine Zahl, kein Punkt/Bindestrich)
			if (!System.Xml.XmlConvert.IsStartNCNameChar(keyName[0]))
			{
				return false;
			}

			// Optional: Alle deutschen Umlaute (ä, ö, ü) sind in XML-Tagnamen technisch erlaubt,
			// aber oft vermieden. Wenn Sie sie verbieten wollen, fügen Sie die Prüfung hier ein.
			// Falls Sie nur ASCII oder lateinische Buchstaben zulassen wollen:
			if (keyName.Any(c => c > 127))
				return false;

			// Wenn alle Prüfungen bestanden sind:
			return true;
		}

		private void FillMMList()
		{
			listViewOptManualMedia.BeginUpdate();
			listViewOptManualMedia.Items.Clear();
			foreach (var entry in this.Options.MediaManualSystemList)
			{
				string systemname = entry.RomSystemID == null ? "*"
					: (RetroSystemList.FirstOrDefault(x => x.Id == entry.RomSystemID.Value)?.Name ?? $"ID {entry.RomSystemID.Value}");
				ListViewItem item = new ListViewItem(entry.Name);
				item.SubItems.Add(systemname);
				item.SubItems.Add(entry.XmlKeyName);
				item.SubItems.Add(entry.AbsolutPathMedia);
				item.SubItems.Add(entry.RelPathMediaToRomPath ?? "");
				item.Tag = entry;
				listViewOptManualMedia.Items.Add(item);
			}
			listViewOptManualMedia.EndUpdate();
		}
		private void buttonOptMMAdd_Click(object sender, EventArgs e)
		{
			// TODO:
			MyMsgBox.Show(Properties.Resources.Txt_Msg_UnderConstruction);
			return;

			bool newEntry = listViewOptManualMedia.SelectedItems.Count != 1;
			MediaManualSystem tempEntry = new MediaManualSystem()
			{
				AbsolutPathMedia = textBoxOptMMPath.Text,
				Name = textBoxOptMMName.Text,
				XmlKeyName = textBoxOptMMXMLTag.Text,
				MediaExtensionFilter = textBoxOptMMFilePatterns.Text,
				RomSystemID = radioButtonOptMMAllSystems.Checked
					? null
					: (comboBoxOptMMSystems.SelectedItem is RetroSystem rs ? rs.Id : (int?)null)
			};

			// Sind grundlegende Daten gefüllt?
			if (string.IsNullOrEmpty(tempEntry.AbsolutPathMedia)
				|| string.IsNullOrEmpty(tempEntry.Name)
				|| string.IsNullOrEmpty(tempEntry.XmlKeyName))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Opt_MM_MissingData);
				return;
			}

			// Ist der Xml-Key bereits im Standard vergeben (z. B. "marquee")?
			if (RetroScrapOptions.GetStandardMediaFolderAndXmlTagList().Contains(tempEntry.XmlKeyName))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Opt_MM_XmlAlwaysExist);
				return;
			}

			// Ist der Xml-Key valide?
			if (!IsValidXmlKeyName(tempEntry.XmlKeyName))
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Opt_MM_InvalidXmlKey);
				return;
			}

			// Wenn der relative Pfad gesetzt werden soll, muss zwingend der ROM-Path angegeben sein
			if (checkBoxOptMMRelPath.Checked)
			{
				if (string.IsNullOrEmpty(Options.RomPath))
				{
					MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Opt_MM_RelPathWithOutRomPath);
					return;
				}
			}

			// Existiert bereits ein Eintrag mit denselben Namen, Pfad und Xml-Key?
			if (newEntry)
			{
				bool alreadyExists = this.Options.MediaManualSystemList.Any(existingEntry =>
							existingEntry.AbsolutPathMedia.Equals(tempEntry.AbsolutPathMedia, StringComparison.OrdinalIgnoreCase)
					&& existingEntry.XmlKeyName.Equals(tempEntry.XmlKeyName, StringComparison.OrdinalIgnoreCase));
				if (alreadyExists)
				{
					MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Opt_MM_AlwaysExist);
					return;
				}
			}
			// Pattern glätten
			string normalizedPatterns = NormalizeFilePatterns(textBoxOptMMFilePatterns.Text);
			tempEntry.MediaExtensionFilter = normalizedPatterns;
			textBoxOptMMFilePatterns.Text = normalizedPatterns;

			// Relativen Pfad zum ROM-Pfad ermitteln
			if (checkBoxOptMMRelPath.Checked)
			{
				string relPath = FileTools.GetRelativePath(tempEntry.AbsolutPathMedia, Options.RomPath!);
				if (string.Compare(relPath, tempEntry.AbsolutPathMedia, true) == 0)
				{
					MyMsgBox.ShowWarn(string.Format(Properties.Resources.Txt_Msg_Opt_MM_WarningRelPathNotSet_RomPath, Options.RomPath));
					checkBoxOptMMRelPath.Checked = false;
					tempEntry.RelPathMediaToRomPath = null;
				}
				else
				{
					tempEntry.RelPathMediaToRomPath = relPath;
				}
			}
			else
			{
				tempEntry.RelPathMediaToRomPath = null;
			}

			// Füge den neuen Eintrag hinzu oder passe den vorhandenen an
			if (newEntry)
			{
				this.Options.MediaManualSystemList.Add(tempEntry);
			}
			else
			{
				MediaManualSystem oldEntry = (MediaManualSystem)listViewOptManualMedia.SelectedItems[0].Tag!;
				oldEntry.CopyFrom(tempEntry);
			}

			FillMMList();
		}

		private void buttonOptDelete_Click(object sender, EventArgs e)
		{
			if (listViewOptManualMedia.SelectedItems.Count == 1)
			{
				MediaManualSystem entryToRemove = (MediaManualSystem)listViewOptManualMedia.SelectedItems[0].Tag!;

				MediaManualSystem? existingEntry = Options.MediaManualSystemList.FirstOrDefault(e =>
							e.AbsolutPathMedia.Equals(entryToRemove.AbsolutPathMedia, StringComparison.OrdinalIgnoreCase)
					&& e.XmlKeyName.Equals(entryToRemove.XmlKeyName, StringComparison.OrdinalIgnoreCase)
					&& e.Name.Equals(entryToRemove.Name, StringComparison.OrdinalIgnoreCase)
				);

				if (existingEntry != null)
				{
					// 3. Entferne das gefundene Objekt aus der Liste
					Options.MediaManualSystemList.Remove(existingEntry);

					// 4. Entferne das Element aus der ListView-Anzeige
					listViewOptManualMedia.SelectedItems[0].Remove();
				}
			}
		}

		private void buttonOptMMScan_Click(object sender, EventArgs e)
		{
			MyMsgBox.Show(Properties.Resources.Txt_Msg_UnderConstruction);
			return;
		}

		private void listViewOptManualMedia_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewOptManualMedia.SelectedItems.Count == 1)
			{
				MediaManualSystem mediaManualSystem = (MediaManualSystem)listViewOptManualMedia.SelectedItems[0].Tag!;
				if (mediaManualSystem.RomSystemID == null)
				{
					radioButtonOptMMAllSystems.Checked = true;
				}
				else
				{
					radioButtonOptMMSystem.Checked = true;
					comboBoxOptMMSystems.SelectedItem = RetroSystemList.FirstOrDefault(x => x.Id == mediaManualSystem.RomSystemID);
				}

				textBoxOptMMFilePatterns.Text = mediaManualSystem.MediaExtensionFilter;
				textBoxOptMMName.Text = mediaManualSystem.Name;
				textBoxOptMMPath.Text = mediaManualSystem.AbsolutPathMedia;
				textBoxOptMMXMLTag.Text = mediaManualSystem.XmlKeyName;
				checkBoxOptMMRelPath.Checked = !string.IsNullOrEmpty(mediaManualSystem.RelPathMediaToRomPath);

			}
			else
			{
				radioButtonOptMMAllSystems.Checked = true;
				textBoxOptMMFilePatterns.Text = string.Empty;
				textBoxOptMMName.Text = string.Empty;
				textBoxOptMMPath.Text = string.Empty;
				textBoxOptMMXMLTag.Text = string.Empty;
				checkBoxOptMMRelPath.Checked = false;
			}
		}

		private void radioButtonOptMMAllSystems_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxOptMMSystems.SelectedIndex = -1;
			comboBoxOptMMSystems.Enabled = false;
		}

		private void radioButtonOptMMSystem_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxOptMMSystems.SelectedIndex = 0;
			comboBoxOptMMSystems.Enabled = Enabled;
		}
	}
}
