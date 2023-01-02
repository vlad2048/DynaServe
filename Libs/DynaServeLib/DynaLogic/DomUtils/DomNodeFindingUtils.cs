using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace DynaServeLib.DynaLogic.DomUtils;

static class DomNodeFindingUtils
{
	public static IHtmlLinkElement[] GetAllCssLinkNodes(this IHtmlDocument dom) => dom
		.FindDescendant<IHtmlHeadElement>()!
		.Children
		.OfType<IHtmlLinkElement>()
		.Where(e => e.Relation == "stylesheet")
		.Where(e => e.Href.IsLinkRelevant())
		.ToArray();
}



static class DomUrlUtils
{
	private const string Prefix = "about:///";

	public static bool IsLinkRelevant(this string? link) => link != null && link.StartsWith(Prefix);

	public static string GetRelevantLinkEnsure(this string? link)
	{
		if (!link.IsLinkRelevant()) throw new ArgumentException();
		return link![Prefix.Length..];
	}
}