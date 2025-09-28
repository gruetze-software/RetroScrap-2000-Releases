using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public sealed class DeveloperVault
	{
		private const char Separator = '|';
		private const string _file = "retroscrap.bin";
		

		public DeveloperVault()
		{
		}

		private static byte[] HexToBytes(string hex)
		{
			if (string.IsNullOrEmpty(hex))
			{
				return new byte[0];
			}

			// Die Länge des Hex-Strings muss gerade sein
			if (hex.Length % 2 != 0)
			{
				throw new ArgumentException("Hex-String hat eine ungerade Länge.");
			}

			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				// Parsen von jeweils zwei Zeichen als Byte (Basis 16)
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}
			return bytes;
		}

		public bool TryLoad(out string? devId, out string? devPwd)
		{
			devId = null;
			devPwd = null;
			string filepath = Path.Combine(AppContext.BaseDirectory, "Config", _file);
			if (!File.Exists(filepath)) 
				return false;

			try
			{
				var blob = File.ReadAllBytes(filepath);

				// Entschlüsseln mit AES
				using (Aes aesAlg = Aes.Create())
				{
					aesAlg.Key = HexToBytes(Properties.Resources.RetroK);
					aesAlg.IV = HexToBytes(Properties.Resources.RetroIV);

					ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

					using (MemoryStream msDecrypt = new MemoryStream(blob))
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					using (StreamReader srDecrypt = new StreamReader(csDecrypt))
					{
						string combinedString = srDecrypt.ReadToEnd();
						string[] parts = combinedString.Split(Separator);

						if (parts.Length == 2)
						{
							devId = parts[0];
							devPwd = parts[1];
							return true;
						}
						return false;
					}
				}
			}
			catch (CryptographicException)
			{
				// Fehler beim Entschlüsseln (falscher Schlüssel oder falsche Daten)
				return false;
			}
			catch
			{
				return false;
			}
		}

		public void Save(string devId, string devPwd)
		{
			string combinedString = devId + Separator + devPwd;
			var bytes = Encoding.UTF8.GetBytes(combinedString);
			string filepath = Path.Combine(AppContext.BaseDirectory, "Config", _file);
			Directory.CreateDirectory(Path.GetDirectoryName(filepath)!);

			// Verschlüsseln mit AES
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = HexToBytes(Properties.Resources.RetroK);
				aesAlg.IV = HexToBytes(Properties.Resources.RetroIV);

				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						csEncrypt.Write(bytes, 0, bytes.Length);
					}
					File.WriteAllBytes(filepath, msEncrypt.ToArray());
				}
			}
		}
	}
}
