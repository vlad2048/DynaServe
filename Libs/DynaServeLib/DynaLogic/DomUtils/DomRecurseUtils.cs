using AngleSharp.Dom;

namespace DynaServeLib.DynaLogic.DomUtils;

static class DomRecurseUtils
{
	public static T[] GetRecursively<T>(this INode root) where T : INode
	{
		var list = new List<T>();
		Recurse(root, node =>
		{
			if (node is T elt)
				list.Add(elt);
		});
		return list.ToArray();
	}

	private static void Recurse(this INode root, Action<INode> action)
	{
		action(root);
		foreach (var child in root.ChildNodes)
			Recurse(child, action);
	}
}