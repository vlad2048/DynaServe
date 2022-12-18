using PowMaybe;
using System.Reactive.Linq;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class HtmlNodeAttrStyleExt
{
	public static HtmlNode Attr(this HtmlNode node, string attrName, string? attrVal)
	{
		if (attrVal == null)
			node.Attrs.Remove(attrName);
		else
			node.Attrs[attrName] = attrVal;
		return node;
	}


	public static HtmlNode Attr(this HtmlNode node, string attrName, IObservable<string?> valObs)
	{
		node.UpdateAttrWhen(attrName, valObs);
		SetInitVal(v => node.Attr(attrName, v), valObs);
		return node;
	}

	public static HtmlNode AttrSetWhenNot(this HtmlNode node, string attrName, string attrVal, IObservable<bool> whenCondition)
	{
		var updateObs = whenCondition.Select(condition => condition switch
		{
			true => null,
			false => attrVal
		});

		node.UpdateAttrWhen(attrName, updateObs);
		SetInitVal(v => node.Attr(attrName, v), updateObs);
		return node;
	}

	public static HtmlNode VisibleWhen(this HtmlNode node, IObservable<bool> whenVisible) =>
		node.AttrSetWhenNot("style", "display: none", whenVisible);

	public static HtmlNode EnableWhen(this HtmlNode node, IObservable<bool> whenEnabled) =>
		node.AttrSetWhenNot("disabled", "true", whenEnabled);


	private static void SetInitVal<T>(Action<T> setFun, IObservable<T> obs)
	{
		var initVal = May.None<T>();
		using var _ = obs.Take(1).Subscribe(v => initVal = May.Some(v));
		if (initVal.IsSome(out var init))
			setFun(init);
	}
}