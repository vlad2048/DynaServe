using System.Reactive;
using DynaServeLib.DynaLogic.Refreshers;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class HtmlNodeWrapExt
{
	public static HtmlNode WrapSmart<T>(
		this HtmlNode node,
		IObservable<T> when,
		Func<IEnumerable<HtmlNode>> fun
	)
	{
		HtmlNode[] MkAndCheckDiffIdsExist()
		{
			var arr = fun().ToArray();
			if (arr.Any(e => e.DiffId == null)) throw new ArgumentException("When using WrapSmart, you need to set DiffIds on the children");
			return arr;
		}
		node.AddRefresher(new DiffRefresher(node.Id, when.ToUnit(), MkAndCheckDiffIdsExist));
		return node;
	}

	public static HtmlNode Wrap(this HtmlNode node, IEnumerable<HtmlNode> children) =>
		node.Wrap(children.ToArray());

	public static HtmlNode Wrap(this HtmlNode node, params HtmlNode[] children)
	{
		node.Children = children;
		return node;
	}

	public static HtmlNode Wrap(this HtmlNode node, IObservable<Unit> when, Func<IEnumerable<HtmlNode>> fun)
	{
		node.AddRefresher(new ChildRefresher(node.Id, when, () => fun().ToArray()));
		return node;
	}

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