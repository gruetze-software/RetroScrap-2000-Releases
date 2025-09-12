using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class ListViewSortIcons
{
	private const int LVM_GETHEADER = 0x1000 + 31;
	private const int HDM_GETITEM = 0x120B;
	private const int HDM_SETITEM = 0x120C;

	private const int HDF_LEFT = 0x0000;
	private const int HDF_STRING = 0x4000;
	private const int HDF_SORTUP = 0x0400;
	private const int HDF_SORTDOWN = 0x0200;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct HDITEM
	{
		public uint mask;
		public int cxy;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszText;
		public IntPtr hbm;
		public int cchTextMax;
		public int fmt;
		public IntPtr lParam;
		public int iImage;
		public IntPtr pvFilter;
		public uint state;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	public static void SetSortIcon(ListView listView, int columnIndex, SortOrder order)
	{
		IntPtr hHeader = SendMessage(listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
		for (int i = 0; i < listView.Columns.Count; i++)
		{
			var item = new HDITEM { mask = 0x0004, fmt = HDF_STRING | HDF_LEFT }; // HDI_FORMAT=0x0004
			IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(item));
			Marshal.StructureToPtr(item, ptr, false);
			SendMessage(hHeader, HDM_GETITEM, (IntPtr)i, ptr);
			item = (HDITEM)Marshal.PtrToStructure(ptr, typeof(HDITEM))!;
			if (order != SortOrder.None && i == columnIndex)
			{
				if (order == SortOrder.Ascending)
					item.fmt |= HDF_SORTUP;
				else
					item.fmt |= HDF_SORTDOWN;
			}
			else
			{
				item.fmt &= ~HDF_SORTDOWN & ~HDF_SORTUP;
			}
			Marshal.StructureToPtr(item, ptr, true);
			SendMessage(hHeader, HDM_SETITEM, (IntPtr)i, ptr);
			Marshal.FreeHGlobal(ptr);
		}
	}
}
