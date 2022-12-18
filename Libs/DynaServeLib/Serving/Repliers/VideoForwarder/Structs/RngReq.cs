using System.Net.Http.Headers;

namespace DynaServeLib.Serving.Repliers.VideoForwarder.Structs;

public class RngReq
{
	private const string Prefix = "bytes=";

	public const string HeaderName = "range";

	public long Start { get; }
	public long? End { get; }

	private RngReq(long start, long? end)
	{
		Start = start;
		End = end;
	}

	public override string ToString() => $"{Prefix}{Start}-" + End switch
	{
		not null => $"{End}",
		null => string.Empty
	};

	public static RngReq Parse(string str)
	{
		if (!str.StartsWith(Prefix)) throw new ArgumentException();
		str = str[Prefix.Length..];
		var parts = str.Split('-');
		if (parts.Length != 2) throw new ArgumentException();
		if (!long.TryParse(parts[0], out var start)) throw new ArgumentException();
		var end = long.TryParse(parts[1], out var endVal) ? (long?)endVal : null;
		return new RngReq(start, end);
	}
	
	public void WriteToRequest(HttpRequestMessage req)
	{
		//req.Headers.Add(HeaderName, $"{this}");
		req.Headers.Range = new RangeHeaderValue(Start, End);
	}

	public bool CheckFit(long length) =>
		Start >= 0 &&
		Start < length &&
		(
			End == null ||
			(
				End >= 0 &&
				End < length &&
				End > Start
			)
		);
}
