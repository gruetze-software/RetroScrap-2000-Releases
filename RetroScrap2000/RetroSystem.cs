using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RetroScrap2000
{
	public class RetroSystem
	{
		public int Id { get; set; } = -1;
		public string Name { get; set; } = "Unknown System";
		public string? RomFolderName { get; set; }
		public string? Hersteller { get; set; }
		public string? Typ { get; set; }
		public string? Description { get; set; }
		public int Debut { get; set; } = 0;
		public int Ende { get; set; } = 0;
		public string? FileIcon { get; set; }
		public string? FileBanner { get; set; }

		public RetroSystem() { }

		public override string ToString()
		{
			return Name;
		}
	}

	public class RetroSystems
	{
		public List<RetroSystem> SystemList { get; set; } = new List<RetroSystem>();

		public static string FolderBanner = Path.Combine(AppContext.BaseDirectory, "Resources", "System_Banner");
		public static string FolderIcons = Path.Combine(AppContext.BaseDirectory, "Resources", "System_Icons");
		public RetroSystems() 
		{ 
		}

		public string GetRomFolder(int systemid)
		{
			var sys = SystemList.FirstOrDefault(x => x.Id == systemid);
			if (sys == null)
			{
				Trace.WriteLine("[RetroSystem::GetRomFolder] skip Id " + systemid.ToString());
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
						Name = !string.IsNullOrEmpty(s.Name) ? s.Name : "Unknown System",
						Typ = s.type,
						RomFolderName = BatoceraFolders.MapToBatoceraFolder(s.noms)
					};

					if (!string.IsNullOrEmpty(retroSystem.RomFolderName))
					{
						FileInfo ico = new FileInfo(Path.Combine(FolderIcons, retroSystem.RomFolderName + ".png"));
						FileInfo banner = new FileInfo(Path.Combine(FolderBanner, retroSystem.RomFolderName + ".png"));

						if (ico.Exists)
							retroSystem.FileIcon = ico.FullName;
						if (banner.Exists)
							retroSystem.FileBanner = banner.FullName;
					}

					SystemList.Add(retroSystem);
				}
			}
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
		public static RetroSystems Load()
		{
			if (!File.Exists(GetRetroSystemsFile()))
				return new RetroSystems();

			var json = File.ReadAllText(GetRetroSystemsFile());
			return JsonSerializer.Deserialize<RetroSystems>(json) ?? new RetroSystems();
		}

		private static readonly object _xmlFileLock = new(); // primitive Sperre pro Prozess

		

		public bool SaveRomToGamelistXml(string romRoot, string systemFolder, GameEntry rom)
		{
			// Pfade
			var sysDir = Path.Combine(romRoot, systemFolder);
			var xmlPath = Path.Combine(sysDir, "gamelist.xml");
			var backupPath = xmlPath + ".bak";

			// relative <path> bestimmen – Primärschlüssel
			var relPath = GetRomPathForXml(sysDir, rom);

			lock (_xmlFileLock)
			{
				// Backup (nur wenn Datei existiert)
				if (File.Exists(xmlPath))
				{
					try { File.Copy(xmlPath, backupPath, overwrite: true); }
					catch { /* egal – Speicher nicht abbrechen */ }
				}

				// Dokument laden oder neu erstellen
				XDocument doc;
				if (File.Exists(xmlPath))
				{
					doc = XDocument.Load(xmlPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
				}
				else
				{
					doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
									new XElement("gameList"));
				}

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

				SetRomToXml(gameEl, relPath, sysDir, rom);

				try
				{
					var xmlWriterSettings = new System.Xml.XmlWriterSettings
					{
						Indent = true,
						IndentChars = "  ", // Normales Leerzeichen verwenden
						Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
						NewLineChars = Environment.NewLine,
						NewLineHandling = System.Xml.NewLineHandling.Replace,
						OmitXmlDeclaration = false
					};
					using (var w = System.Xml.XmlWriter.Create(xmlPath, xmlWriterSettings))
						doc.Save(w);

					return true;
				}
				catch
				{
					throw; // aufrufende Funktion fängt das ab
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

				try
				{
					var xmlWriterSettings = new System.Xml.XmlWriterSettings
					{
						Indent = true,
						IndentChars = "  ", // Normales Leerzeichen verwenden
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
					Trace.WriteLine(Utils.GetExcMsg(ex));
					return false;
				}
			}
		}

		private static void SetRomToXml(XElement gameEl, string relPath, string sysDir, GameEntry rom)
		{
			Trace.WriteLine("[RetroSystem::SetRomToXml] Adding rom " + relPath);
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
			SetElementValue(gameEl, "image", EnsureRelativeMedia(sysDir, rom.MediaScreenshotPath));
			SetElementValue(gameEl, "thumbnail", EnsureRelativeMedia(sysDir, rom.MediaCoverPath));
			SetElementValue(gameEl, "video", EnsureRelativeMedia(sysDir, rom.MediaVideoPath));

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
}
