using RetroScrap2000.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RetroScrap2000
{
	public partial class FormScrapRom : Form
	{
		private readonly GameEntry _current;
		private readonly ScrapeGame _scraped;
		private readonly string _systemRomPath;

		public ScrapeSelection Selection { get; } = new();

		public FormScrapRom(string systemRomPath, GameEntry current, ScrapeGame scraped)
		{
			InitializeComponent();
			_current = current;
			_scraped = scraped;
			_systemRomPath = systemRomPath;
		}

		private async void FormScrapRom_Load(object sender, EventArgs e)
		{
			this.buttonOK.Enabled = false;
			this.UseWaitCursor = true;
			this.Cursor = Cursors.WaitCursor;
			Cursor.Current = Cursors.WaitCursor;

			// Links füllen (current)
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
			// Busy-Platzhalter anzeigen
			if (!string.IsNullOrEmpty(_current.MediaCoverPath))
				UiTools.ShowBusyPreview(pictureBoxRomOldFront, "Cover …");
			if (!string.IsNullOrEmpty(_current.MediaScreenshotPath))
				UiTools.ShowBusyPreview(pictureBoxRomOldScreen, "Screenshot …");
			if (!string.IsNullOrEmpty(_current.MediaVideoPath))
				UiTools.ShowBusyPreview(pictureBoxRomOldVideo, "Video …");

			try
			{
				// asynchron + gecached
				var coverTask = ImageTools.LoadImageCachedAsync(_systemRomPath, _current.MediaCoverPath, CancellationToken.None);
				var shotTask = ImageTools.LoadImageCachedAsync(_systemRomPath, _current.MediaScreenshotPath, CancellationToken.None);
				var prevTask = ImageTools.LoadVideoPreviewAsync(_systemRomPath, _current, CancellationToken.None);

				// Ergebnisse zuweisen + Busy ausblenden
				var cover = await coverTask;
				UiTools.HideBusyPreview(pictureBoxRomOldFront);
				pictureBoxRomOldFront.Image = cover;
				pictureBoxRomOldFront.Tag = FileTools.ResolveMediaPath(_systemRomPath, _current.MediaCoverPath);

				var shot = await shotTask;
				UiTools.HideBusyPreview(pictureBoxRomOldScreen);
				pictureBoxRomOldScreen.Image = shot;
				pictureBoxRomOldScreen.Tag = FileTools.ResolveMediaPath(_systemRomPath, _current.MediaScreenshotPath);

				var prev = await prevTask; // prev?.overlay = Image, prev?.videoAbsPath = Pfad fürs Klicken
				UiTools.HideBusyPreview(pictureBoxRomOldVideo);
				pictureBoxRomOldVideo.Image = prev?.overlay;
				pictureBoxRomOldVideo.Tag = prev?.videoAbsPath; // Klick startet Standardplayer
				pictureBoxRomOldVideo.Cursor = (prev?.overlay != null) ? Cursors.Hand : Cursors.Default;

				// Rechts füllen (scraped)
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

				// Busy-Platzhalter anzeigen
				if (!string.IsNullOrEmpty(_scraped.Box2DUrl))
					UiTools.ShowBusyPreview(pictureBoxRomNewFront, "Lade Cover …");
				if (!string.IsNullOrEmpty(_scraped.ImageUrl))
					UiTools.ShowBusyPreview(pictureBoxRomNewScreen, "Lade Screenshot …");
				if (!string.IsNullOrEmpty(_scraped.VideoUrl))
					UiTools.ShowBusyPreview(pictureBoxRomNewVideo, "Lade Video-Vorschau …");


				// asynchron + gecached
				var coverTasksc = ImageTools.LoadImageFromUrlCachedAsync(_scraped.Box2DUrl, CancellationToken.None);
				var shotTasksc = ImageTools.LoadImageFromUrlCachedAsync(_scraped.ImageUrl, CancellationToken.None);
				var prevTasksc = VideoTools.LoadVideoPreviewFromUrlAsync(_scraped.VideoUrl, CancellationToken.None);

				// Ergebnisse zuweisen + Busy ausblenden
				var coversc = await coverTasksc;
				UiTools.HideBusyPreview(pictureBoxRomNewFront);
				pictureBoxRomNewFront.Image = coversc;
				Selection.MediaBoxTempPath = FileTools.SaveImageToTempFile(coversc);
				pictureBoxRomNewFront.Tag = Selection.MediaBoxTempPath;

				var shotsc = await shotTasksc;
				UiTools.HideBusyPreview(pictureBoxRomNewScreen);
				pictureBoxRomNewScreen.Image = shotsc;
				Selection.MediaScreenTempPath = FileTools.SaveImageToTempFile(shotsc);
				pictureBoxRomNewScreen.Tag = Selection.MediaScreenTempPath;

				var prevsc = await prevTasksc; // prev?.overlay = Image, prev?.videoAbsPath = Pfad fürs Klicken
				UiTools.HideBusyPreview(pictureBoxRomNewVideo);
				pictureBoxRomNewVideo.Image = prevsc?.overlay;
				Selection.MediaVideoTempPath = prevsc?.videoAbsPath;
				pictureBoxRomNewVideo.Tag = Selection.MediaVideoTempPath; // Klick startet Standardplayer
				Selection.MediaVideoPreviewTempPath = prevsc?.videoPreviewAbsPath;
				pictureBoxRomNewVideo.Cursor = (prevsc?.overlay != null) ? Cursors.Hand : Cursors.Default;

				// Checkboxen setzen (nur wenn rechts was da ist)
				checkBoxName.Checked = !string.IsNullOrEmpty(_scraped.Name) && _scraped.Name != _current.Name;
				checkBoxDesc.Checked = !string.IsNullOrEmpty(_scraped.Description) && _scraped.Description != _current.Description;
				checkBoxGenre.Checked = !string.IsNullOrEmpty(_scraped.Genre) && _scraped.Genre != _current.Genre;
				checkBoxPlayer.Checked = !string.IsNullOrEmpty(_scraped.Players) && _scraped.Players != _current.Players;
				checkBoxDev.Checked = !string.IsNullOrEmpty(_scraped.Developer) && _scraped.Developer != _current.Developer;
				checkBoxPub.Checked = !string.IsNullOrEmpty(_scraped.Publisher) && _scraped.Publisher != _current.Publisher;
				checkBoxRating.Checked = _scraped.RatingNormalized.HasValue && _current.Rating != _scraped.RatingNormalized;
				checkBoxRelease.Checked = _scraped.ReleaseDate != _current.ReleaseDate;

				string? currentMedia = FileTools.ResolveMediaPath(_systemRomPath, _current.MediaCoverPath);
				checkBoxMediaFront.Checked = ( 
					!string.IsNullOrEmpty(Selection.MediaBoxTempPath) 
					&& ( string.IsNullOrEmpty(_current.MediaCoverPath) 
						|| string.IsNullOrEmpty(currentMedia) || !File.Exists(currentMedia) ) )
					|| ImageTools.ImagesAreDifferent(currentMedia, Selection.MediaBoxTempPath);

				currentMedia = FileTools.ResolveMediaPath(_systemRomPath, _current.MediaScreenshotPath);
				checkBoxMediaScreen.Checked = (
					!string.IsNullOrEmpty(Selection.MediaScreenTempPath) 
					&& ( string.IsNullOrEmpty(_current.MediaScreenshotPath)
						|| string.IsNullOrEmpty(currentMedia) || !File.Exists(currentMedia) ) )
					|| ImageTools.ImagesAreDifferent(currentMedia, Selection.MediaScreenTempPath);

				currentMedia = FileTools.ResolveMediaPath(_systemRomPath, _current.MediaVideoPreviewImagePath);
				checkBoxMediaVideo.Checked = (
					!string.IsNullOrEmpty(Selection.MediaVideoPreviewTempPath) 
					&& ( string.IsNullOrEmpty(_current.MediaVideoPreviewImagePath)
						|| string.IsNullOrEmpty(currentMedia) || !File.Exists(currentMedia) ) )
					|| ImageTools.ImagesAreDifferent(currentMedia, Selection.MediaVideoPreviewTempPath);
			}
			catch (OperationCanceledException)
			{
				// okay – wird vom Aufrufer gefangen
				throw;
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
			if ( string.IsNullOrEmpty(dt))
				return null;

			if ( DateTime.TryParseExact(dt, "dd.MM.yyyy", CultureInfo.InvariantCulture, 
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
			Selection.TakeMediaBox = checkBoxMediaFront.Checked;
			Selection.TakeMediaScreen = checkBoxMediaScreen.Checked;
			Selection.TakeMediaVideo = checkBoxMediaVideo.Checked;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}

	public sealed class ScrapeSelection
	{
		public bool TakeName, TakeDesc, TakeGenre, TakePlayers, TakeDev, TakePub, TakeRating, TakeRelease;
		public bool TakeMediaBox, TakeMediaVideo, TakeMediaScreen;
		public string? MediaBoxTempPath, MediaScreenTempPath, MediaVideoPreviewTempPath, MediaVideoTempPath;
		// die Werte, die übernommen werden würden (aus ScrapeGame)
		public ScrapeGame? NewData { get; set; }
	}
}
