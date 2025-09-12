using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public sealed class PasswordVault
	{
		private readonly string _filePath;
		// Etwas "Entropy", damit nur meine App die Datei wieder entschlüsselt
		private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("RetroScrap2000-{9D60D5F3-5E7B-4E0A-A2C6-3C0F5B4A8C2F}");

		public PasswordVault(string filePath)
		{
			_filePath = filePath;
		}

		public bool TryLoad(out string? password)
		{
			password = null;
			if (!File.Exists(_filePath)) return false;

			try
			{
				var blob = File.ReadAllBytes(_filePath);
				var unprotected = ProtectedData.Unprotect(blob, _entropy, DataProtectionScope.CurrentUser);
				password = Encoding.UTF8.GetString(unprotected);
				return true;
			}
			catch
			{
				return false; // Datei korrupt/anderer Benutzer etc.
			}
		}

		public void Save(string password)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
			var bytes = Encoding.UTF8.GetBytes(password);
			var protectedBytes = ProtectedData.Protect(bytes, _entropy, DataProtectionScope.CurrentUser);
			File.WriteAllBytes(_filePath, protectedBytes);
		}

		public void Delete()
		{
			if (File.Exists(_filePath)) File.Delete(_filePath);
		}
	}
}
