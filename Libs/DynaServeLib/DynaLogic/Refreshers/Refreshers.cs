using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;
using System.Reactive.Linq;
using System.Reactive;

namespace DynaServeLib.DynaLogic.Refreshers;


interface IRefresher
{
	string NodeId { get; }
	IRefresher CloneWithId(string nodeId);
	PropChange[] GetInitialPropChanges();
	IDisposable Activate(DomOps domOps);
}


record NodeRefresher(string NodeId, IDisposable NodeD) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(DomOps domOps) => NodeD;
}


record ChildrenRefresher(string NodeId, IObservable<Unit> When, Func<HtmlNode[]> Fun) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(DomOps domOps) =>
		When.Subscribe(_ =>
			domOps.UpdateNodeChildren(NodeId, Fun())
		);
}


record PropChangeRefresher(string NodeId, PropChangeType Type, string? AttrName, IObservable<string?> ValObs) : IRefresher
{
	public IRefresher CloneWithId(string nodeId) => this with { NodeId = nodeId };
	public PropChange[] GetInitialPropChanges() => Array.Empty<PropChange>();
	public IDisposable Activate(DomOps domOps) =>
		ValObs.Subscribe(val =>
			domOps.SignalDomEvt(new PropChangeDomEvt(
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

	public IDisposable Activate(DomOps domOps) =>
		domOps.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.HookCalled && e.Id == NodeId && e.EvtName == EvtName)
			.SelectMany(_ => Observable.FromAsync(Action))
			.Subscribe();

	/*.Subscribe(async _ => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
		async _ => await Action(),
		"EvtRefresher",
		NodeId,
		EvtName,
		""
	));*/
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

	public IDisposable Activate(DomOps domOps) =>
		domOps.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.HookArgCalled && e.Id == NodeId && e.EvtName == EvtName)
			.SelectMany(e => Observable.FromAsync(() => Action(e.EvtArg!)))
			.Subscribe();

	/*.Subscribe(async e => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
		Action,
		"EvtRefresherArgs",
		NodeId,
		EvtName,
		e.EvtArg!
	));*/
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
