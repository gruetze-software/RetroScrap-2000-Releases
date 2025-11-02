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
	public partial class FormUpdate : Form
	{
		private string _newversion;
		private string _oldversion;
		private string _url;
		public FormUpdate(string newv, string oldv)
		{
			InitializeComponent();
			_newversion = newv;
			_oldversion = oldv;
			_url = "https://bit.ly/RetroScrap2000-Latest";
		}

		private void FormUpdate_Load(object sender, EventArgs e)
		{
			labelUpdateText.Text = string.Format(Properties.Resources.Txt_Msg_UpdateV1toV2,
				_oldversion, _newversion);

			linkLabelDownload.Text = _url;
			linkLabelDownload.LinkArea = new LinkArea(0, _url.Length);
			linkLabelDownload.LinkClicked += LinkLabelDownload_LinkClicked;
		}

		private void LinkLabelDownload_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
		{
			StartProc(_url);
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
	}
}
