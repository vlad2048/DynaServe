using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace DynaServeLib.DynaLogic.Utils;

/*
In Chrome:
	http://box-pc:7000/css/websockets.css
	(or file:///C:/Dev/Creepy/_infos/js-play/css/pane-scroller.css)

UrlUtils.GetLocalLink(opt.Port) = http://box-pc:7000/

But displayed as (in the dev tools):
	css/websockets.css?c=3

In AngleSharp:
	http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900
	about:///css/websockets.css?c=3
*/
// TODO: code duplication with DomUtils.cs
static class CssLinkUtils
{
	private const string Prefix = "about:///";

	public static IEnumerable<IHtmlLinkElement> FilterCssLinks(this IEnumerable<IElement> source) =>
		source
			.OfType<IHtmlLinkElement>()
			.Where(e => e.Relation == "stylesheet")
			.Where(e => e.Href != null)
			.Where(e => e.Href!.StartsWith(Prefix));

	public static IEnumerable<IHtmlScriptElement> FilterJsLinks(this IEnumerable<IElement> source) =>
		source
			.OfType<IHtmlScriptElement>();

	public static string CssNorm(this string s) => s.StartsWith(Prefix) switch
	{
		true => s[Prefix.Length..],
		false => s
	};

	public static string CssInc(this string s)
	{
		var (l, c) = s
			.CssNorm()
			.CssDemake();
		var cn = c.HasValue switch
		{
			false => 1,
			true => c!.Value + 1,
		};
		return CssMake(l, cn);
	}

	public static (string, int?) CssDemake(this string s)
	{
		var idx = s.IndexOf('?');
		if (idx == -1) return (s, null);
		var parts = s.Split('=');
		if (parts.Length != 2) throw new ArgumentException();
		var c = int.Parse(parts[1]);
		return (s[..idx], c);
	}

	public static string CssMake(string s, int c) => $"{s}?c={c}";
}