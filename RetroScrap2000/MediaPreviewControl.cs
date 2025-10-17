using RetroScrap2000.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroScrap2000
{
	public partial class MediaPreviewControl : UserControl
	{
		public event EventHandler<MediaActionEventArgs>? ViewMediaClicked;
		public event EventHandler<MediaDeleteActionEventArgs>? DeleteMediaClicked;
		public event EventHandler<MediaActionEventArgs>? NewMediaClicked;

		public enum ControlDisplayMode { Checkbox = 0, Buttons, None }
		public PictureBox PicBox { get { return this.pictureBoxMedium; } }
		public CheckBox CheckBox { get { return this.checkBoxTakeOver; } }
		public string? AbsolutPath { get; private set; }

		[DefaultValue(ControlDisplayMode.Checkbox)]
		private ControlDisplayMode _displayMode = ControlDisplayMode.None;
		public ControlDisplayMode DisplayMode
		{
			get { return _displayMode; }
			set
			{
				_displayMode = value;
				UpdateControlVisibility(value);
			}
		}

		[DefaultValue(eMediaType.Unknown)]
		private eMediaType _type;
		public eMediaType MediaType { get { return _type; } set { _type = value; labelTitle.Text = _type.ToString(); } }

		public MediaPreviewControl()
		{
			InitializeComponent();
			buttonOpen.Image = Properties.Resources.open16;
			buttonNew.Image = Properties.Resources.add16;
			buttonDelete.Image = Properties.Resources.delete16;
			toolTipButtons.SetToolTip(buttonOpen, Properties.Resources.Txt_Media_ToolTip_Open);
			toolTipButtons.SetToolTip(buttonNew, Properties.Resources.Txt_Media_ToolTip_New);
			toolTipButtons.SetToolTip(buttonDelete, Properties.Resources.Txt_Media_ToolTip_Delete);
		}

		private void CenterButtons()
		{
			// Berechne die Position, damit das FlowLayoutPanel zentriert wird
			int parentWidth = this.panelForControls.Width;
			int panelWidth = this.flowLayoutPanelButtons.Width;

			// Zentrieren: (Breite des Parents - Breite des Kindes) / 2
			int x = (parentWidth - panelWidth) / 2;

			// Setze die neue Position
			this.flowLayoutPanelButtons.Location = new Point(x, this.flowLayoutPanelButtons.Location.Y);
		}

		private void UpdateControlVisibility(ControlDisplayMode mode)
		{
			bool showControls = (mode != ControlDisplayMode.None);

			// 1. Alle Steuerelemente im Wrapper (oder Checkbox und Button-Panel einzeln) unsichtbar machen
			this.checkBoxTakeOver.Visible = false;
			this.flowLayoutPanelButtons.Visible = false;

			// 2. Das benötigte Steuerelement sichtbar machen, falls nicht None
			if (showControls)
			{
				if (mode == ControlDisplayMode.Checkbox)
				{
					this.checkBoxTakeOver.Visible = true;
				}
				else if (mode == ControlDisplayMode.Buttons)
				{
					this.flowLayoutPanelButtons.Visible = true;
					CenterButtons();
				}
			}

			// Das Layout neu berechnen, um die Änderungen zu übernehmen
			this.PerformLayout();
		}

		public async Task LoadMediaAsync(string? mediaPath, string baseDir, CancellationToken ct, ScrapperManager scraper, bool byPassCache)
		{
			var result = await scraper.LoadMediaAsync(MediaType, mediaPath, baseDir, ct, true, byPassCache);
			AbsolutPath = result.absPath;
			// Aufräumen vorm Setzen
			if (pictureBoxMedium.Image != null)
			{
				pictureBoxMedium.Image.Dispose();
				pictureBoxMedium.Image = null;
			}
			pictureBoxMedium.Image = result.img;
			pictureBoxMedium.Tag = AbsolutPath;
			if (!string.IsNullOrEmpty(AbsolutPath))
			{
				pictureBoxMedium.Cursor = Cursors.Hand;
				buttonDelete.Enabled = true;
				buttonOpen.Enabled = true;
			}
			else
			{
				pictureBoxMedium.Cursor = Cursors.Default;
				buttonDelete.Enabled = false;
				buttonOpen.Enabled = false;
			}
		}

		private void buttonOpen_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.AbsolutPath))
			{
				// Übergibt den aktuell im Control geladenen Pfad
				ViewMediaClicked?.Invoke(this, new MediaActionEventArgs(this.MediaType, this.AbsolutPath));
			}
		}

		private void buttonNew_Click(object sender, EventArgs e)
		{
			if (MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Question_NewMedia) != DialogResult.Yes)
				return;

			OpenFileDialog ofd = new OpenFileDialog();
			if (MediaType == eMediaType.Video)
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
				// WICHTIG: Übergibt den PFAD ZUR NEU GEWÄHLTEN DATEI
				NewMediaClicked?.Invoke(this, new MediaActionEventArgs(this.MediaType, ofd.FileName));
			}
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.AbsolutPath))
			{
				var userChoice = MyMsgBox.ShowQuestion(Properties.Resources.Txt_Msg_Question_DeleteMedia);
				if (userChoice == DialogResult.Cancel)
					return;

				// YES: Alles löschen, deleteOnlyXmlInfo = false; NO: Nur XML-Info löschen, deleteOnlyXmlInfo = true
				bool deleteOnlyXmlInfo = userChoice == DialogResult.No;
				DeleteMediaClicked?.Invoke(this, new MediaDeleteActionEventArgs(this.MediaType, this.AbsolutPath, deleteOnlyXmlInfo));
			}
		}

		private void pictureBoxMedium_MouseClick(object sender, MouseEventArgs e)
		{
			if ( e.Button == MouseButtons.Left && sender is PictureBox pb && !string.IsNullOrEmpty(this.AbsolutPath))
			{
				// Übergibt den aktuell im Control geladenen Pfad
				ViewMediaClicked?.Invoke(this, new MediaActionEventArgs(this.MediaType, this.AbsolutPath));
			}
		}
	}

	// Klasse, um den Medientyp und den FilePath für alle Aktionen zu übergeben
	public class MediaActionEventArgs : EventArgs
	{
		public eMediaType MediaType { get; }
		public string AbsolutPath { get; set; }

		public MediaActionEventArgs(eMediaType mediaType, string absolutPath	)
		{
			MediaType = mediaType;
			AbsolutPath = absolutPath;
		}
	}

	public class MediaDeleteActionEventArgs : MediaActionEventArgs
	{
		public bool DeleteOnlyXmlInfo { get; }

		public MediaDeleteActionEventArgs(eMediaType mediaType, string absolutPath, bool deleteOnlyXmlInfo) 
			: base (mediaType, absolutPath)
		{
			DeleteOnlyXmlInfo = deleteOnlyXmlInfo;
		}
	}
}

