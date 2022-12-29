using DynaServeLib.DynaLogic.Refreshers;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class HtmlNodeExt
{
	public static HtmlNode Id(this HtmlNode node, string id)
	{
		node.Id = id;
		return node;
	}

	public static HtmlNode Cls(this HtmlNode node, string? cls)
	{
		node.Cls = cls;
		return node;
	}

	public static HtmlNode Cls(this HtmlNode node, IRoVar<string?> clsVar)
	{
		node.Cls = clsVar.V;
		node.AddRefresher(new ClsRefresher(node.Id, clsVar));
		return node;
	}

	public static HtmlNode Txt(this HtmlNode node, string? txt)
	{
		node.Txt = txt;
		return node;
	}

	public static HtmlNode Txt(this HtmlNode node, IRoVar<string?> txt) =>
		node.Wrap(
			txt.ToUnit(),
			() => new []
			{
				new HtmlNode("div").Txt(txt.V)
			}
		);

	public static HtmlNode Ref(this HtmlNode node, Ref @ref)
	{
		@ref.Hook(node);
		return node;
	}
}