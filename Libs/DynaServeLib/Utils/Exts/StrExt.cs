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
}