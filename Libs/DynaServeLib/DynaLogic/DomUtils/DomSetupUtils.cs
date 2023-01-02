using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace DynaServeLib.DynaLogic.DomUtils;

static class DomSetupUtils
{
	public static void AddExtraHtmlNodes(
		this IHtmlDocument dom,
		IReadOnlyList<string> extraHtmlNodes
	)
	{
		var body = dom.FindDescendant<IHtmlBodyElement>()!;
		var parser = new HtmlParser();
		foreach (var html in extraHtmlNodes)
		{
			var nodes = parser.ParseFragment(html, body);
			body.PrependNodes(nodes.ToArray());
		}
	}
}