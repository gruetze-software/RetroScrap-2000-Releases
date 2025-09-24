using System.Globalization;

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

			// Setzen der UI-Kultur für den aktuellen Thread
			CultureInfo culture = new CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = culture;
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.
      ApplicationConfiguration.Initialize();
      Application.Run(new FormMain(options));
    }
  }
}