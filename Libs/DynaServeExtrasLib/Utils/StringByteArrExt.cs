using System.Text;

namespace DynaServeExtrasLib.Utils;

static class StringByteArrExt
{
	public static string FromBytes(this byte[] data) => Encoding.UTF8.GetString(data);
	public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);

	//public static string[] ToLines(this string s) => s.Split(Environment.NewLine);
	//public static string FromLines(this IEnumerable<string> ls) => string.Join(Environment.NewLine, ls);
}