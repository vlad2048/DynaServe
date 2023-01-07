using DynaServeLib.Utils;

namespace DynaServeLib.DynaLogic.DomUtils;


//	In Chrome:
//		http://box-pc:7000/css/websockets.css
//		(or file:///C:/Dev/Creepy/_infos/js-play/css/pane-scroller.css)
//
//	UrlUtils.GetLocalLink(opt.Port) = http://box-pc:7000/
//
//	But displayed as (in the dev tools):
//		css/websockets.css?c=3
//
//	In AngleSharp:
//		http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900
//		about:///css/websockets.css?c=3

static class DomUrlUtils
{
	private const string Prefix = "about:///";

	public static bool IsLinkRelevant(this string? link) => link != null && link.StartsWith(Prefix);

	public static string GetRelevantLinkEnsure(this string? link)
	{
		if (!link.IsLinkRelevant()) throw new ArgumentException();
		return link![Prefix.Length..];
	}

	public static bool IsSameAsWithoutQueryParams(this string l1, string l2) =>
		l1.RemoveQueryParams() == l2.RemoveQueryParams();

	public static string BumpQueryParamCounter(this string s) => s
		.VerDemake()
		.VerInc()
		.VerMake();


	private static (string, int?) VerDemake(this string s)
	{
		var idx = s.IndexOf('?');
		if (idx == -1) return (s, null);
		var parts = s.Split('=');
		if (parts.Length != 2) throw new ArgumentException();
		var c = int.Parse(parts[1]);
		return (s[..idx], c);
	}
	private static (string, int) VerInc(this (string, int?) t) => (
		t.Item1,
		t.Item2.HasValue switch
		{
			false => 1,
			true => t.Item2!.Value + 1
		}
	);

	private static string VerMake(this (string, int) t) => $"{t.Item1}?c={t.Item2}";
}