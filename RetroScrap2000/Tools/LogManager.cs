using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000.Tools
{
	public static class LogManager
	{
		public static void Initialize(bool isLoggingEnabled)
		{
			// Setzt das Mindestlevel für die Log-Datei basierend auf der Option
			LogEventLevel fileMinLevel = isLoggingEnabled
					? LogEventLevel.Information // Wenn aktiv: Logge alles ab Information
					: LogEventLevel.Fatal;       // Wenn inaktiv: Logge nur Fatale Fehler (Abstürze)

			// Konfigurieren Sie den Logger
			Log.Logger = new LoggerConfiguration()
					.MinimumLevel.Debug() // Das Debug-Fenster (Trace) bekommt IMMER Debug

					// Debugger-Ausgabe (Trace) – immer aktiv
					.WriteTo.Debug()

					// Log-Datei mit Größenkontrolle – hängt von fileMinLevel ab
					.WriteTo.File(
							path: RetroScrapOptions.GetLoggingFile(),
							fileSizeLimitBytes: 10 * 1024 * 1024,
							rollOnFileSizeLimit: true,
							retainedFileCountLimit: 5,
							restrictedToMinimumLevel: fileMinLevel // <--- Dynamische Steuerung
					)
					.CreateLogger();

			Log.Debug("Logging system initialized.");
		}
	}
}
