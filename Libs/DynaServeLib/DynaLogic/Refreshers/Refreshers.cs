using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;
using System.Reactive.Linq;
using System.Reactive;

namespace DynaServeLib.DynaLogic.Refreshers;

record RefreshCtx(
	Action<IDomEvt> SignalDomEvt,
	IObservable<ClientMsg> WhenClientMsg,
	Action<ServerMsg> SendServerMsg
);

interface IRefresher
{
	string NodeId { get; }
	IDisposable Activate(RefreshCtx ctx);
}

record NodeRefresher(string NodeId, IDisposable NodeD) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => NodeD;
}

record ChildRefresher(string NodeId, IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => When.Subscribe(_ =>
	{
		ctx.SignalDomEvt(new ReplaceChildrenDomEvt(NodeId, Fun()));
	});
}

record DiffRefresher(string NodeId, IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => When.Subscribe(_ =>
	{
		ctx.SignalDomEvt(new DiffChildrenDomEvt(NodeId, Fun()));
	});
}

record AttrRefresher(string NodeId, string AttrName, IObservable<string?> ValObs) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => ValObs.Subscribe(val =>
	{
		ctx.SendServerMsg(ServerMsg.MkSetAttr(NodeId, AttrName, val));
	});
}

record ClsRefresher(string NodeId, IObservable<string?> ValObs) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => ValObs.Subscribe(val =>
	{
		ctx.SendServerMsg(ServerMsg.MkSetCls(NodeId, val));
	});
}

record EvtRefresher(string NodeId, string EvtName, Action Action) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => ctx.WhenClientMsg
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
		});
}

record EvtArgRefresher(string NodeId, string EvtName, Action<string> Action, string ArgExpr) : IRefresher
{
	public IDisposable Activate(RefreshCtx ctx) => ctx.WhenClientMsg
		.Where(e => e.Type == ClientMsgType.HookArgCalled && e.Id == NodeId && e.EvtName == EvtName)
		.Subscribe(e =>
		{
			Action(e.EvtArg!);
		});
}