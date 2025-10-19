using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RetroScrap2000
{
	public class RetroSystem : IComparable<RetroSystem>
	{
		public int Id { get; set; } = -1;
		public string Name_eu { get; set; } = "Unknown System";
		public string Name_us { get; set; } = "Unknown System";
		public string? Extensions { get; set; }
		public string? RomType { get; set; }
		public string? SupportType { get; set; }
		public string RomFolderName { get; set; } = "romsystem";
		public string? Hersteller { get; set; }
		public string? Typ { get; set; }
		public string? Description { get; set; }
		public int Debut { get; set; } = 0;
		public int Ende { get; set; } = 0;
		[JsonIgnore]
		public string? FileIcon { get { return Path.Combine(RetroSystems.FolderIcons, RomFolderName.ToLower() + ".png"); } }
		[JsonIgnore]
		public string? FileBanner { get { return Path.Combine(RetroSystems.FolderBanner, RomFolderName.ToLower() + ".png"); } }

		public RetroSystem() { }

		public override string ToString()
		{
			return Name_eu;
		}

		// Definiert die Sortierlogik für List<T>.Sort()
		public int CompareTo(RetroSystem? other)
		{
			if (other == null) return 1;

			// Vergleiche die Systemnamen
			return string.Compare(this.Name_eu, other.Name_eu, StringComparison.OrdinalIgnoreCase);
		}

		public bool SaveAllScrapedRomsToGamelistXml(string romPath, IEnumerable<GameEntry> roms)
		{
			// Pfade
			string xmlPath = romPath;
			if (!romPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
					xmlPath = Path.Combine(romPath, "gamelist.xml");
			var backupPath = xmlPath + ".bak";
			var tempPath = xmlPath + ".tmp";

			// Sperrt den Dateizugriff, um Race Conditions zu verhindern
			lock (_xmlFileLock)
			{
				XDocument doc;

				// 1. DOKUMENT LADEN ODER NEU ERSTELLEN
				if (File.Exists(xmlPath))
				{
					try
					{
						// Laden des Originals
						doc = XDocument.Load(xmlPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
					}
					catch (System.Xml.XmlException ex)
					{
						// Fehler beim Laden (z.B. nach einem unsauberen Abbruch) -> Versuch, Backup zu nutzen
						if (File.Exists(backupPath))
						{
							doc = XDocument.Load(backupPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
							// Beschädigte Originaldatei löschen (optional, aber sauber)
							File.Delete(xmlPath);
						}
						else
						{
							// Weder Original noch Backup sind lesbar.
							throw new Exception($"Fehler beim Laden von {xmlPath}. Das Backup ist ebenfalls fehlerhaft oder nicht vorhanden.", ex);
						}
					}
				}
				else
				{
					// Neue gamelist.xml erstellen
					doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
													new XElement("gameList"));
				}

				var root = doc.Element("gameList") ?? new XElement("gameList");

				// 2. ALLE GEÄNDERTEN ROMS VERARBEITEN (EINZIGES MAL)
				foreach (var rom in roms.Where(r => r.State == eState.Scraped ))
				{
					var relPath = GetRomPathForXml(romPath, rom);

					// <game> anhand <path> suchen
					var gameEl = root.Elements("game")
													 .FirstOrDefault(x => string.Equals((string?)x.Element("path"), relPath, StringComparison.OrdinalIgnoreCase));

					// Das game-Element nur erstellen, wenn es nicht existiert
					if (gameEl == null)
					{
						gameEl = new XElement("game");
						root.Add(gameEl);
					}

					// Daten des GameEntry-Objekts in das XML-Element schreiben
					SetRomToXml(gameEl, relPath, romPath, rom);
				}

				// Bereinige den Dokumentbaum vor dem Speichern (optional)
				doc.DescendantNodes()
						.Where(n => n.NodeType == System.Xml.XmlNodeType.Text && string.IsNullOrWhiteSpace(n.ToString()))
						.Remove();

				// 3. ATOMARES SPEICHERN (EINZIGES MAL)
				try
				{
					// Konfiguration für den XML Writer
					var xmlWriterSettings = new System.Xml.XmlWriterSettings
					{
						Indent = true,
						IndentChars = "    ",
						Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
						NewLineChars = Environment.NewLine,
						NewLineHandling = System.Xml.NewLineHandling.Replace,
						OmitXmlDeclaration = false
					};

					// Sicherstellen, dass die temporäre Datei nicht existiert
					if (File.Exists(tempPath))
					{
						try { File.Delete(tempPath); } catch { }
					}

					// 3a. Schreibe IMMER in die temporäre Datei
					using (var w = System.Xml.XmlWriter.Create(tempPath, xmlWriterSettings))
						doc.Save(w);

					// 3b. Prüfe auf Existenz der Zieldatei und wähle die Methode
					if (File.Exists(xmlPath))
					{
						// Zieldatei existiert -> ATOMAR ERSETZEN (Sichert das Original)
						File.Replace(tempPath, xmlPath, backupPath, ignoreMetadataErrors: true);
					}
					else
					{
						// Zieldatei existiert NICHT -> EINFACH VERSCHIEBEN
						File.Move(tempPath, xmlPath);
					}

					return true;
				}
				catch (Exception)
				{
					// Wenn das Speichern fehlschlägt, wird die Originaldatei nicht beschädigt.
					throw;
				}
				finally
				{
					// Cleanup: Temporäre Datei löschen (falls noch vorhanden)
					if (File.Exists(tempPath))
					{
						try { File.Delete(tempPath); } catch { /* Ignorieren */ }
					}
				}
			}
		}

		public bool SaveRomToGamelistXml(string romPath, GameEntry rom)
		{
			// Pfade
			var xmlPath = Path.Combine(romPath, "gamelist.xml");
			var backupPath = xmlPath + ".bak";
			var tempPath = xmlPath + ".tmp"; // NEU: Temporärer Pfad

			// relative <path> bestimmen – Primärschlüssel
			var relPath = GetRomPathForXml(romPath, rom);

			lock (_xmlFileLock)
			{
				// 1. BACKUP / DOKUMENT LADEN (Bleibt unverändert)
				// ... (Backup-Logik) ...

				XDocument doc;
				if (File.Exists(xmlPath))
				{
					try
					{
						doc = XDocument.Load(xmlPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
					}
					catch (Exception ex)
					{
						// Wenn die Datei beim Laden beschädigt ist, versuchen Sie, das Backup zu nutzen
						if (File.Exists(backupPath))
						{
							doc = XDocument.Load(backupPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
						}
						else
						{
							// Dokument kann nicht geladen werden, egal ob Original oder Backup
							throw new Exception($"Fehler beim Laden von {xmlPath} und Backup: {ex.Message}");
						}
					}
				}
				else
				{
					doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
													new XElement("gameList"));
				}

				// ... (Suchen, Erstellen und Setzen der game-Elemente) ...
				var root = doc.Element("gameList") ?? new XElement("gameList");

				// <game> anhand <path> suchen
				var gameEl = root.Elements("game")
												 .FirstOrDefault(x => string.Equals((string?)x.Element("path"), relPath, StringComparison.OrdinalIgnoreCase));

				// Das game-Element nur erstellen, wenn es nicht existiert
				if (gameEl == null)
				{
					gameEl = new XElement("game");
					root.Add(gameEl);
				}

				SetRomToXml(gameEl, relPath, romPath, rom);

				// Bereinige den Dokumentbaum vor dem Speichern (optional, aber empfohlen)
				doc.DescendantNodes()
						.Where(n => n.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(n.ToString()))
						.Remove();

				// 2. ATOMARES SPEICHERN
				try
				{
					// Konfiguration für den XML Writer
					var xmlWriterSettings = new System.Xml.XmlWriterSettings
					{
						Indent = true,
						IndentChars = "    ", // 4 Leerzeichen sind Standard
						Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
						NewLineChars = Environment.NewLine,
						NewLineHandling = System.Xml.NewLineHandling.Replace,
						OmitXmlDeclaration = false
					};

					// Sicherstellen, dass die temporäre Datei nicht existiert, um Konflikte zu vermeiden
					if (File.Exists(tempPath))
					{
						try { File.Delete(tempPath); } catch { }
					}

					// 2a. Schreibe IMMER in die temporäre Datei
					using (var w = System.Xml.XmlWriter.Create(tempPath, xmlWriterSettings))
						doc.Save(w);

					// 2b. Prüfe auf Existenz der Zieldatei und wähle die Methode
					if (File.Exists(xmlPath))
					{
						// SZENARIO A: Zieldatei existiert -> ATOMAR ERSETZEN
						// File.Replace ersetzt xmlPath mit tempPath und sichert das Original in backupPath.
						// Die manuelle File.Copy am Anfang ist hier NICHT mehr nötig, da Replace das Backup erstellt.
						File.Replace(tempPath, xmlPath, backupPath, ignoreMetadataErrors: true);
					}
					else
					{
						// SZENARIO B: Zieldatei existiert NICHT -> EINFACH VERSCHIEBEN (UMBENENNEN)
						// Das Backup ist hier irrelevant.
						File.Move(tempPath, xmlPath);
					}

					return true;
				}
				catch
				{
					// Wenn das Speichern in die temporäre Datei fehlschlägt,
					// bleibt die Originaldatei unberührt und unbeschädigt.
					// Die Exception wird weitergeleitet.
					throw;
				}
				finally
				{
					// Cleanup: Sollte die temporäre Datei noch existieren (z.B. nach einer Exception), löschen
					if (File.Exists(tempPath))
					{
						try { File.Delete(tempPath); } catch { /* Ignorieren */ }
					}
				}
			}
		}

		public bool SaveAllRomsToGamelistXml(string baseDir, IEnumerable<GameEntry> roms)
		{
			// Pfade
			var xmlPath = Path.Combine(baseDir, "gamelist.xml");
			var sysDir = Directory.GetParent(xmlPath)?.FullName ?? baseDir;
			var backupPath = xmlPath + ".bak";

			lock (_xmlFileLock)
			{
				Log.Information($"[{this.Name_eu}]: RetroSystem::SaveAllRomsToGamelistXml()");
				// Backup (nur wenn Datei existiert)
				if (File.Exists(xmlPath))
				{
					try { File.Copy(xmlPath, backupPath, overwrite: true); }
					catch { /* ignorieren */ }
				}

				// Neues Dokument anlegen
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("gameList"));
				var root = doc.Element("gameList")!;

				foreach (var rom in roms)
				{
					// relative <path> bestimmen – Primärschlüssel
					var relPath = GetRomPathForXml(sysDir, rom);
					var gameEl = new XElement("game");
					SetRomToXml(gameEl, relPath, sysDir, rom);
					root.Add(gameEl);
				}

				// Bereinige den Dokumentbaum vor dem Speichern.
				// Entfernt alle leeren Textknoten (Whitespace) und unnötige Kommentare,
				// die die automatische Einrückung stören könnten.
				doc.DescendantNodes()
						.Where(n => n.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(n.ToString()))
						.Remove();

				try
				{
					var xmlWriterSettings = new System.Xml.XmlWriterSettings
					{
						Indent = true,
						IndentChars = "    ", // Normales Leerzeichen verwenden
						Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
						NewLineChars = Environment.NewLine,
						NewLineHandling = System.Xml.NewLineHandling.Replace,
						OmitXmlDeclaration = false
					};
					using (var w = System.Xml.XmlWriter.Create(xmlPath, xmlWriterSettings))
						doc.Save(w);

					return true;
				}
				catch (Exception ex)
				{
					Log.Error(Utils.GetExcMsg(ex));
					return false;
				}
			}
		}

		private static readonly object _xmlFileLock = new(); // primitive Sperre pro Prozess
		private static void SetRomToXml(XElement gameEl, string relPath, string sysDir, GameEntry rom)
		{
			// Hilfsmethode, um ein Element zu setzen oder zu entfernen
			void SetElementValue(XElement parent, string name, string? value)
			{
				var element = parent.Element(name);
				if (string.IsNullOrEmpty(value))
				{
					// Wenn der Wert null oder leer ist, Element entfernen
					element?.Remove();
				}
				else
				{
					if (element == null)
					{
						// Wenn das Element nicht existiert, neues hinzufügen
						parent.Add(new XElement(name, value));
					}
					else
					{
						// Wenn das Element existiert, Wert aktualisieren
						element.Value = value;
					}
				}
			}
			void SetAttribute(XElement element, string attributeName, string? value)
			{
				if (string.IsNullOrEmpty(value))
				{
					element.Attribute(attributeName)?.Remove();
				}
				else
				{
					element.SetAttributeValue(attributeName, value);
				}
			}

			// Setzen Sie zuerst das path-Element
			SetElementValue(gameEl, "path", relPath);

			// Setzen Sie alle anderen Elemente mit der Hilfsmethode
			SetElementValue(gameEl, "name", NullIfEmpty(rom.Name));
			SetElementValue(gameEl, "desc", NullIfEmpty(rom.Description));
			SetElementValue(gameEl, "genre", NullIfEmpty(rom.Genre));
			SetElementValue(gameEl, "players", NullIfEmpty(rom.Players));
			SetElementValue(gameEl, "developer", NullIfEmpty(rom.Developer));
			SetElementValue(gameEl, "publisher", NullIfEmpty(rom.Publisher));
			SetElementValue(gameEl, "rating", rom.Rating > 0 ? rom.Rating.ToString("0.00", CultureInfo.InvariantCulture) : null);
			SetElementValue(gameEl, "releasedate", rom.ReleaseDateRaw);
			SetElementValue(gameEl, "favorite", rom.FavoriteString);
			foreach (var kvp in rom.MediaTypeDictionary)
				{
					string xmlTag = RetroScrapOptions.GetStandardMediaFolderAndXmlTag(kvp.Key);
					SetElementValue(gameEl, xmlTag, EnsureRelativeMedia(sysDir, kvp.Value));
				}

			// Attribute setzen
			SetAttribute(gameEl, "id", rom.Id.ToString());
			SetAttribute(gameEl, "source", rom.Source ?? "");
		}

		static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

		/// Primärschlüssel in der XML: <path> (relativ zum Systemordner)
		private string GetRomPathForXml(string systemDir, GameEntry rom)
		{
			// Falls in GameEntry bereits ein XML-konformer relativer Pfad liegt – nutze ihn.
			// Sonst aus absolutem Pfad ein "./…" bauen.
			if (!string.IsNullOrWhiteSpace(rom.Path) && rom.Path.StartsWith("./"))
				return rom.Path.Replace('\\', '/');

			// Fallback: Absolut → relativ
			// rom.Path kann absolut sein; wir rechnen relativ zum Systemordner:
			var abs = !string.IsNullOrEmpty(rom.Path) ? Path.Combine(systemDir, rom.Path.TrimStart('.', '\\', '/')) : null;
			if (!string.IsNullOrEmpty(abs) && File.Exists(abs))
				return "./" + Path.GetFileName(abs);

			// Letzte Instanz: nur Dateiname aus Name/Path
			var file = Path.GetFileName(rom.Path ?? rom.Name ?? "unknown.rom");
			return "./" + file.Replace('\\', '/');
		}

		/// Medien relativ machen (./media/…); wenn schon relativ: unverändert
		private static string? EnsureRelativeMedia(string systemDir, string? mediaFromModel)
		{
			if (string.IsNullOrWhiteSpace(mediaFromModel))
				return null;

			// Bereits relativ?
			if (mediaFromModel.StartsWith("./") || mediaFromModel.StartsWith(".\\"))
				return mediaFromModel.Replace('\\', '/');

			// Absolut unterhalb des Systemordners? → relativer Pfad ab Systemordner
			try
			{
				var full = Path.GetFullPath(mediaFromModel);
				var sys = Path.GetFullPath(systemDir);
				if (full.StartsWith(sys, StringComparison.OrdinalIgnoreCase))
				{
					var rel = full.Substring(sys.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					return "./" + rel.Replace('\\', '/');
				}
			}
			catch { /* ignorieren – gib Original zurück */ }

			// als Fallback Original zurück (EmulationStation kann auch absolute Pfade)
			return mediaFromModel.Replace('\\', '/');
		}
	}

	public class RetroSystems
	{
		public List<RetroSystem> SystemList { get; set; } = new List<RetroSystem>();
		public int? SystemListVersion { get; set; }
		[JsonIgnore]
		public int SystemListAktVersion { get; internal set; }

		[JsonIgnore]
		public bool IsTooOld { get; internal set; }

		[JsonIgnore]
		public static string FolderBanner = Path.Combine(AppContext.BaseDirectory, "Resources", "System_Banner");
		[JsonIgnore]
		public static string FolderIcons = Path.Combine(AppContext.BaseDirectory, "Resources", "System_Icons");
		public RetroSystems() 
		{
			IsTooOld = false;
			SystemListAktVersion = 1;
		}

		public string GetRomFolder(int systemid)
		{
			var sys = SystemList.FirstOrDefault(x => x.Id == systemid);
			if (sys == null)
			{
				Log.Information("[RetroSystem::GetRomFolder] skip Id " + systemid.ToString());
				return "";
			}
			else
				return sys.RomFolderName ?? "";
		}

		public async Task SetSystemsFromApiAsync(ScrapperManager _scrapper)
		{
			SystemList = new List<RetroSystem>();
			var (ok, data, error) = await _scrapper.GetSystemsAsync();
			if (ok)
			{
				foreach (var s in data)
				{
					RetroSystem retroSystem = new RetroSystem()
					{
						Debut = !string.IsNullOrEmpty(s.datedebut) ? (int.TryParse(s.datedebut, out int y) ? y : 0) : 0,
						Ende = !string.IsNullOrEmpty(s.datefin) ? (int.TryParse(s.datefin, out int z) ? z : 0) : 0,
						Hersteller = s.compagnie,
						Id = s.id,
						Name_eu = !string.IsNullOrEmpty(s.Name_eu) ? s.Name_eu : "Unknown System",
						Name_us = !string.IsNullOrEmpty(s.Name_us) ? s.Name_us : s.Name_eu ?? "Unknown System",
						Extensions = s.extensions,
						RomType = GetEngFromFrance(s.romtype),
						SupportType = GetEngFromFrance(s.supporttype),
						Typ = GetEngFromFrance(s.type),
						RomFolderName = BatoceraFolders.MapToBatoceraFolder(s.noms)!

					};
					
					SystemList.Add(retroSystem);
				}
			}
		}

		private string GetEngFromFrance(string? type)
		{
			if (string.IsNullOrEmpty(type)) return string.Empty;

			if (type.ToLower() == "console portable") return "Handheld Console";
			if (type.ToLower() == "ordinateur") return "Computer";
			if (type.ToLower() == "accessoire") return "Accessory";
			if (type.ToLower() == "emulation arcade") return "Arcade Emulation";
			if (type.ToLower() == "autres") return "Others";
			if (type.ToLower() == "machine virtuelle") return "Virtual Machine";
			if (type.ToLower() == "flipper") return "Pinball";
			if (type.ToLower() == "dossier") return "folder";
			if (type.ToLower() == "fichier") return "file";
			if (type.ToLower() == "cartouche") return "cartridge";
			if (type.ToLower() == "disquette") return "floppy";
			if (type.ToLower() == "cartouche-download") return "cartridge-download";
			if (type.ToLower() == "k7") return "cassette";
			if (type.ToLower() == "k7-disquette") return "cassette-floppy";
			if (type.ToLower() == "cartouche-k7") return "cartridge-floppy";
			if (type.ToLower() == "cartouche-k7-disquette") return "cartridge-cassette-floppy";
			if (type.ToLower() == "carte") return "card";
			if (type.ToLower() == "cd-disquette") return "cd-floppy";
			if (type.ToLower() == "non-applicable") return "not applicable";
			if (type.ToLower() == "bluray") return "Blu-ray";



			return type;
		}

		private static string GetRetroSystemsFile()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var dir = Path.Combine(appData, "RetroScrap2000");
			Directory.CreateDirectory(dir); // sicherstellen, dass Ordner existiert
			return Path.Combine(dir, "retrosystems.json");
		}

		/// <summary>
		/// Speichert die Systeme als Json-Datei.
		/// </summary>
		public void Save()
		{
			SystemListVersion = SystemListAktVersion;
			var options = new JsonSerializerOptions
			{
				WriteIndented = true // hübsch formatiert
			};
			var json = JsonSerializer.Serialize(this, options);
			File.WriteAllText(GetRetroSystemsFile(), json);
		}

		/// <summary>
		/// Lädt die Retrosysteme aus einer Json-Datei. 
		/// Falls die Datei nicht existiert, wird ein neues Objekt zurückgegeben.
		/// </summary>
		public void Load()
		{
			this.SystemListVersion = null;
			this.SystemList = new List<RetroSystem>();
			this.IsTooOld = false;	

			var systemjsonfile = GetRetroSystemsFile();
			if (!File.Exists(systemjsonfile))
			{
				return;
			}
				
			var json = File.ReadAllText(GetRetroSystemsFile());
			var retVal = JsonSerializer.Deserialize<RetroSystems>(json);
			if (retVal == null)
			{
				return;
			}
			else
			{
				this.SystemListVersion = retVal.SystemListVersion;
				this.SystemList = retVal.SystemList;
			}

			if (SystemListVersion == null || SystemListVersion.Value < SystemListAktVersion)
				this.IsTooOld = true;

			// TODO Einmalig zum Füllen der Beschreibung: 
			//fillDescription(retVal);
		}

		private static void fillDescription(RetroSystems retVal)
		{
			foreach (var system in retVal.SystemList)
			{
				if (system.RomFolderName?.ToLower() == "snes")
					system.Description = "Super Nintendo Entertainment System is still known by many as SNES or Super NES. This is a gaming console with a " +
					 "viral console platform worldwide. This SNES platform was developed and published by a global famous publisher Nintendo, this is a famous " +
					 "game developer from Japan. First launched in Japan in 1990 and launched in other countries around the world in the following years. SNES is " +
					 "the next generation of the machine that was once famous across the globe Nintendo Entertainment System, also known as NES. " +
					 "It showed advanced audio and graphics capabilities compared to other systems at the time. This device was created to be able to compete " +
					 "with others from the developers of the gaming devices at that time, namely SEGA. But the SNES platform has risen and become the best-selling " +
					 "device of the 16-bit gaming era. Not only that but after the world has reached the era of 32-bit games, this device still been popular around " +
					 "the world. The device’s number of devices sold is approximately 50 million units sold worldwide, and 17.17 million units were sold in Japan alone. " +
					 "In 1997, SNES enthusiasts started programming an emulator called ZSNES. SNES had a very familiar 2D graphics format with a very popular 16-bit style at the " +
					 "time. Gamepad of the device consists of 10 buttons all, including four navigation buttons located on the left side and four buttons used to fight located " +
					 "on the right-hand side. Because it was released in the 90s of the last century, the graphics and gameplay of the game could not be rich enough to make them as " +
					 "heavy as they are now. Most games on the device use a maximum of a few megabytes only. So players will not have to spend too much space to download " +
					 "the application. What makes this device popular all over the world are the games it brings to users. Among them are some of the famous ones such as " +
					 "Super Mario Bros 4, The Legend of Zelda: A Link to the Past, Super Bomberman. And many other titles I cannot mention here because the total number of titles " +
					 "present in this series is up to nearly 1,000 different games for players to experience. In the era of developing technology, the gaming industry is growing more " +
					 "than ever with more and more high-quality 3D graphics games. Everything is excellent but can not give players the same experience as before. So if you want to " +
					 "revive your childhood memories, come to us right away.";
				else if (system.RomFolderName?.ToLower() == "gb")
					system.Description = "Game Boy is an 8-bit handheld console developed by Nintendo, first released in Japan on April 21, 1989, and only three months later in North " +
						"America and finally in Europe one year later. As of 1997, Game Boy shipped globally a total of nearly 65 million and quickly created a craze at the time." +
						"Being the first generation, Game Boy’s design is quite simple, suitable for all subjects including women and children. It is equipped with a black and green " +
						"reflective LCD screen, a navigation button, two A and B action control buttons, and finally the Start and Select function buttons. Although simple, the experience " +
						"that it brings is extremely excellent and highly entertaining. When mentioning Game Boy, you will not forget the game that brought it to the peak of success " +
						"is Tetris, a simple but extremely addictive game like this system. Following on from its success was a series of other typical generations such as Game Boy " +
						"Color, Game Boy Advance, etc. and they were collectively known as the Game Boy family. There are also a lot of other variations with new designs but basically, " +
						"the gameplay remains the same.";
				else if (system.RomFolderName?.ToLower() == "gbc")
					system.Description = "Perhaps the majority of players are familiar with console systems, but there is still a machine called mini console that was released before. " +
						"Among them is a prevalent type, the Game Boy Color, which is also known by many as GBC. GBC was produced by a very famous company from Japan, Nintendo. " +
						"The device is a “handheld game console”, first released on October 21, 1998, in Japan and then released in November of the same year to international markets. " +
						"The device is a trendy gaming device worldwide, with sales of 118.69 million units, including other Game Boy devices. This device is an upgraded version of Game Boy " +
						"that was very famous before, GBC has many new upgrades to bring users exciting experiences. Game Boy Color is part of the fifth-generation home gaming console. " +
						"GBC’s main competitors in Japan are 16-bit gray handsets, Neo Geo Pocket and WonderSwan, although Game Boy Color far outpaces these products. The difference here " +
						"is that GBC has a color screen, not a monochrome screen like the previous version. This has brought players an exciting experience. This device is made up of many of " +
						"the most modern components of the era, such as the CPU Corporation Sharp Corporation LR35902, which is the same CPU of the previous version Game Boy. The device has a " +
						"resolution of 160 × 144 pixels and a 10: 9 aspect ratio, which is similar to the last version of the device. It’s slightly thicker, taller, and has a slightly smaller " +
						"screen than Game Boy Pocket, the direct predecessor in the Game Boy line. Because of its compatibility with Game Boy games, Game Boy Color has a large library of games " +
						"that can be played at launch. The system has accumulated an impressive library of 576 Game Boy Color games over four years. This is the fun of this device, and players " +
						"will not have to cumbersome connection, just boot up the device and play games. In particular, the game Pokemon Gold and Silver is the best-selling game developed for " +
						"Game Boy Color.";
				else if (system.RomFolderName?.ToLower() == "gba")
					system.Description = "Game Boy Advance (or GBA) is Nintendo’s most successful gaming console, with nearly 100 million devices sold worldwide, it has brought joy and inspiration " +
						"to many, the owner is children and adolescents. With its compact size and the most powerful hardware at the time, GBA became the most popular console system in the early " +
						"2000s. Game Boy Advance has 32bit 2D graphics hardware, 2.9-inch display screen including two physical buttons on the right and 4 scroll buttons on the left. Most of " +
						"GBA’s games are platformer and RPG, with only a few megabytes in size, you can easily download our GBA ROMs files below to play with your mobile emulator or the PC. " +
						"In the current 4.0 era, many people are too familiar with the game with beautiful graphics and a deep story. However, there are still many people want to recall their " +
						"childhood memories with the classic retro games and GBA is the bridge to connect you and those feelings. Would you like to join Pokemon training in Pokémon FireRed and " +
						"LeafGreen and Pokémon Emerald? Or do you want to immerse yourself in the fantasy world of Final Fantasy, or simply rescue the princess with the dwarf Mario? All of these " +
						"legendary games are available for download. While the NES (Nintendo Entertainment System) helped Nintendo become a household name in every home TV, Game Boy ushered in " +
						"the era of handheld gaming consoles that could be taken anywhere. Game Boy is considered the premise for the modern handheld game machine later, especially Nintendo DS, a " +
						"system that inherits all the best of GBA. Game Boy’s success was not due to luck, but superiority, a breakthrough design at the time. More importantly, mobility as a " +
						"smartphone and a battery life was dreaming compared to the Nintendo Switch of the time from now on. Besides the main versions, there are more than 50 variants of Game Boy " +
						"released such as Game Boy Kirby, Tamagochi, Hello Kitty, Pokémon … or Game Boy Advance SP, Game Boy Light (exclusively for Japan) and many sessions limited edition only. " +
						"Currently, it is very difficult to collect the limited machines but to buy Game Boy Advance; you can go to Amazon to search for.";
				else if (system.RomFolderName?.ToLower() == "nes")
					system.Description = "The global gaming community knows Nintendo Entertainment System as NES, which is an 8-bit console-based gaming device. The device is manufactured by " +
						"Nintendo, a manufacturer specializing in Japanese technology and games. This is a very well-known device before, especially for the 9x generation, when you are young, " +
						"surely many players want to have this NES. The device was first launched in July 15, 1983, in Japan and sold in the US and Europe in 1985 and 1986. After a series of " +
						"successful arcade games in the early 1980s, Nintendo plans to create a tape game machine called Famicom, which stands for Family Computer. And so the device was born in " +
						"no small number of players in 1983. The device was discontinued on September 25, 2003, during 20 years of production, the device has sold 61.91 million units above. " +
						"Around the world. NES is an 8-bit console and includes a Ricoh 2A03 processor paired with 2KB of RAM. This allows the system to display a screen of 256 × 240 resolution " +
						"with 48 colors and six grey variants. The handle of the device is straightforward, which includes 4 five navigation buttons on the left side of the handle and four buttons " +
						"to control combat characters. NES allows two players to play at the same time with two controllers at the same time to share experiences and support each other when playing " +
						"games. After being discontinued, NES has a lot of different emulators, so people can also experience the games of this device. In it, we have NESticle licensed by Nintendo " +
						"to operate. The device has a lot of famous games, such as “The Legend of Zelda”, “Zelda II: The Adventure of Link”, “Super Mario Bros. 2” and there are many famous titles " +
						"of the device. Among them, the game with the most players is “Super Mario Bros” with more than 40 million units sold. Perhaps the era now is the most successful era of the " +
						"gaming industry, but players can not find the feeling of childhood anymore.";
				else if (system.RomFolderName?.ToLower() == "n64")
					system.Description = "With 32.93 million devices sold, Nintendo 64 (or N64) is also considered one of the most successful devices in the Nintendo system. Called upon production, " + "" +
						"“Project Reality”, although the production process was completed in 1995, it was not until the end of 1996 that the new console was released. And indeed, the name “Machine of " +
						"the Year” at that time that Time.com magazine dedicated is totally worth it. Nintendo 64 ROMs have about 388 games officially released, which is a modest figure for other Nintendo " +
						"consoles. Some famous ROMs of N64 is The Legend Of Zelda: Ocarina Of Time Collector’s Edition, Super Smash Bros., Mario Kart 64, Pokémon Stadium, Mario Party, Paper Mario, " +
						"Donkey Kong 64… In hardware, Nintendo 64 is also considered a monster of its time, with a 640 × 480 pixel screen that displays 16.8 million colors along with NEC VR4300 CPU " +
						"and 8MB RAM, N64 can run game disks with a capacity of 4-64MB at the time, an extremely impressive parameter.";
				else if (system.RomFolderName?.ToLower() == "psp")
					system.Description = "Introduced by Sony in E3 2003, the PlayStation Portable became a highlight throughout the year and soon after its release in late 2004, it became the most powerful gaming " +
						"device of its time. When compared with GBA, the PSP is quite similar in terms of launch time and sales, but in terms of hardware configuration, the PSP proved to be much stronger when it was " +
						"said to be as strong as the PS1 and could run games up to several GB in size. Not only a pure gaming device, but PSP is also considered a mobile entertainment device when it provides " +
						"a variety of powerful entertainment applications such as web browser, youtube, a media player. With a large external speaker and a beautiful display, many people at that time used the PSP " +
						"to replace the inconvenience PC. The PSP is a handheld gaming device that should be fairly basic in size, 17cm in length, 7.4cm wide, 2.3cm thick and weighs 280 grams (including the " +
						"battery). This is an ideal size from that day until now for a handheld gaming device. Sony is also very fond of equipping its pet with a large 4.3 inch LCD screen with 16.77 million " +
						"colours that can be displayed. The resolution of 480×272 is quite meagre compared to now but with 10 years ago, it was the best screen resolution that a compact gaming device could carry " +
						"in itself. However, with the LCD background, you can play for hours without eye strain. That’s why until now, PSP devices are still very popular with users because of its portability. " +
						"Unfortunately, SONY is no longer focusing on developing their handheld gaming devices. Instead, heavy gaming machines like the PS4 and PS5 are about to be released. Although equipped with " +
						"entertainment capabilities, the PSP still only equipped with the buttons of a basic gaming device. It is two keys L and R, the key buttons of the PlayStation system (triangle, square, " +
						"round and X) and an Analogue-stick. So you can control everything easily. And of course, because it is a touch screen, users can touch anywhere on that LCD for faster access. Swipe gesture " +
						"is quite similar to smartphones from now. The battery life of the PlayStation Portable battery is quite small compared to other systems from Nintendo, with nearly 2000mAh battery, you " +
						"can play games continuously for 3 hours with the network connection turned on. If you’re only listening to music and watching movies, its battery can last up to 24 hours.";
			}
		}

		

		

		
	}
}
