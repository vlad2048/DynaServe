using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Utils;

static class HtmlNodeInternalExt
{
	public static HtmlNode AutofocusIfFirst(this HtmlNode node, bool isFirst) => isFirst switch
	{
		true => node.Attr("autofocus", ""),
		false => node
	};
}