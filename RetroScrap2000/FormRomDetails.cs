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
	public partial class FormRomDetails : Form
	{
		private GameEntry _rom;
		private string _romFile;
		public FormRomDetails(GameEntry rom, string romFile)
		{
			InitializeComponent();
			_rom = rom;
			_romFile = romFile;
		}

		private void FormRomDetails_Load(object sender, EventArgs e)
		{
			FileInfo f = new FileInfo(_romFile);
			
			ListViewItem item = new ListViewItem("Name:");
			item.SubItems.Add(f.FullName);
			listViewDetails.Items.Add(item);
		}
	}
}
