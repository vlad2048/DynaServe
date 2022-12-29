using System.Reactive;
using DynaServeLib.DynaLogic.Refreshers;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class HtmlNodeWrapExt
{
	// **********
	// * Static *
	// **********
	public static HtmlNode Wrap(this HtmlNode node, params HtmlNode[] children)
	{
		node.Children = children;
		return node;
	}

	// ***********
	// * Dynamic *
	// ***********
	public static HtmlNode Wrap(this HtmlNode node, IObservable<Unit> when, Func<IEnumerable<HtmlNode>> fun)
	{
		node.AddRefresher(new ChildrenRefresher(node.Id, when, () => fun().ToArray()));
		return node;
	}

	// *********
	// * Sugar *
	// *********
	public static HtmlNode Wrap(this HtmlNode node, IEnumerable<HtmlNode> children) =>
		node.Wrap(children.ToArray());

	public static HtmlNode Wrap(this HtmlNode node, IRoVar<Maybe<HtmlNode>> mayNodeVar) =>
		node.Wrap(
			mayNodeVar.ToUnit(),
			() => mayNodeVar.V.IsSome(out var childode) switch
			{
				true => new[] { childode! },
				false => Array.Empty<HtmlNode>()
			}
		);

	public static HtmlNode Wrap(this HtmlNode node, IRoVar<IEnumerable<HtmlNode>> nodeArrVar) =>
		node.Wrap(
			nodeArrVar.ToUnit(),
			() => nodeArrVar.V
		);
}