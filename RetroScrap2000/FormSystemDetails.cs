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
	public partial class FormSystemDetails : Form
	{
		private RetroSystem _retrosystem;
		public FormSystemDetails(RetroSystem sys)
		{
			InitializeComponent();
			_retrosystem = sys;
		}

		private void FormSystemDetails_Load(object sender, EventArgs e)
		{
			if (_retrosystem.FileBanner != null)
				pictureBoxSystem.Image = Image.FromFile(_retrosystem.FileBanner);

			if (string.Compare(_retrosystem.Name_eu, _retrosystem.Name_us, StringComparison.OrdinalIgnoreCase) != 0)
			{
				ListViewItem itemname = new ListViewItem("Name (EU):");
				itemname.SubItems.Add(_retrosystem.Name_eu);
				listViewDetails.Items.Add(itemname);

				itemname = new ListViewItem("Name (US):");
				itemname.SubItems.Add(_retrosystem.Name_us);
				listViewDetails.Items.Add(itemname);
			}
			else
			{
				ListViewItem itemname = new ListViewItem("Name:");
				itemname.SubItems.Add(_retrosystem.Name_eu);
				listViewDetails.Items.Add(itemname);
			}

			ListViewItem item = new ListViewItem("Manufacturer:");
			item.SubItems.Add(_retrosystem.Hersteller);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Date");
			string dd = "";
			if ( _retrosystem.Debut > 0 && _retrosystem.Ende > 0 )
				dd = $"{_retrosystem.Debut.ToString()} - {_retrosystem.Ende.ToString()}";
			else if (_retrosystem.Debut > 0 || _retrosystem.Ende > 0 )
				dd = _retrosystem.Debut > 0 ? _retrosystem.Debut.ToString() : _retrosystem.Ende.ToString();
			item.SubItems.Add(dd);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Type:");
			item.SubItems.Add(_retrosystem.Typ);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Support type:");
			item.SubItems.Add(_retrosystem.SupportType);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Rom type:");
			item.SubItems.Add(_retrosystem.RomType);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("File extensions");
			item.SubItems.Add($"{_retrosystem.Extensions}");
			listViewDetails.Items.Add(item);

			textBoxHistory.Text = _retrosystem.Description;

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

		private void FormSystemDetails_Resize(object sender, EventArgs e)
		{
			ResizeListView();
		}

		private void textBoxHistory_TextChanged(object sender, EventArgs e)
		{
			_retrosystem.Description = textBoxHistory.Text;
		}
	}
}
