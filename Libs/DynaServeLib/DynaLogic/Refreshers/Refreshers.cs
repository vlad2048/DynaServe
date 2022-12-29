﻿using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using PowRxVar;
using AngleSharp.Dom;

namespace DynaServeLib.DynaLogic.Refreshers;

record RefreshCtx(
	Action<IDomEvt> SignalDomEvt,
	IObservable<ClientMsg> WhenClientMsg,
	Action<ServerMsg> SendServerMsg
);

interface IRefresher
{
	string NodeId { get; }
	AttrChange[] GetInitialAttrChanges();
	IDisposable Activate(RefreshCtx ctx);
	IRefresher CloneWithId(string nodeId);
}

record NodeRefresher(string NodeId, IDisposable NodeD) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges() => Array.Empty<AttrChange>();
	public IDisposable Activate(RefreshCtx ctx) => NodeD;
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}

record ChildrenRefresher(string NodeId, IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges() => Array.Empty<AttrChange>();
	public IDisposable Activate(RefreshCtx ctx) => When.Subscribe(_ =>
	{
		ctx.SignalDomEvt(new UpdateChildrenDomEvt(NodeId, Fun()));
	});
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}

record AttrRefresher(string NodeId, string AttrName, IObservable<string?> ValObs) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges() => Array.Empty<AttrChange>();
	public IDisposable Activate(RefreshCtx ctx) => ValObs.Subscribe(val =>
	{
		ctx.SendServerMsg(ServerMsg.MkSetAttr(NodeId, AttrName, val));
	});
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}

record ClsRefresher(string NodeId, IObservable<string?> ValObs) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges() => Array.Empty<AttrChange>();
	public IDisposable Activate(RefreshCtx ctx) => ValObs.Subscribe(val =>
	{
		ctx.SendServerMsg(ServerMsg.MkSetCls(NodeId, val));
	});
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}

record EvtRefresher(string NodeId, string EvtName, Action Action, bool StopPropagation) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges()
	{
		var stopStr = StopPropagation switch
		{
			true => "event.stopPropagation();",
			false => null
		};
		return new AttrChange[]
		{
			new(NodeId, $"on{EvtName}", $"{stopStr}sockEvt('{NodeId}', '{EvtName}')")
		};
	}

	public IDisposable Activate(RefreshCtx ctx)
	{
		var d = new Disp();
		Disposable.Create(() =>
		{
			var chk = NodeId;
			var abc = 123;
		}).D(d);
		ctx.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.HookCalled && e.Id == NodeId && e.EvtName == EvtName)
			.Subscribe(_ =>
			{
				try
				{
					Action();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception in EvtRefresher");
					Console.WriteLine($"  NodeId : {NodeId}");
					Console.WriteLine($"  EvtName: {EvtName}");
					Console.WriteLine($"  Ex.Msg : {ex.Message}");
					Console.WriteLine("  Ex:");
					Console.WriteLine($"{ex}");
				}
			}).D(d);
		return d;
	}

	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}

record EvtArgRefresher(string NodeId, string EvtName, Action<string> Action, string ArgExpr, bool StopPropagation) : IRefresher
{
	public AttrChange[] GetInitialAttrChanges()
	{
		var stopStr = StopPropagation switch
		{
			true => "event.stopPropagation();",
			false => null
		};
		return new AttrChange[]
		{
			new(NodeId, $"on{EvtName}", $"{stopStr}sockEvtArg('{NodeId}', '{EvtName}', {ArgExpr})")
		};
	}

	public IDisposable Activate(RefreshCtx ctx) => ctx.WhenClientMsg
		.Where(e => e.Type == ClientMsgType.HookArgCalled && e.Id == NodeId && e.EvtName == EvtName)
		.Subscribe(e =>
		{
			Action(e.EvtArg!);
		});
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
}