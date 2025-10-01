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

			item = new ListViewItem("Id:");
			item.SubItems.Add(_rom.Id.ToString());
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Source:");
			item.SubItems.Add(_rom.Source);
			listViewDetails.Items.Add(item);

			// TODO

			ResizeListView();
		}

		private void ResizeListView()
		{
			// Sicherstellen, dass die Spalten im Detailmodus existieren.
			if (listViewDetails.View == View.Details && listViewDetails.Columns.Count > 0)
			{
				// Setze eine Basisbreite für alle Spalten außer der letzten.
				int totalWidthExcludingLastColumn = 0;
				for (int i = 0; i < listViewDetails.Columns.Count - 1; i++)
				{
					totalWidthExcludingLastColumn += listViewDetails.Columns[i].Width;
				}

				// Berechne die neue Breite der letzten Spalte, indem du den verbleibenden Platz nimmst.
				// Der '- 4' am Ende ist für den Scrollbar-Bereich.
				int newWidth = listViewDetails.ClientSize.Width - totalWidthExcludingLastColumn - 4;

				// Wenn die neue Breite positiv ist, setze sie.
				if (newWidth > 0)
				{
					listViewDetails.Columns[listViewDetails.Columns.Count - 1].Width = newWidth;
				}
			}
		}

		private void FormRomDetails_Resize(object sender, EventArgs e)
		{
			ResizeListView();
		}
	}
}
