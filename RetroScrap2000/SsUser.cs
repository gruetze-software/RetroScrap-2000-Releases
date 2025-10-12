using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public sealed class SsUserInfosResponse
	{
		public SsResponse? response { get; set; }
	}

	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public sealed class SsResponse
	{
		public SsUser? ssuser { get; set; }
		public SsServeurs? serveurs { get; set; }
	}

	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public sealed class SsUser
	{
		public string? id { get; set; }
		public int? numid { get; set; }
		public int? niveau { get; set; }
		public int? contribution { get; set; }
		public int? uploadsysteme { get; set; }
		public int? uploadinfos { get; set; }
		public int? romasso { get; set; }
		public int? uploadmedia { get; set; }
		public int? propositionok { get; set; }
		public int? propositionko { get; set; }
		public int? quotarefu { get; set; }
		public int? maxthreads { get; set; }
		public int? maxdownloadspeed { get; set; }        // Download-Geschwindigkeit (in KB/s), die für den Benutzer zulässig ist
		public int? requeststoday { get; set; } // Gesamtzahl der Aufrufe der API während des Tages in Kürze
		public int? requestskotoday { get; set; } // Anzahl der Aufrufe der API mit negativem Return (rom/set nicht gefunden) im Laufe des Tages kurz
		public int? maxrequestspermin { get; set; } // Anzahl der Aufrufe der maximal erlaubten API pro Minute für den Benutzer
		public int? maxrequestsperday { get; set; } // Die maximale Anzahl von API-Aufrufen, die pro Tag für den Benutzer zulässig sind
		public int? maxrequestskoperday { get; set; } // Maximale Anzahl von API-Aufrufen mit negativem Feedback (ROM/Set nicht gefunden), die pro Tag für den Benutzer zulässig sind
		public int? visites { get; set; } // Anzahl der Besuche des Benutzers bei ScreenScraper
		public string? datedernierevisite { get; set; }   // "yyyy-MM-dd HH:mm:ss"
		public string? favregion { get; set; } // bevorzugte Region der Besuche des Benutzers bei ScreenScraper (Frankreich, Europa, USA, Japan)

		public double UsedTodayPercent()
		{
			if (maxrequestsperday == null || maxrequestsperday.Value <= 0 )
				return 0;
			if (requeststoday == null || requeststoday.Value <= 0 )
				return 0;	
			
			return 100.0 * requeststoday!.Value / (double)maxrequestsperday!.Value;
		}

		public DateTime? LastVisit()
		{
			if (string.IsNullOrWhiteSpace(datedernierevisite)) return null;
			return DateTime.TryParseExact(
					datedernierevisite,
					"yyyy-MM-dd HH:mm:ss",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal,
					out var dt)
					? dt : null;
		}

		public string GetQuotaToday()
		{
			return string.Format(Properties.Resources.Txt_Api_SsUser_RequestPerDay,
								requeststoday != null ? requeststoday : "-",
								maxrequestsperday != null ? maxrequestsperday : "-",
								UsedTodayPercent());
		}
	}

	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public sealed class SsServeurs
	{
		public int? cpu1 { get; set; }
		public int? cpu2 { get; set; }
		public int? cpu3 { get; set; }
		public int? cpu4 { get; set; }
		public int? threadsmin { get; set; }
		public int? nbscrapeurs { get; set; }
		public int? apiacces { get; set; }
		public int? closefornomember { get; set; }
		public int? closeforleecher { get; set; }
		public int? maxthreadfornonmember { get; set; }
		public int? threadfornonmember { get; set; }
		public int? maxthreadformember { get; set; }
		public int? threadformember { get; set; }
	}

	public sealed class ScreenScraperApiException(int statusCode, string message) : Exception(message)
	{
		public int StatusCode { get; } = statusCode;
	}
}
