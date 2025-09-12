using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public static class Utils
	{
		public static string GetExcMsg(Exception ex)
		{
			string msg = ex.Message;
			if (ex.InnerException != null)
				msg += "\r\n" + GetExcMsg(ex.InnerException);
			return msg;
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
	}
}
