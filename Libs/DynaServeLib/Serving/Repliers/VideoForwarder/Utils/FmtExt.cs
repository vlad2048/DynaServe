namespace DynaServeLib.Serving.Repliers.VideoForwarder.Utils;

public static class FmtExt
{
	public static string FmtSize(this long v)
	{
		var kb = v / 1024.0;
		return $"{kb:n}kb";
	}

	public static string FmtSize(this int v)
	{
		var kb = v / 1024.0;
		return $"{kb:n}kb";
	}
}