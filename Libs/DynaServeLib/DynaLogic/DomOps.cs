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

namespace DynaServeLib.DynaLogic;


record DomDbgNfo(
	IHtmlDocument ServerDom,
	RefreshTrackerDbgNfo RefreshTrackerDbgNfo
);


class DomOps
{
	private readonly Messenger messenger;
	private readonly RefreshTracker refreshTracker;

	public IHtmlDocument Dom { get; }
	public Action<IDomEvt> SignalDomEvt { get; }
	public IObservable<ClientMsg> WhenClientMsg => messenger.WhenClientMsg;
	public void SendToClient(ServerMsg msg) => messenger.SendToClient(msg);
	public DomDbgNfo GetDbgNfo() => new(Dom, refreshTracker.GetDbgNfo());

	public DomOps(
		IHtmlDocument dom,
		Action<IDomEvt> signalDomEvt,
		Messenger messenger
	)
	{
		Dom = dom;
		SignalDomEvt = signalDomEvt;
		this.messenger = messenger;
		refreshTracker = new RefreshTracker(this);
	}

	// ******************
	// * new ServInst() *
	// ******************
	public void AddInitialNodes(HtmlNode[] rootNodes)
	{
		var body = Dom.FindDescendant<IHtmlBodyElement>()!;
		foreach (var rootNode in rootNodes)
		{
			var (node, refreshers) = Dom.CreateNode(rootNode);
			var refreshersInitialPropChanges = refreshers.SelectMany(f => f.GetInitialPropChanges()).ToArray();
			DiffAlgo.ApplyPropChanges_In_DomNodeTrees(new[] { node }, refreshersInitialPropChanges);
			body.AppendChild(node);
			refreshTracker.AddRefreshers(refreshers);
		}
	}

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
			var refreshersNextInitialPropChanges = refreshersNext.SelectMany(f => f.GetInitialPropChanges()).ToArray();
			var propChanges = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(childrenPrev, childrenNext);
			propChanges = propChanges.Concat(refreshersNextInitialPropChanges).ToArray();

			// Dom
			DiffAlgo.ApplyPropChanges_In_DomNodeTrees(childrenPrev, propChanges);

			// Refreshers
			refreshTracker.ReplaceRefreshers(refreshersPrevKeys, refreshersNext);

			// Client
			if (propChanges.Any()) messenger.SendToClient(ServerMsg.MkPropChangesDomUpdate(propChanges));
		}
		else
		{
			// Dom
			node.RemoveAllChildren();
			node.AppendChildren(childrenNext);

			// Refreshers
			refreshTracker.ReplaceRefreshers(refreshersPrevKeys, refreshersNext);
			var refreshersNextInitialPropChanges = refreshersNext.SelectMany(f => f.GetInitialPropChanges()).ToArray();
			DiffAlgo.ApplyPropChanges_In_DomNodeTrees(childrenNext, refreshersNextInitialPropChanges);

			// Client
			messenger.SendToClient(ServerMsg.MkReplaceChildrenDomUpdate(childrenNext.Fmt(), evt.NodeId));
		}
	}

	public void Handle_PropChangeDomEvt(PropChangeDomEvt evt)
	{
		var node = Dom.GetById(evt.Chg.NodeId);
		var propChanges = new[] { evt.Chg };
		DiffAlgo.ApplyPropChanges_In_DomNodeTrees(new [] { node }, propChanges);
		messenger.SendToClient(ServerMsg.MkPropChangesDomUpdate(propChanges));
	}

	public void Handle_AddBodyNodeDomEvt(AddBodyNodeDomEvt evt)
	{
		var node = evt.Node;

		var (domNode, refreshers) = Dom.CreateNode(node);
		var refreshersInitialPropChanges = refreshers.SelectMany(f => f.GetInitialPropChanges()).ToArray();
		DiffAlgo.ApplyPropChanges_In_DomNodeTrees(new [] { domNode }, refreshersInitialPropChanges);
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
			SignalDomEvt(new PropChangeDomEvt(PropChange.MkAttrChange(
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