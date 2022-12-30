using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;
using System.Reactive.Linq;
using System.Reactive;
using PowMaybe;

namespace DynaServeLib.DynaLogic.Refreshers;

record RefreshCtx(
	Action<IDomEvt> SignalDomEvt,
	IObservable<ClientMsg> WhenClientMsg,
	Action<ServerMsg> SendServerMsg
);

interface IRefresher
{
	string NodeId { get; }
	IRefresher CloneWithId(string nodeId);
	PropChange[] GetInitialPropChanges();
	IDisposable Activate(RefreshCtx ctx);
}

record NodeRefresher(string NodeId, IDisposable NodeD) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(RefreshCtx ctx) => NodeD;
}

record ChildrenRefresher(string NodeId, IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(RefreshCtx ctx) =>
		When.Subscribe(_ =>
			ctx.SignalDomEvt(new UpdateChildrenDomEvt(NodeId, Fun()))
		);
}

record PropChangeRefresher(string NodeId, PropChangeType Type, string? AttrName, IObservable<string?> ValObs) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(RefreshCtx ctx) =>
		ValObs.Subscribe(val =>
			ctx.SignalDomEvt(new PropChangeDomEvt(
				Type switch
				{
					PropChangeType.Attr => PropChange.MkAttrChange(NodeId, AttrName!, val),
					PropChangeType.Text => PropChange.MkTextChange(NodeId, val),
					_ => throw new ArgumentException()
				}
			))
		);

	public static PropChangeRefresher MkAttr(string nodeId, string? attrName, IObservable<string?> valObs) => new(nodeId, PropChangeType.Attr, attrName, valObs);
	public static PropChangeRefresher MkText(string nodeId, IObservable<string?> valObs) => new(nodeId, PropChangeType.Text, null, valObs);
}


/*record AttrRefresher(string NodeId, string AttrName, IObservable<string?> ValObs) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(RefreshCtx ctx) =>
		ValObs.Subscribe(attrVal =>
			ctx.SignalDomEvt(new PropChangeDomEvt(
				PropChange.MkAttrChange(NodeId, AttrName, attrVal)
			))
		);
}

record ClsRefresher(string NodeId, IObservable<string?> ValObs) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(RefreshCtx ctx) =>
		ValObs.Subscribe(val =>
		{
			//ctx.SendServerMsg(ServerMsg.MkSetCls(NodeId, val));
			
			Console.WriteLine($"ClsRefresher: {PropChange.MkAttrChange(NodeId, "class", val)}");

			ctx.SignalDomEvt(new PropChangeDomEvt(
				PropChange.MkAttrChange(NodeId, "class", val)
			));
		});
}*/


record EvtRefresher(string NodeId, string EvtName, Func<Task> Action, bool StopPropagation) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => new[]
	{
		PropChange.MkAttrChange(
			NodeId,
			$"on{EvtName}",
			$"{EvtRefresherUtils.MkStopPropagationStr(StopPropagation)}sockEvt('{NodeId}', '{EvtName}')"
		)
	};
	public IDisposable Activate(RefreshCtx ctx) =>
		ctx.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.HookCalled && e.Id == NodeId && e.EvtName == EvtName)
			.Subscribe(async _ => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
				async _ => await Action(),
				"EvtRefresher",
				NodeId,
				EvtName,
				""
			));
}

record EvtArgRefresher(string NodeId, string EvtName, Func<string, Task> Action, string ArgExpr, bool StopPropagation) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => new[]
	{
		PropChange.MkAttrChange(
			NodeId,
			$"on{EvtName}",
			$"{EvtRefresherUtils.MkStopPropagationStr(StopPropagation)}sockEvtArg('{NodeId}', '{EvtName}', {ArgExpr})"
		)
	};

	public IDisposable Activate(RefreshCtx ctx) =>
		ctx.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.HookArgCalled && e.Id == NodeId && e.EvtName == EvtName)
			.Subscribe(async e => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
				Action,
				"EvtRefresherArgs",
				NodeId,
				EvtName,
				e.EvtArg!
			));
}




file static class EvtRefresherUtils
{
	public static string? MkStopPropagationStr(bool stopPropagation) => stopPropagation switch
	{
		true => "event.stopPropagation();",
		false => null
	};

	public static async Task WrapAsyncActionInTryCatch(
		Func<string, Task> action,
		string title,
		string nodeId,
		string evtName,
		string argStr
	)
	{
		try
		{
			await action(argStr);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception in {title}");
			Console.WriteLine($"  NodeId : {nodeId}");
			Console.WriteLine($"  EvtName: {evtName}");
			Console.WriteLine($"  Ex.Msg : {ex.Message}");
			Console.WriteLine("  Ex:");
			Console.WriteLine($"{ex}");
		}
	}
}
