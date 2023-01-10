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
	private readonly RefStats refStats = new();
	private readonly HashSet<string> refSet = new();
	private IHtmlBodyElement Body => Dom.FindDescendant<IHtmlBodyElement>()!;
	private int idCnt;
	private bool IsRefMounted(string? nodeId) => nodeId != null && refSet.Contains(nodeId);

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
		refStats.Clear();
		var rootPrev = Dom.GetById(evt.NodeId);
		var nodsRefs = evt.Children.Create(Dom);

		if (DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(rootPrev.Children, nodsRefs.Nods))
		{
			DelRefs(rootPrev.Children);
			var rootPrevCopy = (rootPrev.Clone() as IElement)!;
			rootPrev.RemoveAllChildren();
			rootPrev.AppendChildren(nodsRefs.Nods);
			AddRefs(nodsRefs.RefreshMap);

			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(
				rootPrevCopy,
				rootPrev
			);
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
		refStats.Clear();
		var nodRefs = evt.Node.Create(Dom);
		AddUnder(Body, nodRefs, true);
		LogState("Add Body Node");
	}

	public void Handle_RemoveBodyNodeDomEvt(RemoveBodyNodeDomEvt evt)
	{
		refStats.Clear();
		var nodeId = evt.NodeId;
		var domNode = Dom.GetById(nodeId);
		DelParent(domNode, true);
		LogState("Remove Body Node");
	}

	public void Handle_ChgDomEvt(ChgDomEvt[] evts)
	{
		refStats.Clear();
		var chgs = evts.SelectToArray(e => e.Chg);
		chgs = DiffAlgo.ApplyChgs_In_Dom(Dom, "Handle_ChgDomEvt", chgs);
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
			refStats.Added.Add(node.Id);
			refSet.Add(node.Id);
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
				refSet.Remove(node.Id ?? "");
				refStats.Removed.Add(node.Id ?? "(missing)");
				refD.Dispose();
				map.Remove(node);
			}
		});
	}

	private void LogState(string transitionStr)
	{
		//DomOpsUtils.LogState(transitionStr, Dom, map, refStats);
	}
}
