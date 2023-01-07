using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Logging;
using PowRxVar;
using PowTrees.Algorithms;
using System.Text;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic;

static class DomOpsUtils
{
	public static Dictionary<IElement, IElement> GetBackMap(
		IElement rootPrev,
		IElement rootNext
	)
	{
		var treePrev = rootPrev.ToTree().TreeOfType<IElement>();
		var treeNext = rootNext.ToTree().TreeOfType<IElement>();
		return treePrev.ZipTree(treeNext)
			.ToDictionary(
				t => t.V.Item2,
				t => t.V.Item1
			);
	}

	public static void FlashRefreshers(
		IElement root, RefreshMap refMap,
		Dictionary<IElement, IElement> backMap,
		DomOps domOps
	) =>
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
				var refD = @ref.Activate(node, domOps);
				refD.Dispose();
			}
		});

	public static void LogState(
		string transitionStr,
		IHtmlDocument dom,
		Dictionary<IElement, Disp> map
	)
	{
		var tree = dom.FindDescendant<IHtmlBodyElement>()!
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
			opt.FormatFun = e => GetNodeStr(e, map);
		});
		LTitle(transitionStr);
		L(treeStr);
		var unvisited = GetUnmountedRefresherNodes(dom, map);
		if (unvisited.Any())
		{
			var unvisitedStr = unvisited.Select(e => e.Id ?? "_").JoinText(",");
			L($" -> {unvisited.Length} nodes: {unvisitedStr}");
		}
		LN();
	}

	private static string GetNodeStr(
		IElement node,
		Dictionary<IElement, Disp> map
	)
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

	private static IElement[] GetUnmountedRefresherNodes(
		IHtmlDocument dom,
		Dictionary<IElement, Disp> map
	)
	{
		var visited = new List<IElement>();
		dom.FindDescendant<IHtmlBodyElement>()!.Recurse<IElement>(e =>
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