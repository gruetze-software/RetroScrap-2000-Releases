using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

public sealed class SmartListViewComparer : IComparer
{
	private readonly int _col;
	private readonly SortOrder _order;
	private readonly StringComparer _textComparer;

	public SmartListViewComparer(int column, SortOrder order)
	{
		_col = column;
		_order = order;
		_textComparer = StringComparer.CurrentCultureIgnoreCase;
	}

	public int Compare(object? x, object? y)
	{
		var aItem = (ListViewItem)x!;
		var bItem = (ListViewItem)y!;
		string a = GetText(aItem, _col);
		string b = GetText(bItem, _col);

		// Leere ans Ende
		bool aEmpty = string.IsNullOrWhiteSpace(a);
		bool bEmpty = string.IsNullOrWhiteSpace(b);
		if (aEmpty && bEmpty) return 0;
		if (aEmpty) return _order == SortOrder.Ascending ? 1 : -1;
		if (bEmpty) return _order == SortOrder.Ascending ? -1 : 1;

		// 1) Datum (mehrere Formate)
		if (TryParseDate(a, out var da) && TryParseDate(b, out var db))
			return ApplyOrder(da.CompareTo(db));

		// 2) Zahl (inkl. %; sowohl Komma als auch Punkt)
		if (TryParseNumber(a, out var na) && TryParseNumber(b, out var nb))
			return ApplyOrder(na.CompareTo(nb));

		// 3) Bool („Ja/Nein“, „Yes/No“, „true/false“)
		if (TryParseBool(a, out var ba) && TryParseBool(b, out var bb))
			return ApplyOrder(ba.CompareTo(bb));

		// 4) Text
		return ApplyOrder(_textComparer.Compare(a, b));
	}

	private int ApplyOrder(int cmp) =>
			_order == SortOrder.Descending ? -cmp : cmp;

	private static string GetText(ListViewItem it, int col) =>
			col < it.SubItems.Count ? it.SubItems[col].Text ?? "" : "";

	private static bool TryParseDate(string s, out DateTime dt)
	{
		s = s.Trim();
		// häufige Formate: ISO, yyyy-MM-dd, dd.MM.yyyy, dd/MM/yyyy, releasedate aus XML „yyyyMMdd“
		if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt)) return true;

		var fmts = new[]
		{
						"yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy", "MM/dd/yyyy",
						"yyyyMMdd", "yyyyMMdd'T'HHmmss", "yyyy-MM-dd HH:mm",
						"dd.MM.yyyy HH:mm", "yyyy/MM/dd"
				};
		return DateTime.TryParseExact(s, fmts, CultureInfo.InvariantCulture,
																	DateTimeStyles.None, out dt);
	}

	private static bool TryParseNumber(string s, out double d)
	{
		s = s.Trim();

		// Prozent abschneiden
		if (s.EndsWith("%", StringComparison.Ordinal))
			s = s[..^1];

		// Erst aktuelle Kultur versuchen (Komma als Dezimaltrenner)
		if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
												CultureInfo.CurrentCulture, out d))
			return true;

		// Dann invariant (Punkt als Dezimaltrenner)
		if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
												CultureInfo.InvariantCulture, out d))
			return true;

		// Notfall: Punkt -> Komma ersetzen
		var alt = s.Replace('.', ',');
		return double.TryParse(alt, NumberStyles.Float | NumberStyles.AllowThousands,
													 CultureInfo.CurrentCulture, out d);
	}

	private static bool TryParseBool(string s, out bool b)
	{
		s = s.Trim().ToLowerInvariant();
		if (s is "ja" or "yes" or "true") { b = true; return true; }
		if (s is "nein" or "no" or "false") { b = false; return true; }
		b = false; return false;
	}
}
