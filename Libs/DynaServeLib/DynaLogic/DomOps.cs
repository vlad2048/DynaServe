using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Syncing.Structs;
using AngleSharp.Dom;
using DynaServeLib.DynaLogic.DiffLogic;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.Logging;
using PowRxVar;

namespace DynaServeLib.DynaLogic;


/*record DomDbgNfo(
	IHtmlDocument ServerDom,
	RefreshTrackerDbgNfo RefreshTrackerDbgNfo
);*/


class DomOps : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Messenger messenger;
	private readonly Dictionary<IElement, Disp> map;
	private IHtmlBodyElement Body => Dom.FindDescendant<IHtmlBodyElement>()!;
	//private readonly RefreshTracker refreshTracker;

	public IHtmlDocument Dom { get; }
	public ILogr Logr { get; }
	public Action<IDomEvt> SignalDomEvt { get; }
	public IObservable<ClientMsg> WhenClientMsg => messenger.WhenClientMsg;
	public void SendToClient(ServerMsg msg) => messenger.SendToClient(msg);
	//public DomDbgNfo GetDbgNfo() => new(Dom, refreshTracker.GetDbgNfo());

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
		map = new Dictionary<IElement, Disp>().D(d);
		//refreshTracker = new RefreshTracker(this).D(d);
	}

	public void Log(string s) => Logr.Log(s);

	// ******************
	// * new ServInst() *
	// ******************
	public void AddInitialNodes(HtmlNode[] rootNodes)
	{
		var nodRefs = rootNodes.CreateNodes(Dom);
		AddUnder(Body, nodRefs, false);
	}


	// ******************
	// * DomEvtActioner *
	// ******************
	public void Handle_UpdateChildrenDomEvt(UpdateChildrenDomEvt evt)
	{
		var node = Dom.GetById(evt.NodeId);
		var nodsRefs = evt.Children.CreateNodes(Dom);
		if (false) //DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(node.Children, nodsRefs.Nods))
		{

		}
		else
		{
			DelUnder(node, true);
			AddUnder(node, nodsRefs, true);
		}
	}

	/*public void Handle_UpdateChildrenDomEvt(UpdateChildrenDomEvt evt)
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
	}*/

	public void Handle_AddBodyNodeDomEvt(AddBodyNodeDomEvt evt)
	{
		var nodRefs = evt.Node.CreateNode(Dom);
		AddUnder(Body, nodRefs, true);
	}

	public void Handle_RemoveBodyNodeDomEvt(RemoveBodyNodeDomEvt evt)
	{
		var nodeId = evt.NodeId;
		var domNode = Dom.GetById(nodeId);
		DelParent(domNode, true);
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



	// ****************
	// * LiveReloader *
	// ****************
	// image/img.png
	public void BumpImageUrl(string imgUrl)
	{
		// TODO: image auto updates

		/*var nodes = Dom
			.GetAllImgNodes()
			.WhereToArray(e => e.Id != null && e.Source.GetRelevantLinkEnsure().IsSameAsWithoutQueryParams(imgUrl));
		foreach (var node in nodes)
			SignalDomEvt(new ChgDomEvt(ChgMk.Attr(
				node.Id!,
				"src",
				node.Source.GetRelevantLinkEnsure().BumpQueryParamCounter()
			)));*/
	}



	// *********************
	// * ChildrenRefresher *
	// *********************
	public void UpdateNodeChildren(string nodeId, HtmlNode[] children)
	{
		SignalDomEvt(new UpdateChildrenDomEvt(nodeId, children));
	}





	// ******************************************
	// * private Node & Refreshers manipulation *
	// ******************************************
	private void AddUnder(IElement parent, NodRefs nodRefs, bool sendMsg)
	{
		parent.AppendChild(nodRefs.Nod);
		AddRefs(nodRefs.RefreshMap);
		if (sendMsg)
			messenger.SendToClient(ServerMsg.MkDomOp(
				DomOp.MkInsertHtmlUnderParent(nodRefs.Nod.Fmt(), parent.Id ?? throw new ArgumentException())
			));
	}

	private void AddUnder(IElement parent, NodsRefs nodsRefs, bool sendMsg)
	{
		parent.AppendChildren(nodsRefs.Nods);
		AddRefs(nodsRefs.RefreshMap);
		if (sendMsg)
			messenger.SendToClient(ServerMsg.MkDomOp(
				DomOp.MkInsertHtmlUnderParent(nodsRefs.Nods.Fmt(), parent.Id ?? throw new ArgumentException())
			));
	}

	private void DelUnder(IElement parent, bool sendMsg)
	{
		DelRefs(parent.Children);
		parent.RemoveAllChildren();
		if (sendMsg)
			messenger.SendToClient(ServerMsg.MkDomOp(
				DomOp.MkDeleteHtmlUnderParent(parent.Id ?? throw new ArgumentException())
			));
	}

	private void DelParent(IElement parent, bool sendMsg)
	{
		DelRefs(parent);
		parent.Remove();
		if (sendMsg)
			messenger.SendToClient(ServerMsg.MkDomOp(
				DomOp.MkDeleteParent(parent.Id ?? throw new ArgumentException())
			));
	}



	public int idCnt;

	private void AddRefs(RefreshMap refMap)
	{
		foreach (var (node, refs) in refMap)
		{
			if (map.ContainsKey(node)) throw new ArgumentException();
			node.Id = $"id-{idCnt++}";;
			var refD = new Disp();
			foreach (var @ref in refs)
				@ref.Activate(node, this).D(refD);
			map[node] = refD;
		}
	}

	private void DelRefs(IEnumerable<IElement> roots) => roots.ForEach(DelRefs);

	private void DelRefs(IElement root)
	{
		root.Recurse<IElement>(node =>
		{
			if (map.TryGetValue(node, out var refD))
			{
				refD.Dispose();
				map.Remove(node);
			}
		});
	}
}