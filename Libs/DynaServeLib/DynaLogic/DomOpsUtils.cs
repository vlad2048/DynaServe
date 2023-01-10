using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Logging;
using PowRxVar;
using PowTrees.Algorithms;
using System.Text;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic;


class RefStats
{
	public List<string> Added { get; } = new();
	public List<string> Removed { get; } = new();

	public void Clear()
	{
		Added.Clear();
		Removed.Clear();
	}
}


static class DomOpsUtils
{
	public static void LogState(
		string transitionStr,
		IHtmlDocument dom,
		Dictionary<IElement, Disp> map,
		RefStats refStats
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

		void ShowRef(string title, List<string> list)
		{
			var str = (list.Count == 0) switch
			{
				true => "_",
				false => list.JoinText(", ")
			};
			L($"  {title} {str}");
		}
		L("Refreshers");
		ShowRef("Added  : ", refStats.Added);
		ShowRef("Removed: ", refStats.Removed);

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