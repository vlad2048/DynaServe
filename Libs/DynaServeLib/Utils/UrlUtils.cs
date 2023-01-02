using System.Net;

namespace DynaServeLib.Utils;

static class UrlUtils
{
	public static string GetLocalLink(int port) =>
		$"http://{Environment.MachineName.ToLowerInvariant()}:{port}/";

	public static string GetWSLink(int port) =>
		$"ws://{Environment.MachineName.ToLowerInvariant()}:{port}/";

	public static string GetUrl(this HttpListenerRequest req)
	{
		if (req.Url == null) throw new ArgumentException();
		return req.Url.PathAndQuery.RemoveLeadingSlash();
	}

	private static string RemoveLeadingSlash(this string s) => (s[0] == '/') switch
	{
		true => s[1..],
		false => s
	};

	public static string RemoveQueryParams(this string s)
	{
		var idx = s.IndexOf('?');
		return (idx != -1) switch
		{
			true => s[..idx],
			false => s
		};
	}
}