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
	public IObservable<IClientMsg> WhenClientMsg => messenger.WhenClientMsg;
	public void SendToClient(IServerMsg msg) => messenger.SendToClient(msg);
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
		var nodRef = rootNodes.Create(Dom);
		AddUnder(Body, nodRef, false);
	}


	// ******************
	// * DomEvtActioner *
	// ******************
	public void Handle_UpdateChildrenDomEvt(UpdateChildrenDomEvt evt)
	{
		var node = Dom.GetById(evt.NodeId);
		var nodsRefs = evt.Children.Create(Dom);
		//ReplUnder(node, nodsRefs);

		/*if (DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(node.Children, nodsRefs.Nods))
		{
			var rootNext = Dom.CreateElementWithChildren<IHtmlDivElement>(nodsRefs.Nods);
			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(
				node,
				rootNext
			);
			DiffAlgo.ApplyChgs_In_Dom(Dom, chgs);
			messenger.SendToClient(new ChgsDomUpdateServerMsg(chgs));
		}
		else*/
		{
			ReplUnder(node, nodsRefs);
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
		var nodRefs = evt.Node.Create(Dom);
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
			DiffAlgo.ApplyChgs_In_Dom(Dom, chg);
		}

		var chgs = evts.SelectToArray(e => e.Chg);
		messenger.SendToClient(new ChgsDomUpdateServerMsg(chgs));
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
	private void AddUnder(IElement parent, NodRef nodRef, bool sendMsg)
	{
		parent.AppendChildren(nodRef.Nods);
		AddRefs(nodRef.RefreshMap);
		if (sendMsg)
			messenger.SendToClient(new DomOpServerMsg(
				DomOpType.InsertHtmlUnderParent,
				nodRef.Nods.Fmt(),
				parent.Id ?? throw new ArgumentException())
			);
	}

	private void ReplUnder(IElement parent, NodRef nodRef)
	{
		DelRefs(parent.Children);
		parent.RemoveAllChildren();
		parent.AppendChildren(nodRef.Nods);
		AddRefs(nodRef.RefreshMap);
		messenger.SendToClient(new DomOpServerMsg(
			DomOpType.ReplaceHtmlUnderParent,
			nodRef.Nods.Fmt(),
			parent.Id ?? throw new ArgumentException())
		);
	}

	private void DelParent(IElement parent, bool sendMsg)
	{
		DelRefs(parent);
		parent.Remove();
		if (sendMsg)
			messenger.SendToClient(new DomOpServerMsg(
				DomOpType.DeleteParent,
				null,
				parent.Id ?? throw new ArgumentException()
			));
	}



	public int idCnt;

	private void AddRefs(RefreshMap refMap)
	{
		foreach (var (node, refs) in refMap)
		{
			if (map.ContainsKey(node)) throw new ArgumentException();
			node.Id ??= $"id-{idCnt++}";
			var refD = new Disp();
			foreach (var @ref in refs)
			{
				L($"Adding[{node.Id}] {@ref.GetType().Name}");
				@ref.Activate(node, this).D(refD);
			}

			map[node] = refD;
		}
	}

	private static void L(string s) => Console.WriteLine(s);

	private void DelRefs(IEnumerable<IElement> roots) => roots.ForEach(DelRefs);

	private void DelRefs(IElement root)
	{
		root.Recurse<IElement>(node =>
		{
			if (map.TryGetValue(node, out var refD))
			{
				var idStr = node.Id ?? "_";
				L($"Removing[{idStr}]");
				refD.Dispose();
				map.Remove(node);
			}
		});
	}
}