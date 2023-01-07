using DynaServeLib.DynaLogic.Events;
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
		When.Subscribe(_ =>
			domOps.UpdateNodeChildren(node.GetIdEnsure(), Fun())
		);
}


record ChgRefresher(ChgKey Key, IObservable<string?> ValObs) : IRefresher
{
	public IDisposable Activate(IElement node, DomOps domOps) =>
		ValObs.Subscribe(val =>
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
			.Where(e => e.EvtName == EvtName)
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

	/*public static async Task WrapAsyncActionInTryCatch(
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
	}*/
}




/*.Subscribe(async _ => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
	async _ => await Action(),
	"EvtRefresher",
	NodeId,
	EvtName,
	""
));


.Subscribe(async e => await EvtRefresherUtils.WrapAsyncActionInTryCatch(
	Action,
	"EvtRefresherArgs",
	NodeId,
	EvtName,
	e.EvtArg!
));*/
