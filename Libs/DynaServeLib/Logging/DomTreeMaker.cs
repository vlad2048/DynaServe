using AngleSharp.Dom;
using PowTrees.Algorithms;

namespace DynaServeLib.Logging;

public static class DomTreeMaker
{
	public static TNod<INode> ToTree(this INode root)
	{
		static bool IsNodeRelevant(INode n) => n switch
		{
			IText e => !string.IsNullOrWhiteSpace(e.TextContent),
			_ => true,
		};

		TNod<INode> Recurse(INode node) =>
			Nod.Make(node, node.ChildNodes.Select(Recurse));

		return Recurse(root)
			.Filter(IsNodeRelevant, opt =>
			{
				opt.AlwaysKeepRoot = true;
				opt.Type = TreeFilterType.KeepIfMatchingOnly;
			})
			.Single();
	}

	public static TNod<T> TreeOfType<T>(this TNod<INode> root) where T : IElement =>
		root
			.Filter(e => e is T)
			.Single()
			.Map(e => (T)e);
}