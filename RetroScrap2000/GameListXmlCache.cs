using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RetroScrap2000
{

	/// <summary>
	/// Verwaltet das Caching und die Indexierung des EmulationStation gameList.xml-Dokuments, 
	/// um die Performance bei wiederholten Abfragen (wie der Duplikatprüfung) zu verbessern.
	/// </summary>
	public static class GameListXmlCache
	{
		private static XDocument? _cachedDoc;
		private static readonly object _cacheLock = new object();
		private static string? _currentXmlPath;

		// Dictionary speichert den Pfad und die Anzahl der Vorkommen
		private static Dictionary<string, int>? _pathCountIndex;

		// ***************************************************************
		// ÖFFENTLICHE METHODE ZUM PRÜFEN DER EXISTENZ
		// ***************************************************************

		/// <summary>
		/// Prüft, wie oft ein ROM-Pfad bereits in der XML-Datei existiert (nach Normalisierung).
		/// </summary>
		public static int GetNumbersOfEntriesInXmlCached(string xmlPath, string romPath)
		{
			// Sicherstellen, dass das Dokument geladen und der Index erstellt wurde
			if (GetOrLoadDocument(xmlPath) == null || _pathCountIndex == null)
			{
				return 0;
			}

			string normalizedRomPath = GameListLoader.NormalizeRelativePath(romPath);

			// Dictionary-Lookup: Ist der Pfad enthalten? Wenn ja, gib die Anzahl zurück, sonst 0.
			if (_pathCountIndex.TryGetValue(normalizedRomPath, out int count))
			{
				return count;
			}

			return 0;
		}

		/// <summary>
		/// Entfernt einen Eintrag aus dem Index und aktualisiert die Zählung, 
		/// ohne das gesamte XML-Dokument neu laden zu müssen.
		/// </summary>
		/// <param name="xmlPath">Der Pfad der XML-Datei.</param>
		/// <param name="romPath">Der zu entfernende normalisierte Pfad.</param>
		public static void DecrementEntryCount(string xmlPath, string romPath)
		{
			// Wir müssen nur den Cache des aktuell geladenen Systems bearbeiten
			if (!_currentXmlPath!.Equals(xmlPath, StringComparison.OrdinalIgnoreCase))
			{
				// Wenn der Pfad nicht übereinstimmt, leeren wir den Cache für die nächste Operation
				// (Dies geschieht normalerweise nicht, da die Löschung nur für das aktive System aufgerufen wird)
				ClearCache();
				return;
			}

			lock (_cacheLock)
			{
				if (_pathCountIndex == null || string.IsNullOrEmpty(romPath))
				{
					return;
				}

				string normalizedRomPath = GameListLoader.NormalizeRelativePath(romPath);

				if (_pathCountIndex.TryGetValue(normalizedRomPath, out int count))
				{
					if (count > 1)
					{
						// Nur die Zählung dekrementieren
						_pathCountIndex[normalizedRomPath] = count - 1;
					}
					else
					{
						// Letzten Eintrag entfernen
						_pathCountIndex.Remove(normalizedRomPath);
					}
				}
			}
		}

		// ***************************************************************
		// CACHING-METHODEN
		// ***************************************************************

		/// <summary>
		/// Lädt das XML-Dokument oder gibt die gecachte Version zurück. 
		/// Stellt sicher, dass der Pfad-Index nach dem Laden neu aufgebaut wird.
		/// </summary>
		private static XDocument? GetOrLoadDocument(string xmlPath)
		{
			lock (_cacheLock)
			{
				// Fall 1: Dokument ist bereits geladen und der Pfad ist identisch
				if (_cachedDoc != null && xmlPath.Equals(_currentXmlPath, StringComparison.OrdinalIgnoreCase))
				{
					return _cachedDoc;
				}

				// Fall 2: Muss neu geladen werden (entweder Pfad ist neu oder Cache ist leer)
				if (File.Exists(xmlPath))
				{
					try
					{
						_cachedDoc = XDocument.Load(xmlPath);
						_currentXmlPath = xmlPath;

						// Index sofort nach dem erfolgreichen Laden erstellen/aktualisieren
						BuildPathIndex(_cachedDoc);

						return _cachedDoc;
					}
					catch
					{
						// Fehler beim Laden (z.B. ungültiges XML)
						ClearCache();
						return null;
					}
				}
				// Datei existiert nicht
				ClearCache();
				return null;
			}
		}

		/// <summary>
		/// Erstellt den Index aller normalisierten Pfade aus dem geladenen Dokument.
		/// </summary>
		private static void BuildPathIndex(XDocument doc)
		{
			// Erstellung des Indexes: Gruppiert alle Pfade und zählt die Vorkommen
			_pathCountIndex = doc.Element("gameList")?
				 .Elements("game")
				 // Den Pfad aus dem XML holen und normalisieren
				 .Select(g => GameListLoader.NormalizeRelativePath(g.Element("path")?.Value ?? string.Empty))
				 .Where(p => !string.IsNullOrEmpty(p)) // Leere Pfade ignorieren
																							 // Nach dem Pfad gruppieren und zählen
				 .GroupBy(path => path, StringComparer.Ordinal)
				 .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
			// Stellt sicher, dass bei Null ein leeres Dictionary entsteht (oder eine passende Fehlerbehandlung)
		}

		/// <summary>
		/// Löscht das gesamte Caching. Muss nach jedem Speichervorgang der XML-Datei aufgerufen werden.
		/// </summary>
		public static void ClearCache()
		{
			lock (_cacheLock)
			{
				_cachedDoc = null;
				_currentXmlPath = null;
				_pathCountIndex = null;
			}
		}
	}

}
