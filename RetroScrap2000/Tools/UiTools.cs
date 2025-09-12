using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000.Tools
{
	public static class UiTools
	{
		private const string BusyPreviewTag = "BUSY_PREVIEW";

		/// <summary>Zeigt einen statischen Busy-Platzhalter im PictureBox an.</summary>
		public static void ShowBusyPreview(PictureBox pb, string text = "Loading …")
		{
			// vorhandenes Busy-Bild freigeben
			if (pb.Tag as string == BusyPreviewTag && pb.Image != null)
			{
				pb.Image.Dispose();
				pb.Image = null;
			}

			int w = Math.Max(pb.ClientSize.Width, 64);
			int h = Math.Max(pb.ClientSize.Height, 64);

			var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			using (var g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.Clear(Color.FromArgb(24, 24, 24));

				// Kreisbogen (Spinner-Optik)
				var r = Math.Min(w, h) / 4;
				var cx = w / 2;
				var cy = h / 2 - 8;
				using (var pen = new Pen(Color.Silver, 4f))
					g.DrawArc(pen, cx - r, cy - r, r * 2, r * 2, 30, 300);

				// Text
				using var f = new Font(FontFamily.GenericSansSerif, Math.Max(10, w / 20f));
				var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				g.DrawString(text, f, Brushes.Gainsboro, new RectangleF(0, cy + r + 6, w, h - (cy + r + 6)), sf);
			}

			// evtl. fremdes Bild freigeben
			if (pb.Tag is string tag && tag != BusyPreviewTag && pb.Image != null)
			{
				pb.Image.Dispose();
				pb.Image = null;
			}

			pb.Image = bmp;
			pb.Tag = BusyPreviewTag;
		}

		/// <summary>Entfernt den Busy-Platzhalter, falls noch aktiv.</summary>
		public static void HideBusyPreview(PictureBox pb)
		{
			if (pb.Tag as string == BusyPreviewTag)
			{
				pb.Image?.Dispose();
				pb.Image = null;
				pb.Tag = null;
			}
		}

		public static void OpenPicBoxTagFile(PictureBox picbox)
		{
			if (picbox != null && picbox.Tag != null && picbox.Tag is string file && File.Exists(file))
			{
				try
				{
					var psi = new System.Diagnostics.ProcessStartInfo
					{
						FileName = file,
						UseShellExecute = true // wichtig: Standard-App verwenden
					};
					System.Diagnostics.Process.Start(psi);
				}
				catch (Exception ex)
				{
					MyMsgBox.ShowErr(Utils.GetExcMsg(ex));
				}
			}
		}
	}
}
