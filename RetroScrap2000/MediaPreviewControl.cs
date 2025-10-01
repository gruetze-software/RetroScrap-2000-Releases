using RetroScrap2000.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
		// Öffentliche Eigenschaften für den externen Zugriff (für FormScrapRom)
		public PictureBox PicBox { get { return this.pictureBoxMedium; } }
		public CheckBox CheckBox { get { return this.checkBoxTakeOver; } }
		public Label Label { get { return this.labelTitle; } }
		public string? AbsolutPath{ get; private set; }

		[DefaultValue(eMediaType.Unknown)]
		private eMediaType _type;
		public eMediaType MediaType { get { return _type; } set { _type = value; labelTitle.Text = _type.ToString(); } }

		[DefaultValue(false)]
		private bool _checkboxVisible = false;
		public bool ShowCheckbox { get { return _checkboxVisible; } set { _checkboxVisible = value; checkBoxTakeOver.Visible = _checkboxVisible; } }

		public MediaPreviewControl()
		{
			InitializeComponent();
		}
		
		public async Task LoadMediaAsync(string? mediaPath, string baseDir, CancellationToken ct)
		{
			var result = await Utils.LoadMediaAsync(MediaType, mediaPath, baseDir, ct);
			AbsolutPath = result.absPath;
			pictureBoxMedium.Image = result.img;
			pictureBoxMedium.Tag = AbsolutPath;
			if (result.img != null && !string.IsNullOrEmpty(AbsolutPath))
			{
				this.PicBox.Click += (s, e) => UiTools.OpenPicBoxTagFile(this.PicBox);
				pictureBoxMedium.Cursor = Cursors.Hand;
			}
		}
	}
}

