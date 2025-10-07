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
	public partial class FormScrapRomSelection : Form
	{
		private List<GameDataRecherce> _dataRecherceList;
		private RetroScrapOptions _options;
		public GameDataRecherce? SelectedGame { get; set; }

		public FormScrapRomSelection(List<GameDataRecherce> dataRecherceList, RetroScrapOptions opt)
		{
			InitializeComponent();
			_dataRecherceList = dataRecherceList;
			_options = opt;
			buttonOK.Enabled = false;
		}

		private void FormScrapRomSelection_Load(object sender, EventArgs e)
		{
			listViewData.Items.Clear();
			foreach (var g in _dataRecherceList)
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

		private void listViewData_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewData.SelectedItems.Count == 1)
			{
				SelectedGame = (GameDataRecherce)listViewData.SelectedItems[0].Tag!;
				buttonOK.Enabled = true;
			}
			else
			{
				SelectedGame = null;
				buttonOK.Enabled = false;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (listViewData.SelectedItems.Count == 1)
			{
				SelectedGame = (GameDataRecherce)listViewData.SelectedItems[0].Tag!;
			}
			else
			{
				SelectedGame = null;
			}
		}

		private void FormScrapRomSelection_FormClosing(object sender, FormClosingEventArgs e)
		{

		}
	}
}
