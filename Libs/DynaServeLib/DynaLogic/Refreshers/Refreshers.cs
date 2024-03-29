﻿using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using AngleSharp.Dom;
using PowRxVar;

namespace DynaServeLib.DynaLogic.Refreshers;


interface IRefresher
{
	IDisposable Activate(IElement node, DomOps domOps);
}



record ChildrenRefresher(IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public IDisposable Activate(IElement node, DomOps domOps) =>
		When
			//.Where(_ => domOps.IsRefMounted(node.Id))
			.Subscribe(_ =>
				domOps.SignalDomEvt(new UpdateChildrenDomEvt(node.GetIdEnsure(), Fun()))
			);
}


record ChgRefresher(ChgKey Key, IObservable<string?> ValObs) : IRefresher
{
	public IDisposable Activate(IElement node, DomOps domOps) =>
		ValObs
			//.Where(_ => domOps.IsRefMounted(node.Id))
			.Subscribe(val =>
			{
				var nodeId = node.GetIdEnsure();
				var xPath = $"//*[@id='{nodeId}']";
				var chg = Key.Make(xPath, val);
				domOps.SignalDomEvt(new ChgDomEvt(chg));
			});
}


record EvtRefresher(string EvtName, Func<Task> Action, bool StopPropagation) : IRefresher
{
	public IDisposable Activate(IElement node, DomOps domOps)
	{
		var id = node.GetIdEnsure();
		EvtRefresherUtils.SetNodeEvtAttr(node, EvtName, StopPropagation, null);
		return domOps.WhenClientMsg
			.OfType<HookCalledClientMsg>()
			.Where(e => e.Id == id && e.EvtName == EvtName)
			//.Where(_ => domOps.IsRefMounted(node.Id))
			.SelectMany(_ => Observable.FromAsync(Action))
			.Catch<Unit, Exception>(ex =>
			{
				Console.WriteLine($"Exception caught in EvtRefresher {ex}");
				return Observable.Throw<Unit>(ex);
			})
			.Retry()
			.Subscribe();
	}
}

record EvtArgRefresher(string EvtName, Func<string, Task> Action, string ArgExpr, bool StopPropagation) : IRefresher
{
	public IDisposable Activate(IElement node, DomOps domOps)
	{
		var id = node.GetIdEnsure();
		EvtRefresherUtils.SetNodeEvtAttr(node, EvtName, StopPropagation, ArgExpr);
		return domOps.WhenClientMsg
			.OfType<HookArgCalledClientMsg>()
			.Where(e => e.Id == id && e.EvtName == EvtName)
			//.Where(_ => domOps.IsRefMounted(node.Id))
			.SelectMany(e => Observable.FromAsync(() => Action(e.EvtArg!)))
			.Catch<Unit, Exception>(ex =>
			{
				L($"Exception caught in EvtArgRefresher {ex}");
				return Observable.Throw<Unit>(ex);
			})
			.Retry()
			.Subscribe();
	}

	private static void L(string s) => Console.WriteLine(s);
}




file static class EvtRefresherUtils
{
	public static void SetNodeEvtAttr(IElement node, string evtName, bool stopPropagation, string? argExpr) =>
		node.SetAttribute($"on{evtName}", MkEvtAttrVal(node.GetIdEnsure(), evtName, stopPropagation, argExpr));

	private static string MkEvtAttrVal(string id, string evtName, bool stopPropagation, string? argExpr) => argExpr switch
	{
		null => $"{MkStopPropagationStr(stopPropagation)}sockEvt('{id}', '{evtName}')",
		not null => $"{MkStopPropagationStr(stopPropagation)}sockEvtArg('{id}', '{evtName}', {argExpr})"
	};

	private static string? MkStopPropagationStr(bool stopPropagation) => stopPropagation switch
	{
		true => "event.stopPropagation();",
		false => null
	};
}
