using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class ProgressObj
	{
		public enum eTyp : byte
		{
			Info= 0,
			Warning,
			Error
		};

		public eTyp Typ { get; set; } = eTyp.Info;

		public int ProgressPerc { get; set; }
		public string RomName { get; set; }
		public string? RomNumberAndTotal { get; set; }
		public string? MessageText { get; set; }
		public ProgressObj() 
		{
			Typ = eTyp.Info;
			ProgressPerc = -1;
			RomName = string.Empty;
			RomNumberAndTotal = string.Empty;
			MessageText = "";
		}

		public ProgressObj(eTyp typ, int perc, string messageText) : this()
		{
			Typ = typ;
			ProgressPerc = perc;
			MessageText = messageText;
		}

		public ProgressObj(int progressPerc, string messageText) : this(
			eTyp.Info, progressPerc, messageText)
		{	}

		public ProgressObj(int progressPerc, int romtotal, int romnr,
			string romName, string messageText) : this(progressPerc, messageText)
		{
			RomNumberAndTotal = $"({romnr}/{romtotal})";
			RomName = romName;
		}
		

		public override string ToString()
		{
			return $"{Typ}: {MessageText}";
		}
	}
}
