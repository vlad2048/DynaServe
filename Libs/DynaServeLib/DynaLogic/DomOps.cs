using System.Runtime.Intrinsics.Arm;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DiffLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Nodes;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Syncing.Structs;
using AngleSharp.Dom;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.Logging;
using PowRxVar;

namespace DynaServeLib.DynaLogic;


record DomDbgNfo(
	IHtmlDocument ServerDom,
	RefreshTrackerDbgNfo RefreshTrackerDbgNfo
);


class DomOps : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Messenger messenger;
	private readonly RefreshTracker refreshTracker;

	public IHtmlDocument Dom { get; }
	public ILogr Logr { get; }
	public Action<IDomEvt> SignalDomEvt { get; }
	public IObservable<ClientMsg> WhenClientMsg => messenger.WhenClientMsg;
	public void SendToClient(ServerMsg msg) => messenger.SendToClient(msg);
	public DomDbgNfo GetDbgNfo() => new(Dom, refreshTracker.GetDbgNfo());

	public DomOps(
		IHtmlDocument dom,
		ILogr logr,
		Action<IDomEvt> signalDomEvt,
		Messenger messenger
	)
	{
		Dom = dom;
		Logr = logr;
		SignalDomEvt = signalDomEvt;
		this.messenger = messenger;
		refreshTracker = new RefreshTracker(this).D(d);
	}

	public void Log(string s) => Logr.Log(s);

	// ******************
	// * new ServInst() *
	// ******************
	public void AddInitialNodes(HtmlNode[] rootNodes)
	{
		var body = Dom.FindDescendant<IHtmlBodyElement>()!;
		foreach (var rootNode in rootNodes)
		{
			var (node, refreshers) = Dom.CreateNode(rootNode);
			body.AppendChild(node);
			refreshTracker.AddRefreshers(refreshers);
		}
	}

	/*private void LogNode(string title)
	{
		void L(string s) => Logr.Log($"=============> {s}");
		Logr.Log(" ");
		L($"[{title}]");
		var nod = Dom.GetElementById("id-16");
		if (nod == null)
		{
			L("NOD-16 not found");
			return;
		}
		var attr = nod.GetAttribute("onclick");
		if (attr == null)
		{
			L("NOD-16 onclick == null");
			return;
		}
		L($"NOD-16 onclick == '{attr}'");
	}*/

	// ******************
	// * DomEvtActioner *
	// ******************
	public void Handle_UpdateChildrenDomEvt(UpdateChildrenDomEvt evt)
	{
		var node = Dom.GetById(evt.NodeId);
		var (childrenPrev, refreshersPrevKeys) = node.GetChildrenAndTheirRefresherKeys();
		var (childrenNext, refreshersNext) = Dom.CreateNodes(evt.Children);

		if (DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(childrenPrev, childrenNext))
		{
			// (optimization: keep the same NodeIds as before to avoid sending updates for those)
			refreshersNext = DiffAlgo.KeepPrevIds_In_StructurallyIdentical_DomNodesTree_And_GetUpdatedRefreshers(childrenPrev, childrenNext, refreshersNext);
			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(childrenPrev, childrenNext);

			// Dom
			DiffAlgo.ApplyChgs_In_DomNodeTrees(childrenPrev, chgs);

			// Refreshers
			refreshTracker.ReplaceRefreshers(refreshersPrevKeys, refreshersNext);

			// Client
			if (chgs.Any()) messenger.SendToClient(ServerMsg.MkChgsDomUpdate(chgs));
		}
		else
		{
			// Dom
			node.RemoveAllChildren();
			node.AppendChildren(childrenNext);

			// Refreshers
			refreshTracker.ReplaceRefreshers(refreshersPrevKeys, refreshersNext);

			// Client
			messenger.SendToClient(ServerMsg.MkReplaceChildrenDomUpdate(childrenNext.Fmt(), evt.NodeId));
		}
	}

	public void Handle_ChgDomEvt(ChgDomEvt[] evts)
	{
		foreach (var evt in evts)
		{
			var chg = evt.Chg;
			var node = Dom.GetById(chg.NodeId);
			DiffAlgo.ApplyChgs_In_DomNodeTrees(node, chg);
		}

		var chgs = evts.SelectToArray(e => e.Chg);
		messenger.SendToClient(ServerMsg.MkChgsDomUpdate(chgs));
	}

	/*public void Handle_ChgDomEvt(ChgDomEvt evt)
	{
		var chg = evt.Chg;
		var node = Dom.GetById(chg.NodeId);
		DiffAlgo.ApplyChgs_In_DomNodeTrees(node, chg);
		messenger.SendToClient(ServerMsg.MkChgsDomUpdate(chg));
	}*/

	public void Handle_AddBodyNodeDomEvt(AddBodyNodeDomEvt evt)
	{
		var node = evt.Node;

		var (domNode, refreshers) = Dom.CreateNode(node);
		var body = Dom.FindDescendant<IHtmlBodyElement>()!;

		body.AppendChild(domNode);
		refreshTracker.AddRefreshers(refreshers);

		messenger.SendToClient(ServerMsg.MkAddChildToBody(domNode.Fmt()));
	}

	public void Handle_RemoveBodyNodeDomEvt(RemoveBodyNodeDomEvt evt)
	{
		var nodeId = evt.NodeId;

		var domNode = Dom.GetById(nodeId);
		refreshTracker.RemoveChildrenRefreshers(domNode, true);
		domNode.Remove();

		messenger.SendToClient(ServerMsg.MkRemoveChildFromBody(nodeId));
	}



	// ****************
	// * LiveReloader *
	// ****************
	// image/img.png
	public void BumpImageUrl(string imgUrl)
	{
		var nodes = Dom
			.GetAllImgNodes()
			.WhereToArray(e => e.Id != null && e.Source.GetRelevantLinkEnsure().IsSameAsWithoutQueryParams(imgUrl));
		foreach (var node in nodes)
			SignalDomEvt(new ChgDomEvt(ChgMk.Attr(
				node.Id!,
				"src",
				node.Source.GetRelevantLinkEnsure().BumpQueryParamCounter()
			)));
	}



	// *********************
	// * ChildrenRefresher *
	// *********************
	public void UpdateNodeChildren(string nodeId, HtmlNode[] children)
	{
		SignalDomEvt(new UpdateChildrenDomEvt(nodeId, children));
	}
}