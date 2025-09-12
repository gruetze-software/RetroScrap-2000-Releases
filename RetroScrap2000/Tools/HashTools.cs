using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000.Tools
{
	public static class HashTools
	{
		public static string? GetHashCodeFile(string file)
		{
			if (string.IsNullOrWhiteSpace(file))
				return null;
			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(file));
			return Convert.ToHexString(bytes); // AABB…
		}

		/// <summary>
		/// Erzeugt einen format-/metadatenunabhängigen Hash der Bild-Pixel (ARGB, 32bpp).
		/// Gleich, wenn die dekomprimierten Pixel identisch sind.
		/// </summary>
		public static string? GetImagePixelHash(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath)) return null;

			using var src = Image.FromFile(imagePath); // entkoppelt, schließt Datei erst nach Dispose
			using var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bmp))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.DrawImage(src, 0, 0, src.Width, src.Height);
			}

			var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
			var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
			try
			{
				int len = Math.Abs(data.Stride) * data.Height;
				byte[] buffer = new byte[len];
				Marshal.Copy(data.Scan0, buffer, 0, len);

				using var sha = SHA256.Create();
				return Convert.ToHexString(sha.ComputeHash(buffer));
			}
			finally
			{
				bmp.UnlockBits(data);
			}
		}

		/// <summary>
		/// Perceptual Hash (dHash, 64 Bit). Niedrige Hamming-Distanz = visuell ähnlich.
		/// </summary>
		public static ulong GetImageDHash64(string imagePath)
		{
			using var src = Image.FromFile(imagePath);

			// 9x8 skalieren (für 8x8 Differenzen), Graustufen
			using var small = new Bitmap(9, 8, PixelFormat.Format24bppRgb);
			using (var g = Graphics.FromImage(small))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.DrawImage(src, new Rectangle(0, 0, 9, 8));
			}

			ulong hash = 0UL;
			int bit = 0;
			for (int y = 0; y < 8; y++)
			{
				// Zeile als Graustufen lesen
				byte prev = 0;
				for (int x = 0; x < 9; x++)
				{
					var c = small.GetPixel(x, y);
					byte gray = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

					if (x > 0)
					{
						if (prev > gray) hash |= (1UL << bit);
						bit++;
					}
					prev = gray;
				}
			}
			return hash;
		}

		/// <summary>
		/// Hamming-Distanz zwischen zwei 64-Bit-Hashes (Anzahl unterschiedlicher Bits).
		/// </summary>
		public static int HammingDistance(ulong a, ulong b)
		{
			ulong x = a ^ b;
			int d = 0;
			while (x != 0) { x &= x - 1; d++; } // Brian-Kernighan-Trick
			return d;
		}
	}
}
