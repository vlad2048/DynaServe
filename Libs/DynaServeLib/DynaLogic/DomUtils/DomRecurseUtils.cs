using AngleSharp.Dom;

namespace DynaServeLib.DynaLogic.DomUtils;

static class DomRecurseUtils
{
	public static T[] GetRecursively<T>(this INode root) where T : INode
	{
		var list = new List<T>();
		Recurse<T>(root, list.Add);
		return list.ToArray();
	}

	public static void RecurseUnder<T>(this INode root, Action<T> action) where T : INode
	{
		var first = true;
		root.Recurse<T>(elt =>
		{
			if (first)
			{
				first = false;
				return;
			}
			action(elt);
		});
	}

	public static void Recurse<T>(this INode root, Action<T> action) where T : INode
	{
		if (root is T elt)
			action(elt);
		foreach (var child in root.ChildNodes)
			Recurse(child, action);
	}

	public static void Recurse(this INode root, Action<INode> action)
	{
		action(root);
		foreach (var child in root.ChildNodes)
			Recurse(child, action);
	}
}