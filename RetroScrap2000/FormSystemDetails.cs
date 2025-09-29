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

			ListViewItem item = new ListViewItem("Name:");
			item.SubItems.Add(_retrosystem.Name);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Manufacture:");
			item.SubItems.Add(_retrosystem.Hersteller);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Type:");
			item.SubItems.Add(_retrosystem.Typ);
			listViewDetails.Items.Add(item);

			item = new ListViewItem("Date");
			item.SubItems.Add($"{_retrosystem.Debut.ToString()} - {_retrosystem.Ende.ToString()}");
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
