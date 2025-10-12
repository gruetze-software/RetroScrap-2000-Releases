using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class EmulatorLauncher
	{
		// --- EINSTELLUNGEN MÜSSEN HIER ANGEPASST WERDEN ---
		const string MameExePath = @"C:\Tools\MAME\mame.exe";

		public static async Task StartMameGame(string romName)
		{
			if (!File.Exists(MameExePath))
			{
				MessageBox.Show("FEHLER: MAME-Executable wurde nicht gefunden.", "MAME Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// ACHTUNG: MAME erwartet den ROM-NAMEN, nicht den vollen Pfad, wenn der rompath in mame.ini korrekt gesetzt ist.
			// Daher habe ich den Parameter von 'rompath' (was verwirrend ist) in 'romName' umbenannt.
			string arguments = $"{romName} -skip_gameinfo -window";

			Trace.WriteLine($"Starte MAME mit Spiel: {romName}...");
			Trace.WriteLine($"Verwendeter Befehl: \"{MameExePath}\" {arguments}");

			var processStartInfo = new ProcessStartInfo
			{
				FileName = MameExePath,
				Arguments = arguments,

				// --- ÄNDERUNGEN FÜR DIE FEHLERANALYSE ---
				UseShellExecute = false,        // Deaktiviert die Shell, damit wir die Ausgabe umleiten können.
				RedirectStandardOutput = true,  // Leitet die normale Konsolenausgabe um.
				RedirectStandardError = true,   // Leitet Fehler-Ausgabe um.
				CreateNoWindow = true           // Keine separate Konsole für MAME öffnen.
																				// ----------------------------------------
			};

			try
			{
				using var process = Process.Start(processStartInfo);
				if (process == null) return;

				// Starten des asynchronen Wartens im Hintergrund, um die UI nicht zu blockieren.
				await Task.Run(() => process.WaitForExit());

				// Die Ausgabe und Fehler nach Beendigung lesen.
				string output = await process.StandardOutput.ReadToEndAsync();
				string errors = await process.StandardError.ReadToEndAsync();

				if (process.ExitCode != 0)
				{
					// Wenn MAME einen Fehlercode zurückgibt, die Fehlermeldungen anzeigen.
					string errorMessage = $"MAME hat einen Fehler gemeldet (Exit Code: {process.ExitCode}).\n\n" +
																"Grund (Standard-Ausgabe):\n" + (string.IsNullOrWhiteSpace(output) ? "(Keine spezifische Ausgabe)" : output) +
																"\n\nGrund (Fehler-Ausgabe):\n" + (string.IsNullOrWhiteSpace(errors) ? "(Keine spezifische Fehlerausgabe)" : errors);

					MessageBox.Show(errorMessage, $"MAME Startfehler für {romName}", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					Trace.WriteLine($"MAME für {romName} wurde beendet (Exit Code 0).");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}", "Systemfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
