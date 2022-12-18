using System.Net;

namespace DynaServeLib.Serving.Repliers.VideoForwarder.Structs;

public class RngRes
{
	private const string Prefix = "bytes ";

	public const string HeaderName = "Content-Range";

	public long Start { get; }
	public long End { get; }
	public long Length { get; }

	public RngRes(long start, long end, long length)
	{
		Start = start;
		End = end;
		Length = length;
	}

	public override string ToString() => $"{Prefix}{Start}-{End}/{Length}";

	public static RngRes Parse(string str)
	{
		if (!str.StartsWith(Prefix)) throw new ArgumentException();
		str = str[Prefix.Length..];
		var parts = str.Split('-', '/');
		if (parts.Length != 3) throw new ArgumentException();
		if (
			!long.TryParse(parts[0], out var start) ||
			!long.TryParse(parts[1], out var end) ||
			!long.TryParse(parts[2], out var length)
		) throw new ArgumentException();
		return new RngRes(start, end, length);
	}

	public void WriteToResponse(HttpListenerResponse res)
	{
		var lng = End - Start + 1;
		res.AddHeader(HeaderName, $"{this}");
		res.AddHeader("Content-Length", $"{lng}");
		res.ContentLength64 = lng;
		res.StatusCode = 206;
	}
}