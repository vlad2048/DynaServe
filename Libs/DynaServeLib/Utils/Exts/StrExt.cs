using System.Text;

namespace DynaServeLib.Utils.Exts;

public static class StrExt
{
	public static string[] ToLines(this string s) => s.Split(Environment.NewLine);
	public static string FromLines(this IEnumerable<string> ls) => string.Join(Environment.NewLine, ls);
	public static string FromBytes(this byte[] data) => Encoding.UTF8.GetString(data);
	public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);

	public static string AddPrefixToLines(this string str, string prefix) =>
		str
			.ToLines()
			.Select(e => $"{prefix}{e}")
			.FromLines();

	internal static int GetLongestLineLength(this string? str) => str switch
	{
		null => 0,
		not null => str
			.Replace("\t", "    ")
			.Split('\r', '\n')
			.MaxOrZero(e => e.Length)
	};

	internal static string ApplySubsts(this string str, (string, string)[] substs)
	{
		foreach (var t in substs)
			str = str.Replace(t.Item1, t.Item2);
		return str;
	}
	
	private static int MaxOrZero<T>(this IEnumerable<T> source, Func<T, int> fun)
	{
		var max = 0;
		foreach (var elt in source)
		{
			var val = fun(elt);
			if (val > max)
				max = val;
		}
		return max;
	}
}