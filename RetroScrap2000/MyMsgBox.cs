using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public class MyMsgBox
	{
		private static AppInfo? _appinfo;

		private static string GetTitle()
		{
			if (_appinfo == null)
				_appinfo = Utils.GetAppInfo();

			return _appinfo.Value.ProductName + " - " + _appinfo.Value.ProductVersion;
		}

		public static void Show(string msg)
		{
			MessageBox.Show(msg, GetTitle(), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static void ShowErr(string msg)
		{
			MessageBox.Show(msg, GetTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static void ShowWarn(string msg)
		{
			MessageBox.Show(msg, GetTitle(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static DialogResult ShowQuestion(string msg)
		{
			return MessageBox.Show(msg, GetTitle(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
		}
	}
}
