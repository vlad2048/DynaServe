using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Utils.Exts;
using System.Runtime.CompilerServices;

namespace DynaServeLib.Nodes;

public static class HtmlNodeExt
{
	// **********
	// * Static *
	// **********
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

	public static HtmlNode Txt(this HtmlNode node, string? txt)
	{
		node.Txt = txt;
		return node;
	}


	// ***********
	// * Dynamic *
	// ***********
	public static HtmlNode Cls(
		this HtmlNode node,
		IObservable<string?> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	) =>
		node.Attr("class", valObs, valObsName);
	

	// *******
	// * Ref *
	// *******
	public static HtmlNode Ref(this HtmlNode node, Ref @ref)
	{
		@ref.Hook(node);
		return node;
	}
}