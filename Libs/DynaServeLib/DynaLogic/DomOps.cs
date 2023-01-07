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


class DomOps : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Messenger messenger;
	private readonly Dictionary<IElement, Disp> map;
	private IHtmlBodyElement Body => Dom.FindDescendant<IHtmlBodyElement>()!;
	private int idCnt;

	public IHtmlDocument Dom { get; }
	public string GetNextNodeId() => $"id-{idCnt++}";
	public ILogr Logr { get; }
	public Action<IDomEvt> SignalDomEvt { get; }
	public IObservable<IClientMsg> WhenClientMsg => messenger.WhenClientMsg;
	public void SendToClient(IServerMsg msg) => messenger.SendToClient(msg);

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
		var rootPrev = Dom.GetById(evt.NodeId);
		var nodsRefs = evt.Children.Create(Dom);

		if (DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(rootPrev.Children, nodsRefs.Nods))
		{
			var rootNext = Dom.CreateElementWithChildren<IHtmlDivElement>(nodsRefs.Nods);

			// Before we can compare the new nodes to the old ones to detect changes,
			// we need to apply and remove the refreshers on the new nodes so they have the same attributes
			// and they need to be applied with the same NodeIds, that's why we compute a map lookup first
			var backMap = DomOpsUtils.GetBackMap(rootPrev, rootNext);
			DomOpsUtils.FlashRefreshers(rootNext, nodsRefs.RefreshMap, backMap, this);

			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(
				rootPrev,
				rootNext
			);
			DiffAlgo.ApplyChgs_In_Dom(Dom, "Handle_UpdateChildrenDomEvt", chgs);
			messenger.SendToClient(new ChgsDomUpdateServerMsg(chgs));
		}
		else
		{
			ReplUnder(rootPrev, nodsRefs);
		}
		LogState($"Update Children id={evt.NodeId}");
	}



	public void Handle_AddBodyNodeDomEvt(AddBodyNodeDomEvt evt)
	{
		var nodRefs = evt.Node.Create(Dom);
		AddUnder(Body, nodRefs, true);
		LogState("Add Body Node");
	}

	public void Handle_RemoveBodyNodeDomEvt(RemoveBodyNodeDomEvt evt)
	{
		var nodeId = evt.NodeId;
		var domNode = Dom.GetById(nodeId);
		DelParent(domNode, true);
		LogState("Remove Body Node");
	}

	public void Handle_ChgDomEvt(ChgDomEvt[] evts)
	{
		foreach (var evt in evts)
		{
			var chg = evt.Chg;
			DiffAlgo.ApplyChgs_In_Dom(Dom, "Handle_ChgDomEvt", chg);
		}

		var chgs = evts.SelectToArray(e => e.Chg);
		messenger.SendToClient(new ChgsDomUpdateServerMsg(chgs));
		LogState($"Chg Dom cnt={evts.Length}");
	}



	// ****************
	// * LiveReloader *
	// ****************
	// image/img.png
	public void BumpImageUrl(string imgUrl)
	{
		// TODO: image auto updates

		//var nodes = Dom
		//	.GetAllImgNodes()
		//	.WhereToArray(e => e.Id != null && e.Source.GetRelevantLinkEnsure().IsSameAsWithoutQueryParams(imgUrl));
		//foreach (var node in nodes)
		//	SignalDomEvt(new ChgDomEvt(ChgMk.Attr(
		//		node.Id!,
		//		"src",
		//		node.Source.GetRelevantLinkEnsure().BumpQueryParamCounter()
		//	)));
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



	private void AddRefs(RefreshMap refMap)
	{
		foreach (var (node, refs) in refMap)
		{
			if (map.ContainsKey(node)) throw new ArgumentException();
			node.Id ??= GetNextNodeId();
			var refD = new Disp();
			foreach (var @ref in refs)
			{
				@ref.Activate(node, this).D(refD);
			}

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

	private void LogState(string transitionStr)
	{
		//DomOpsUtils.LogState(transitionStr, Dom, map);
	}
}
