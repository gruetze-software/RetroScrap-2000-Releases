using RetroScrap2000.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroScrap2000
{
	public struct AppInfo
	{
		public string ProductName, ProductVersion, Company, Copyright;
	}

	public static class Utils
	{
		public static string GetExcMsg(Exception ex)
		{
			string msg = ex.Message;
			if (ex.InnerException != null)
				msg += "\r\n" + GetExcMsg(ex.InnerException);
			return msg;
		}

		public static AppInfo GetAppInfo()
		{
			AppInfo retVal = new AppInfo();

			// Abrufen des Assembly-Objekts
			Assembly assembly = Assembly.GetExecutingAssembly();

			// Abrufen der Company-Information
			retVal.Company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Grütze-Soft";

			// Abrufen der Product-Information
			retVal.ProductName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "RetroScrap 2000";

			// Abrufen der Copyright-Information
			retVal.Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright © 2025 Grütze-Soft";

			// Abrufen der Informationsversion (InformationalVersion)
			retVal.ProductVersion = assembly.GetName().Version?.ToString() ?? "1.0.0";

			return retVal;
		}

		public static string DecodeTextFromApi(string? raw)
		{
			if (string.IsNullOrEmpty(raw)) return string.Empty;
			return WebUtility.HtmlDecode(raw);
		}

		/// Berechnet den Prozentwert basierend auf dem aktuellen Wert und dem Gesamtwert.
		/// </summary>
		/// <param name="current">Der aktuelle Wert.</param>
		/// <param name="total">Der Gesamtwert.</param>
		/// <returns>Der berechnete Prozentwert als Integer, oder 0 bei einem Gesamt von 0.</returns>
		public static int CalculatePercentage(int current, int total)
		{
			if (total == 0)
			{
				return 0;
			}

			// Vermeide Ganzzahldivision durch die Multiplikation mit 100.0 (einem double)
			return (int)((current / (double)total) * 100);
		}

		public static string GetNameFromFile(string? filePath)
		{
			// Liefert den Dateinamen OHNE Erweiterung zurück.
			// Path.GetFileNameWithoutExtension() kann das auch, aber bei
			// Namen wie "Dr. Mario.zip" liefert es nur "Dr." zurück.
			// Daher hier eine eigene Implementierung.

			if (string.IsNullOrWhiteSpace(filePath))
				return string.Empty;
			
			string filename = Path.GetFileName(filePath);
			int index = filename.LastIndexOf('.');
			if (index < 0)
				return filename.Trim(); // keine Erweiterung
			if ( index == 0)
				return string.Empty; // nur Erweiterung, kein Name
			
			return filename.Substring(0, index).Trim(); // Name ohne Erweiterung
		}

		public static bool? IsMediaIdentical(eMediaType typ, string? absPathNew, string? relPathOld, string systemRomPath )
		{
			if (typ == eMediaType.Unknown
					|| string.IsNullOrEmpty(absPathNew)
					|| !File.Exists(absPathNew))
				return null;

			if (string.IsNullOrEmpty(relPathOld))
				return false; // kein altes Medium, also nicht identisch
		
			// Altes Medium nicht mehr da
			var oldMedia = FileTools.ResolveMediaPath(systemRomPath, relPathOld);
			if (string.IsNullOrEmpty(oldMedia) || !File.Exists(oldMedia))
				return false;
			
			// Filegröße unterschiedlich?
			FileInfo old = new FileInfo(oldMedia);
			FileInfo neu = new FileInfo(absPathNew);
			if (old.Length != neu.Length)
				return false; // unterschiedliche Filegröße

			// Letzte Prüfung: Bilder unterschiedlich?
			if (typ != eMediaType.Video && typ != eMediaType.Manual
				&& ImageTools.ImagesAreDifferent(oldMedia, absPathNew))
				return false;

			return true;
		}

		public static void ForceHorizontalScrollForMediaPreviewControls(FlowLayoutPanel flowLayoutPanel)
		{
			if (flowLayoutPanel.Controls.Count == 0)
			{
				return;
			}

			Debug.Assert(flowLayoutPanel.Controls[0] is MediaPreviewControl, "Expected MediaPreviewControl in FlowLayoutPanel");
			int CONTROL_WIDTH = flowLayoutPanel.Controls[0].Width;
			int MARGIN = flowLayoutPanel.Controls[0].Margin.All;          // Der Margin-Wert Ihres Controls oder Panels
			int SCROLLBAR_SAFETY_MARGIN = 20; // Extra Platz für die Scrollbar selbst

			// Berechnung der Gesamtbreite aller UserControls
			int totalWidth = (flowLayoutPanel.Controls.Count * CONTROL_WIDTH) +
											 (flowLayoutPanel.Controls.Count * MARGIN) +
											 SCROLLBAR_SAFETY_MARGIN;

			// Nur setzen, wenn die berechnete Breite größer ist als die aktuelle Breite des Host-Panels.
			// Wir setzen hier die Breite des FlowLayoutPanel (des Kindes)
			if (totalWidth > flowLayoutPanel.Parent!.ClientRectangle.Width)
			{
				flowLayoutPanel.Width = totalWidth;
			}
			else
			{
				// Wichtig: Wenn die Gesamtbreite kleiner ist, muss die Breite des FlowLayoutPanel
				// wieder auf die Breite des Hosts gesetzt werden, um die Scrollbar zu entfernen.
				flowLayoutPanel.Width = flowLayoutPanel.Parent.ClientRectangle.Width;
			}

			// Setzen der Höhe auf die feste Höhe der Controls, um vertikale Probleme zu vermeiden
			flowLayoutPanel.Height = flowLayoutPanel.Controls[0].Height + MARGIN;
		}
	}
}
