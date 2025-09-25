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


	}
}
