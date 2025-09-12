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

		public int? progressPerc;
		public string? progressText { get; set; }
		public string? MessageText { get; set; }
		public ProgressObj() 
		{
			progressPerc = -1;
			progressText = string.Empty;
			MessageText = "";
		}

		public ProgressObj(int perc, string progText) : this()
		{
			progressPerc = perc;
			progressText = progText;
		}

		public ProgressObj(int perc, string progText, string messageText) : this(perc, progText)
		{
			MessageText = messageText;	
		}

		public ProgressObj(eTyp typ, string message) : this()
		{
			Typ = typ;
			MessageText = message;
		}

		public override string ToString()
		{
			return $"{Typ}: {progressText} {MessageText}";
		}
	}
}
