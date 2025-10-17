using RetroScrap2000.Tools;
using System.Globalization;
using Serilog;

namespace RetroScrap2000
{
  internal static class Program
  {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      // Die gespeicherte Sprache aus den Benutzereinstellungen lesen.
      // Wenn diese leer ist (erster Start), wird der Standardwert "en-US" verwendet.
      var options = RetroScrapOptions.Load();
      string? language = options.Language;
      
			if (string.IsNullOrEmpty(language))
      {
        language = CultureInfo.CurrentCulture.ToString();
        options.Language = language;
        options.Save();
			}

			// LogManager setzen
			LogManager.Initialize(options.Logging == true);

			// 2. Globale Exception-Hooks setzen
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			// Setzen der UI-Kultur für den aktuellen Thread
			CultureInfo culture = new CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = culture;
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.
      ApplicationConfiguration.Initialize();
			Log.Information("**************** Application is starting ****************");
      Application.Run(new FormMain(options));
			Log.Information("**************** Application is closed ****************");
			// Führt Aufräumarbeiten für Serilog durch, bevor die Anwendung beendet wird
			Log.CloseAndFlush();
		}

		// ---------------------------------------------------------------------
		// EXCEPTION HANDLER
		// ---------------------------------------------------------------------

		// A. UI-Thread-Fehler (meistens synchroner Code)
		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			// Protokolliert den Fehler als FATAL, da der UI-Thread abstürzt
			Log.Fatal(e.Exception, "Unhandled error in the UI thread: The application will terminate.");

			// Hier eine benutzerdefinierte Fehlermeldung anzeigen
			MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Error_CriticalShutdownWithLog);

			// Anwendung beenden
			Application.Exit();
		}

		// B. Hintergrund-Thread-Fehler (z.B. Task.Run oder Async-Fehler)
		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;

			// Protokolliert den Fehler als FATAL
			Log.Fatal(exception, "Unhandled error in a background thread (IsTerminating: {IsTerminating}).", e.IsTerminating);

			// Wenn IsTerminating true ist, wird die Anwendung sowieso beendet.
			if (e.IsTerminating)
			{
				MyMsgBox.ShowErr(Properties.Resources.Txt_Msg_Error_CriticalShutdown);
			}
			// Ansonsten könnte der Thread abbrechen, aber die Anwendung weiterlaufen
		}
	}
}