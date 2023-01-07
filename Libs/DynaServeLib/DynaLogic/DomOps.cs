using System.Text;
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
using PowTrees.Algorithms;

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
			var backMap = GetBackMap(rootPrev, rootNext);
			FlashRefreshers(rootNext, nodsRefs.RefreshMap, backMap);

			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(
				rootPrev,
				rootNext
			);
			DiffAlgo.ApplyChgs_In_Dom(Dom, chgs);
			messenger.SendToClient(new ChgsDomUpdateServerMsg(chgs));
		}
		else
		{
			ReplUnder(rootPrev, nodsRefs);
		}
		LogState($"Update Children id={evt.NodeId}");
	}

	private Dictionary<IElement, IElement> GetBackMap(IElement rootPrev, IElement rootNext)
	{
		var treePrev = rootPrev.ToTree().TreeOfType<IElement>();
		var treeNext = rootNext.ToTree().TreeOfType<IElement>();
		return treePrev.ZipTree(treeNext)
			.ToDictionary(
				t => t.V.Item2,
				t => t.V.Item1
			);
	}

	private void FlashRefreshers(IElement root, RefreshMap refMap, Dictionary<IElement, IElement> backMap) =>
		root.Recurse<IElement>(node =>
		{
			if (!refMap.TryGetValue(node, out var refs)) return;
			if (!backMap.TryGetValue(node, out var nodePrev))
			{
				throw new ArgumentException();
			}
			if (nodePrev.Id == null)
			{
				throw new ArgumentException();
			}
			node.Id = nodePrev.Id;
			foreach (var @ref in refs)
			{
				var refD = @ref.Activate(node, this);
				refD.Dispose();
			}
		});

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
			DiffAlgo.ApplyChgs_In_Dom(Dom, chg);
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
		return;
		var tree = Dom.FindDescendant<IHtmlBodyElement>()!
			.ToTree()
			.Filter(
				e => e is IElement,
				opt =>
				{
					opt.Type = TreeFilterType.KeepIfMatchingOnly;
				}
			).Single()
			.Map(e => (IElement)e);
		var treeStr = tree.LogToString(opt =>
		{
			opt.FormatFun = GetNodeStr;
		});
		LTitle(transitionStr);
		L(treeStr);
		var unvisited = GetUnmountedRefresherNodes();
		if (unvisited.Any())
		{
			var unvisitedStr = unvisited.Select(e => e.Id ?? "_").JoinText(",");
			L($" -> {unvisited.Length} nodes: {unvisitedStr}");
		}
		LN();
	}

	private string GetNodeStr(IElement node)
	{
		var sb = new StringBuilder();
		sb.Append($"<{node.TagName}");
		if (node.Id != null)
			sb.Append($" id='{node.Id}'");
		if (node.GetAttribute("onclick") != null)
			sb.Append(" onclick");
		if (map.ContainsKey(node))
			sb.Append(" (*)");
		sb.Append(">");
		return sb.ToString();
	}

	private IElement[] GetUnmountedRefresherNodes()
	{
		var visited = new List<IElement>();
		Dom.FindDescendant<IHtmlBodyElement>()!.Recurse<IElement>(e =>
		{
			if (map.ContainsKey(e))
				visited.Add(e);
		});
		var unvisited = map.Keys.WhereNotToArray(visited.Contains);
		return unvisited;
	}

	private static void L(string s) => Console.WriteLine(s);
	private static void LTitle(string s)
	{
		L(s);
		L(new string('=', s.Length));
	}
	private static void LN() => Console.WriteLine();
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
