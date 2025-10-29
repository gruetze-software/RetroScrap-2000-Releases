using RetroScrap2000.Properties;
using RetroScrap2000.Tools;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RetroScrap2000
{
	public partial class FormScrapRom : Form
	{
		private readonly GameEntry _current;
		private readonly GameScrap _scraped;
		private readonly string _systemRomPath;
		private readonly RetroScrapOptions _options;
		private ScraperManager _scraper;
		private CancellationTokenSource? _romCts;


		public ScrapeSelection Selection { get; } = new();
		public StarRatingControl starRatingControlRomOld { get; set; } = new StarRatingControl();
		public StarRatingControl starRatingControlRomNew { get; set; } = new StarRatingControl();
		public FormScrapRom(string systemRomPath, GameEntry current, GameScrap scraped, RetroScrapOptions options, ScraperManager scraper	)
		{
			InitializeComponent();

			starRatingControlRomOld = new StarRatingControl()
			{
				AllowHalfStars = true,
				EmptyColor = Color.LightGray,
				FilledColor = Color.Red,
				Name = "starRatingControlRomOld",
				OutlineColor = Color.Black,
				Rating = 0D,
				StarCount = 5,
				StarSpacing = 4,
				Dock = DockStyle.Fill
			};
			starRatingControlRomNew = new StarRatingControl()
			{
				AllowHalfStars = true,
				EmptyColor = Color.LightGray,
				FilledColor = Color.Red,
				Name = "starRatingControlRomNew",
				OutlineColor = Color.Black,
				Rating = 0D,
				StarCount = 5,
				StarSpacing = 4,
				Dock = DockStyle.Fill,
			};

			tableLayoutPanelLeft.Controls.Add(starRatingControlRomOld, 1, 7);
			tableLayoutPanelRight.Controls.Add(starRatingControlRomNew, 0, 7);

			_current = current;
			_scraped = scraped;
			_scraper = scraper;
			_systemRomPath = systemRomPath;
			_options = options;

			// Für das linke FlowLayoutPanel
			panelLeft.AutoScroll = true; // Ermöglicht das Scrollen, wenn der Inhalt größer als der sichtbare Bereich ist
			this.flowLayoutPanelMediaLeft.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			this.flowLayoutPanelMediaLeft.AutoScroll = false;
			this.flowLayoutPanelMediaLeft.WrapContents = false;
			this.flowLayoutPanelMediaLeft.AutoSize = false;      // **Muss True sein** -> Erlaubt es, horizontal über die Grenzen des Formulars hinauszuwachsen.
			this.flowLayoutPanelMediaLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

			// Für das rechte FlowLayoutPanel
			panelRight.AutoScroll = true; // Ermöglicht das Scrollen, wenn der Inhalt größer als der sichtbare Bereich ist
			this.flowLayoutPanelMediaRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
			this.flowLayoutPanelMediaRight.AutoScroll = false;
			this.flowLayoutPanelMediaRight.WrapContents = false; // **Muss False sein**
			this.flowLayoutPanelMediaRight.AutoSize = false;
			this.flowLayoutPanelMediaRight.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
			
		}

		private async void FormScrapRom_Load(object sender, EventArgs e)
		{
			this.buttonOK.Enabled = false;
			this.UseWaitCursor = true;
			this.Cursor = Cursors.WaitCursor;
			Cursor.Current = Cursors.WaitCursor;

			// Links füllen (current)
			//////////////////////////////////////////////////////////////////////////
			textBoxRomOldName.Text = _current.Name ?? "";
			textBoxRomOldDesc.Text = _current.Description ?? "";
			textBoxRomOldGenre.Text = _current.Genre ?? "";
			textBoxRomOldPlayers.Text = _current.Players ?? "";
			textBoxRomOldDev.Text = _current.Developer ?? "";
			textBoxRomOldPub.Text = _current.Publisher ?? "";
			if (_current.ReleaseDate.HasValue)
				textBoxRomOldRelease.Text = _current.ReleaseDate.Value.ToShortDateString();
			else
				textBoxRomOldRelease.Text = "";
			starRatingControlRomOld.Rating = _current.RatingStars;

			// laufende System-Ladevorgänge abbrechen
			_romCts?.Cancel();
			_romCts = new CancellationTokenSource();
			var ct = _romCts.Token;

			try
			{
				foreach (var kvp in _current.MediaTypeDictionary)
				{
					if (string.IsNullOrEmpty(kvp.Value))
						continue;

					string? mediafile = FileTools.ResolveMediaPath(_systemRomPath, kvp.Value);
					GameMediaSettings m = RetroScrapOptions.GetMediaSettings(kvp.Key)!;
					m.FilePath = mediafile;
					
					if ( kvp.Key == eMediaType.Video && !FileTools.IsVideoFile(mediafile) )
					{
						Log.Warning($"Skip Video Media \"{mediafile}\", it's not a valid video file...");
						continue; // kein Video
					}

					var control = await CreateMediaPreviewPanel(m, _systemRomPath, CancellationToken.None, false);
					flowLayoutPanelMediaLeft.Controls.Add(control);
				}
				Utils.ForceHorizontalScrollForMediaPreviewControls(flowLayoutPanelMediaLeft);

				// Rechts füllen (scraped)
				/////////////////////////////////////////////////////////////////////
				textBoxRomNewName.Text = _scraped.Name ?? "";
				_scraped.Description = Utils.DecodeTextFromApi(_scraped.Description);
				textBoxRomNewDesc.Text = _scraped.Description ?? "";
				textBoxRomNewGenre.Text = _scraped.Genre ?? "";
				textBoxRomNewPlayers.Text = _scraped.Players ?? "";
				textBoxRomNewDev.Text = _scraped.Developer ?? "";
				textBoxRomNewPub.Text = _scraped.Publisher ?? "";
				textBoxRomNewRelease.Text = _scraped.ReleaseDate.HasValue ? _scraped.ReleaseDate.Value.ToShortDateString() : "";
				if (_scraped.RatingNormalized != null && _scraped.RatingNormalized.HasValue && _scraped.RatingNormalized.Value > 0)
					starRatingControlRomNew.Rating = _scraped.RatingNormalized.Value * 5.0;

				var medien = _scraped.PossibleMedien
					.OrderBy(m => m.Type)
					.ToList();
				foreach (var medium in medien)
				{
					if (string.IsNullOrEmpty(medium.Url))
						continue;

					if (_options.IsMediaTypeEnabled(medium) != true)
					{
						Log.Information($"Skip Download \"{medium}\", it's not checked in Options...");
						continue; // diese Media-Art ist nicht gewünscht
					}

					var control = await CreateMediaPreviewPanel(medium, _systemRomPath, ct, true);
					if (ct.IsCancellationRequested)
					{
						return;
					}

					flowLayoutPanelMediaRight.Controls.Add(control);
				}
				Utils.ForceHorizontalScrollForMediaPreviewControls(flowLayoutPanelMediaRight);

				// Checkboxen setzen (nur wenn rechts was da ist)
				checkBoxName.Checked = !string.IsNullOrEmpty(_scraped.Name) && _scraped.Name != _current.Name;
				checkBoxDesc.Checked = !string.IsNullOrEmpty(_scraped.Description) && _scraped.Description != _current.Description;
				checkBoxGenre.Checked = !string.IsNullOrEmpty(_scraped.Genre) && _scraped.Genre != _current.Genre;
				checkBoxPlayer.Checked = !string.IsNullOrEmpty(_scraped.Players) && _scraped.Players != _current.Players;
				checkBoxDev.Checked = !string.IsNullOrEmpty(_scraped.Developer) && _scraped.Developer != _current.Developer;
				checkBoxPub.Checked = !string.IsNullOrEmpty(_scraped.Publisher) && _scraped.Publisher != _current.Publisher;
				checkBoxRating.Checked = _scraped.RatingNormalized.HasValue && _current.Rating != _scraped.RatingNormalized;
				checkBoxRelease.Checked = _scraped.ReleaseDate != _current.ReleaseDate;

				// Media-Checkboxen: nur wenn rechts was da ist UND es ist anders als links
				foreach (var control in flowLayoutPanelMediaRight.Controls.OfType<MediaPreviewControl>())
				{
					if (control != null
						&& control.MediaType != eMediaType.Unknown
						&& !string.IsNullOrEmpty(control.AbsolutPath)
						&& File.Exists(control.AbsolutPath))
					{
						Selection.MediaTempPaths.Add(control.MediaType, (tempPath: control.AbsolutPath, take: control.CheckBox.Checked));
					}
				} // Next control
			}
			catch (Exception ex)
			{
				MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
			}
			finally
			{
				this.UseWaitCursor = false;
				this.Cursor = Cursors.Default;
				Cursor.Current = Cursors.Default;
				this.Refresh();
				this.buttonOK.Enabled = true;
			}
		}

		private void pictureBox_Click(object sender, EventArgs e)
		{
			UiTools.OpenPicBoxTagFile((PictureBox)sender);
		}

		private string? ConvertDateFormat(string dt)
		{
			// Parsen des Eingabe-Strings mit dem Format "dd.mm.yyyy"
			if (string.IsNullOrEmpty(dt))
				return null;

			if (DateTime.TryParseExact(dt, "dd.MM.yyyy", CultureInfo.InvariantCulture,
				DateTimeStyles.None, out var parsedDate))
			{
				// Formatieren des DateTime-Objekts in das Zielformat "yyyyMMddT000000"
				return parsedDate.ToString("yyyy-MM-dd");
			}
			else
			{
				// Ungültiges Format
				return null;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			// GUI -Werte in Selection packen
			_scraped.Description = textBoxRomNewDesc.Text.Trim();
			_scraped.Genre = textBoxRomNewGenre.Text.Trim();
			_scraped.Name = textBoxRomNewName.Text.Trim();
			_scraped.Players = textBoxRomNewPlayers.Text.Trim();
			_scraped.Developer = textBoxRomNewDev.Text.Trim();
			_scraped.Publisher = textBoxRomNewPub.Text.Trim();
			_scraped.ReleaseDateRaw = ConvertDateFormat(textBoxRomNewRelease.Text.Trim());
			_scraped.RatingNormalized = starRatingControlRomNew.Rating > 0.0 ? starRatingControlRomNew.Rating / 5.0 : 0.0;

			Selection.NewData = _scraped;
			Selection.TakeName = checkBoxName.Checked;
			Selection.TakeDesc = checkBoxDesc.Checked;
			Selection.TakeGenre = checkBoxGenre.Checked;
			Selection.TakePlayers = checkBoxPlayer.Checked;
			Selection.TakeDev = checkBoxDev.Checked;
			Selection.TakePub = checkBoxPub.Checked;
			Selection.TakeRating = checkBoxRating.Checked;
			Selection.TakeRelease = checkBoxRelease.Checked;

			foreach (MediaPreviewControl control in flowLayoutPanelMediaRight.Controls.OfType<MediaPreviewControl>())
			{
				if ( !string.IsNullOrEmpty(control.AbsolutPath)
					&& Selection.MediaTempPaths.ContainsKey(control.MediaType))
				{
					var tuple = Selection.MediaTempPaths[control.MediaType];
					tuple.take = control.CheckBox.Checked;
					Selection.MediaTempPaths[control.MediaType] = tuple;
				}
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private async Task<MediaPreviewControl> CreateMediaPreviewPanel(GameMediaSettings media, string baseDir, CancellationToken ct, bool checkboxen)
		{
			var control = new MediaPreviewControl();
			control.MediaType = media.Type;
			control.DisplayMode = checkboxen ? MediaPreviewControl.ControlDisplayMode.Checkbox : MediaPreviewControl.ControlDisplayMode.None;

			if (ct.IsCancellationRequested)
				return control;
			await control.LoadMediaAsync(media.FilePath, baseDir, media.NewData, media.ContentType, ct, _scraper, false);
			return control;
		}

		private void Cancel()
		{
			// laufende System-Ladevorgänge abbrechen
			Log.Debug("_romCts?.CancelAsync().Wait(2000);");
			_romCts?.CancelAsync().Wait(2000);
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Cancel();
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void FormScrapRom_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Cancel();
				e.Cancel = false;
			}
		}
	}

	public sealed class ScrapeSelection
	{
		public bool TakeName, TakeDesc, TakeGenre, TakePlayers, TakeDev, TakePub, TakeRating, TakeRelease;
		public bool TakeMediaBox, TakeMediaVideo, TakeMediaScreen;
		public Dictionary<eMediaType, (string tempPath, bool take)> MediaTempPaths { get; set; } = new();
		public GameScrap? NewData { get; set; }
	}
}
