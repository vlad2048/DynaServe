﻿using AngleSharp.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Serving.Syncing.Structs;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DiffLogic;

static class DiffAlgo
{
	public static bool Are_DomNodeTrees_StructurallyIdentical(IElement[] rootsPrev, IElement[] rootsNext) =>
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
		var map = new Dictionary<string, string>();
		void Recurse(IElement nodePrev, IElement nodeNext)
		{
			map[nodeNext.Id!] = nodePrev.Id!;
			nodeNext.Id = nodePrev.Id;
			var childrenPrev = nodePrev.Children.ToArray();
			var childrenNext = nodeNext.Children.ToArray();
			foreach (var (childPrev, childNext) in childrenPrev.Zip(childrenNext))
				Recurse(childPrev, childNext);
		}
		foreach (var (childPrev, childNext) in rootsPrev.Zip(rootsNext))
			Recurse(childPrev, childNext);

		return refreshersNext.SelectToArray(e => e.CloneWithId(map[e.NodeId]));
	}



	public static PropChange[] ComputePropChanges_In_StructurallyIdentical_DomNodesTree(IElement[] rootsPrev, IElement[] rootsNext)
	{
		var list = new List<PropChange>();

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

			list.AddRange(attrsAdded.Select(e => PropChange.MkAttrChange(nodeId, e, nodeNext.GetAttribute(e))));
			list.AddRange(attrsRemoved.Select(e => PropChange.MkAttrChange(nodeId, e, null)));
			list.AddRange(attrsChanged.Select(e => PropChange.MkAttrChange(nodeId, e, nodeNext.GetAttribute(e))));


			// ********
			// * Text *
			// ********
			if (nodeNext.GetOnlyThisNodeText() != nodePrev.GetOnlyThisNodeText())
				list.Add(PropChange.MkTextChange(nodeId, nodeNext.GetOnlyThisNodeText()));


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

	public static void ApplyPropChanges_In_DomNodeTrees(IElement[] rootsNext, PropChange[] propChanges)
	{
		var nodeMap = rootsNext.GetNodeMap();
		foreach (var chg in propChanges)
		{
			var node = nodeMap[chg.NodeId];
			switch (chg.Type)
			{
				case PropChangeType.Attr:
					if (chg.AttrVal != null)
						node.SetAttribute(chg.AttrName!, chg.AttrVal);
					else
						node.RemoveAttribute(chg.AttrName!);
					break;
				case PropChangeType.Text:
					node.TextContent = chg.TextVal ?? "";
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