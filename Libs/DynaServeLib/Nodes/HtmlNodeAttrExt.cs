﻿using System.Reactive.Linq;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Utils.Exts;
using System.Runtime.CompilerServices;
using DynaServeLib.Serving.Syncing.Structs;

namespace DynaServeLib.Nodes;

public static class HtmlNodeAttrExt
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
	public static HtmlNode Txt(
		this HtmlNode node,
		IObservable<string?> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	)
	{
		node.AddRefresher(new ChgRefresher(ChgKeyMk.Text(node.Id), valObs.ThrowIf_Observable_IsNot_Derived_From_RxVar(valObsName)));
		return node;
	}

	public static HtmlNode Attr(
		this HtmlNode node,
		string attrName,
		IObservable<string?> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	)
	{
		node.AddRefresher(new ChgRefresher(ChgKeyMk.Attr(node.Id, attrName), valObs.ThrowIf_Observable_IsNot_Derived_From_RxVar(valObsName)));
		return node;
	}

	public static HtmlNode PropStr(
		this HtmlNode node,
		string propName,
		IObservable<string?> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	)
	{
		node.AddRefresher(new ChgRefresher(ChgKeyMk.PropStr(node.Id, propName), valObs.ThrowIf_Observable_IsNot_Derived_From_RxVar(valObsName)));
		return node;
	}

	public static HtmlNode PropBool(
		this HtmlNode node,
		string propName,
		IObservable<bool> valObs,
		[CallerArgumentExpression(nameof(valObs))] string? valObsName = null
	)
	{
		node.AddRefresher(new ChgRefresher(ChgKeyMk.PropBool(node.Id, propName), valObs.Select(e => e ? "true" : "").ThrowIf_Observable_IsNot_Derived_From_RxVar(valObsName)));
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