using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class MyMsgBox
	{
		public static void Show(string msg)
		{
			MessageBox.Show(msg, "RetroScrap2000", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static void ShowErr(string msg)
		{
			MessageBox.Show(msg, "RetroScrap2000", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ShowWarn(string msg)
		{
			MessageBox.Show(msg, "RetroScrap2000", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static DialogResult ShowQuestion(string msg)
		{
			return MessageBox.Show(msg, "RetroScrap2000", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
		}
	}
}
