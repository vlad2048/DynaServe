using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Utils;

public static class HtmlNodeArrExt
{
	public static HtmlNode[] AddNodeIf(this HtmlNode node, HtmlNode nodeExtra, bool cond) => new [] { node }.AddNodeIf(nodeExtra, cond);
	
	public static HtmlNode[] AddNodeIf(this HtmlNode[] nodes, HtmlNode nodeExtra, bool cond) => cond switch
	{
		false => nodes,
		true => nodes.Append(nodeExtra).ToArray()
	};
}