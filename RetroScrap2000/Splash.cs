using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public static class Splash
	{
		private static FormSplashScreen? _splashScreen;
		private static Thread? _splashThread;

		public static void ShowSplashScreen()
		{
			// Sicherstellen, dass der Splashscreen nicht bereits läuft
			if (_splashThread == null)
			{
				_splashThread = new Thread(() =>
				{
					_splashScreen = new FormSplashScreen();

					// Setzt die Startposition auf die Bildschirmmitte. Dies ist Thread-sicher.
					_splashScreen.StartPosition = FormStartPosition.CenterScreen;

					// ShowDialog ohne Parent-Formular aufrufen. Es blockiert nur diesen Thread.
					_splashScreen.ShowDialog();
				});
				_splashThread.IsBackground = true;
				_splashThread.Start();
			}
		}

		public static async Task WaitForSplashScreenAsync()
		{
			// Warte, bis der Splashscreen initialisiert wurde
			while (_splashScreen == null || !_splashScreen.IsHandleCreated)
			{
				await Task.Delay(50); // Warte 50 ms und prüfe erneut
			}
		}

		public static void CloseSplashScreen()
		{
			// Check, ob der Splashscreen existiert UND sein Handle erstellt wurde
			if (_splashScreen != null && _splashScreen.IsHandleCreated)
			{
				// Invoke, um sicher vom korrekten Thread zu schließen
				_splashScreen.Invoke(new System.Action(() =>
				{
					_splashScreen.Close();
				}));

				// Nach dem Aufruf des Closings, die Referenz auf null setzen,
				// um weitere Zugriffe zu verhindern.
				_splashScreen = null;
				_splashThread = null;
			}
		}

		public static void UpdateSplashScreenStatus(string statusText)
		{
			if (_splashScreen != null && _splashScreen.IsHandleCreated)
			{
				// Invoke wird benötigt, da diese Methode von einem anderen Thread aufgerufen werden könnte
				_splashScreen.Invoke(new System.Action(() => _splashScreen.UpdateStatus(statusText)));
			}
		}

		// Status aktualisieren und warten.
		public static async Task ShowStatusWithDelayAsync(string statusText, int delayMilliseconds)
		{
			// 1. Status sofort aktualisieren
			UpdateSplashScreenStatus(statusText);

			// 2. Asynchron warten, ohne den aufrufenden Thread zu blockieren
			await Task.Delay(delayMilliseconds);
		}

		public static void UpdateSplashScreenProgress(int percentage)
		{
			if (_splashScreen != null && _splashScreen.IsHandleCreated)
			{
				_splashScreen.Invoke(new System.Action(() => _splashScreen.UpdateProgress(percentage)));
			}
		}

		public static void SetSplashScreenMarqueeProgress()
		{
			if (_splashScreen != null && _splashScreen.IsHandleCreated)
			{
				_splashScreen.Invoke(new System.Action(() => _splashScreen.SetMarqueeProgress()));
			}
		}
	}
}
