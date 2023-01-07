using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.XPath;
using DynaServeLib.Serving.Syncing.Structs;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DiffLogic;

static class DiffAlgo
{
	// ************************************************************
	// * Check if 2 list of DOM nodes are structurally equivalent *
	// ************************************************************
	public static bool Are_DomNodeTrees_StructurallyIdentical(IHtmlCollection<IElement> rootsPrev, IElement[] rootsNext) =>
		Are_DomNodeTrees_StructurallyIdentical(rootsPrev.ToArray(), rootsNext);

	private static bool Are_DomNodeTrees_StructurallyIdentical(IElement[] rootsPrev, IElement[] rootsNext) =>
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


	// ****************************************************************************************
	// * Compute the Txt/Attr differences between 2 structurally equivalent list of DOM nodes *
	// ****************************************************************************************
	public static Chg[] ComputePropChanges_In_StructurallyIdentical_DomNodesTree(
		IElement rootPrev,
		IElement rootNext
	)
	{
		var list = new List<Chg>();

		void Recurse(IElement nodePrev, IElement nodeNext, string xPath, bool isFirst)
		{
			if (!isFirst)
			{
				// ********
				// * Attr *
				// ********
				var attrsPrev = nodePrev.Attributes.Where(e => e.Name != "id").SelectToArray(e => e.Name);
				var attrsNext = nodeNext.Attributes.Where(e => e.Name != "id").SelectToArray(e => e.Name);

				var attrsAdded = attrsNext.WhereNotToArray(attrsPrev.Contains);
				var attrsRemoved = attrsPrev.WhereNotToArray(attrsNext.Contains);
				var attrsChanged = attrsNext.Where(attrsPrev.Contains).WhereToArray(e => nodeNext.GetAttribute(e) != nodePrev.GetAttribute(e));

				list.AddRange(attrsAdded.Select(e => ChgKeyMk.Attr(e).Make(xPath, nodeNext.GetAttribute(e))));
				list.AddRange(attrsRemoved.Select(e => ChgKeyMk.Attr(e).Make(xPath, null)));
				list.AddRange(attrsChanged.Select(e => ChgKeyMk.Attr(e).Make(xPath, nodeNext.GetAttribute(e))));

				// ********
				// * Text *
				// ********
				if (nodeNext.GetOnlyThisNodeText() != nodePrev.GetOnlyThisNodeText())
					list.Add(ChgKeyMk.Text().Make(xPath, nodeNext.GetOnlyThisNodeText()));
			}


			var childrenPrev = nodePrev.Children.ToArray();
			var childrenNext = nodeNext.Children.ToArray();
			var cntMap = new Dictionary<string, int>();
			foreach (var (childPrev, childNext) in childrenPrev.Zip(childrenNext))
			{
				var name = childPrev.TagName.ToLowerInvariant();
				var cnt = cntMap.GetCnt(name);
				var childXPath = $"{xPath}/{name}[{cnt}]";
				Recurse(childPrev, childNext, childXPath, false);
			}
		}

		var rootXPath = $"//*[@id='{rootPrev.Id}']";

		Recurse(rootPrev, rootNext, rootXPath, true);

		return list.ToArray();
	}

	private static int GetCnt(this Dictionary<string, int> map, string name)
	{
		if (!map.ContainsKey(name))
			map[name] = 1;
		return map[name]++;
	}

	private static string? GetOnlyThisNodeText(this IElement node) => node.HasTextNodes() switch
	{
		false => null,
		true => node.Text(),
	};


	// ****************************************************
	// * Apply the list of changes to a list of DOM nodes *
	// ****************************************************
	public static void ApplyChgs_In_Dom(IHtmlDocument doc, params Chg[] chgs)
	{
		var body = doc.FindDescendant<IHtmlBodyElement>()!;

		foreach (var chg in chgs)
		{
			var node = body.SelectSingleNode(chg.NodePath) as IElement ?? throw new ArgumentException($"Cannot find '{chg.NodePath}'");

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
