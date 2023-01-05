using AngleSharp.Dom;
using DynaServeLib.Serving.Syncing.Structs;

namespace DynaServeLib.DynaLogic.DiffLogic;

static class DiffAlgo
{
	public static void ApplyChgs_In_DomNodeTrees(IElement rootsNext, params Chg[] chgs) => ApplyChgs_In_DomNodeTrees(new[] { rootsNext }, chgs);
	public static void ApplyChgs_In_DomNodeTrees(IElement[] rootsNext, params Chg[] chgs)
	{
		var nodeMap = rootsNext.GetNodeMap();
		foreach (var chg in chgs)
		{
			var node = nodeMap[chg.NodeId];
			switch (chg.Type)
			{
				case ChgType.Attr:
					if (chg.Val != null)
						node.SetAttribute(chg.Name!, chg.Val);
					else
						node.RemoveAttribute(chg.Name!);
					break;

				case ChgType.Prop:
					break;

				case ChgType.Text:
					node.TextContent = chg.Val ?? "";
					break;

				default:
					throw new ArgumentException();
			}
		}
	}
}


/*
using AngleSharp.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Serving.Syncing.Structs;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DiffLogic;

static class DiffAlgo
{
	public static bool Are_DomNodeTrees_StructurallyIdentical(IHtmlCollection<IElement> rootsPrev, IElement[] rootsNext) =>
		rootsPrev.Length == rootsNext.Length &&
		rootsPrev.Zip(rootsNext).All(t => Is_DomNodeTree_StructurallyIdentical(t.First, t.Second));

	private static bool Is_DomNodeTree_StructurallyIdentical(IElement rootPrev, IElement rootNext)
	{
		if (rootPrev.NodeName != rootNext.NodeName)
			return false;
		var childrenPrev = rootPrev.Children.ToArray();
		var childrenNext = rootNext.Children.ToArray();
		return Are_DomNodeTrees_StructurallyIdentical(childrenPrev, childrenNext);
	}


	public static IRefresher[] KeepPrevIds_In_StructurallyIdentical_DomNodesTree_And_GetUpdatedRefreshers(IElement[] rootsPrev, IElement[] rootsNext, IRefresher[] refreshersNext)
	{
		var map = MakeNodeMap(rootsPrev, rootsNext);
		rootsPrev.Zip(rootsNext).ForEach(t => Recurse(t.First, t.Second, (eltPrev, eltNext) =>
		{
			Fix(eltPrev, eltNext, eltNext.Id!, map[eltNext.Id!]);
		}));
		return refreshersNext.SelectToArray(e => e.CloneWithId(map[e.NodeId]));
	}

	private static void Fix(IElement eltPrev, IElement eltNext, string idSrc, string idDst)
	{
		eltNext.Id = eltPrev.Id;

		eltNext.Attributes
			.Where(attr => attr.Value.Contains("sockEvt('") || attr.Value.Contains("sockEvtArg('"))
			.ForEach(attr =>
			{
				var attrValPrev = attr.Value;
				var attrValNext = attrValPrev.Replace(idSrc, idDst);
				attr.Value = attrValNext;
			});
	}

	private static void Recurse(IElement eltPrev, IElement eltNext, Action<IElement, IElement> action)
	{
		action(eltPrev, eltNext);
		var childrenPrev = eltPrev.Children.ToArray();
		var childrenNext = eltNext.Children.ToArray();
		foreach (var (childPrev, childNext) in childrenPrev.Zip(childrenNext))
			Recurse(childPrev, childNext, action);
	}


	private static Dictionary<string, string> MakeNodeMap(IElement[] rootsPrev, IElement[] rootsNext)
	{
		var map = new Dictionary<string, string>();
		void RecurseLoc(IElement nodePrev, IElement nodeNext)
		{
			map[nodeNext.Id!] = nodePrev.Id!;
			var childrenPrev = nodePrev.Children.ToArray();
			var childrenNext = nodeNext.Children.ToArray();
			foreach (var (childPrev, childNext) in childrenPrev.Zip(childrenNext))
				RecurseLoc(childPrev, childNext);
		}
		foreach (var (childPrev, childNext) in rootsPrev.Zip(rootsNext))
			RecurseLoc(childPrev, childNext);
		return map;
	}



	public static Chg[] ComputePropChanges_In_StructurallyIdentical_DomNodesTree(IElement[] rootsPrev, IElement[] rootsNext)
	{
		var list = new List<Chg>();

		void Recurse(IElement nodePrev, IElement nodeNext)
		{
			if (nodeNext.Id != nodePrev.Id) throw new ArgumentException();
			var nodeId = nodeNext.Id!;


			// ********
			// * Attr *
			// ********
			var attrsPrev = nodePrev.Attributes.SelectToArray(e => e.Name);
			var attrsNext = nodeNext.Attributes.SelectToArray(e => e.Name);

			var attrsAdded = attrsNext.WhereNotToArray(attrsPrev.Contains);
			var attrsRemoved = attrsPrev.WhereNotToArray(attrsNext.Contains);
			var attrsChanged = attrsNext.Where(attrsPrev.Contains).WhereToArray(e => nodeNext.GetAttribute(e) != nodePrev.GetAttribute(e));

			list.AddRange(attrsAdded.Select(e => ChgMk.Attr(nodeId, e, nodeNext.GetAttribute(e))));
			list.AddRange(attrsRemoved.Select(e => ChgMk.Attr(nodeId, e, null)));
			list.AddRange(attrsChanged.Select(e => ChgMk.Attr(nodeId, e, nodeNext.GetAttribute(e))));


			// ********
			// * Text *
			// ********
			if (nodeNext.GetOnlyThisNodeText() != nodePrev.GetOnlyThisNodeText())
				list.Add(ChgMk.Text(nodeId, nodeNext.GetOnlyThisNodeText()));


			//if (nodeNext.ClassName != nodePrev.ClassName)
			//	list.Add(PropChange.MkClsChange(nodeId, nodeNext.ClassName));

			var childrenPrev = nodePrev.Children.ToArray();
			var childrenNext = nodeNext.Children.ToArray();
			foreach (var (childPrev, childNext) in childrenPrev.Zip(childrenNext))
				Recurse(childPrev, childNext);
		}

		foreach (var (rootPrev, rootNext) in rootsPrev.Zip(rootsNext))
			Recurse(rootPrev, rootNext);

		return list.ToArray();
	}

	public static void ApplyChgs_In_DomNodeTrees(IElement rootsNext, params Chg[] chgs) => ApplyChgs_In_DomNodeTrees(new[] { rootsNext }, chgs);
	public static void ApplyChgs_In_DomNodeTrees(IElement[] rootsNext, params Chg[] chgs)
	{
		var nodeMap = rootsNext.GetNodeMap();
		foreach (var chg in chgs)
		{
			var node = nodeMap[chg.NodeId];
			switch (chg.Type)
			{
				case ChgType.Attr:
					if (chg.Val != null)
						node.SetAttribute(chg.Name!, chg.Val);
					else
						node.RemoveAttribute(chg.Name!);
					break;

				case ChgType.Prop:
					break;

				case ChgType.Text:
					node.TextContent = chg.Val ?? "";
					break;

				default:
					throw new ArgumentException();
			}
		}
	}


	private static string? GetOnlyThisNodeText(this IElement node) => node.HasTextNodes() switch
	{
		false => null,
		true => node.Text(),
	};
}
*/
