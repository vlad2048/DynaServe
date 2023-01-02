using System.Reactive.Linq;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Utils.Exts;
using System.Runtime.CompilerServices;

namespace DynaServeLib.Nodes;

public static class HtmlNodeAttrStyleExt
{
	// **********
	// * Static *
	// **********
	public static HtmlNode Attr(
		this HtmlNode node,
		string attrName,
		string? attrVal
	)
	{
		if (attrVal == null)
			node.Attrs.Remove(attrName);
		else
			node.Attrs[attrName] = attrVal;
		return node;
	}

	
	// ***********
	// * Dynamic *
	// ***********
	public static HtmlNode Attr(
		this HtmlNode node,
		string attrName,
		IObservable<string?> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	)
	{
		node.AddRefresher(PropChangeRefresher.MkAttr(node.Id, attrName, valObs.ThrowIf_Observable_IsNot_Derived_From_RxVar(valObsName)));
		return node;
	}


	// ****************
	// * Sugar (base) *
	// ****************
	public static HtmlNode AttrSetWhenNot(
		this HtmlNode node,
		string attrName,
		string attrVal,
		IObservable<bool> whenCondition,
		[CallerArgumentExpression(nameof(whenCondition))] string? whenConditionName = null
	)
	{
		var attrValObs = whenCondition
			.ThrowIf_Observable_IsNot_Derived_From_RxVar(whenConditionName)
			.Select(condition => condition switch
			{
				true => null,
				false => attrVal
			});
		return node.Attr(attrName, attrValObs, whenConditionName);
	}


	// *********
	// * Sugar *
	// *********
	public static HtmlNode VisibleWhen(
		this HtmlNode node,
		IObservable<bool> whenVisible,
		[CallerArgumentExpression(nameof(whenVisible))] string? whenVisibleName = null
	) =>
		node.AttrSetWhenNot("style", "display: none", whenVisible, whenVisibleName);

	public static HtmlNode EnableWhen(
		this HtmlNode node,
		IObservable<bool> whenEnabled,
		[CallerArgumentExpression(nameof(whenEnabled))] string? whenEnabledName = null
	) =>
		node.AttrSetWhenNot("disabled", "", whenEnabled, whenEnabledName);
}