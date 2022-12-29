using AngleSharp.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic;

static class DomExt
{
	public static IElement GetById(this Dom dom, string id) => dom.Doc.GetElementById(id) ?? throw new ArgumentException();

	public static void RemoveAllChildren(this IElement elt)
	{
		var children = elt.Children.ToArray();
		foreach (var child in children)
			elt.RemoveChild(child);
	}

	public static void AppendChildren(this IElement node, IEnumerable<IElement> children) =>
		children.ForEach(child => node.AppendChild(child));

	public static (IElement[], string[]) GetChildrenAndTheirRefresherKeys(this IElement node) =>
		(
			node.Children.ToArray(),
			node.GetAllChildrenIds(false)
		);

	public static Dictionary<string, IElement> GetNodeMap(this IElement[] roots)
	{
		var map = new Dictionary<string, IElement>();
		void Recurse(IElement node)
		{
			if (node.Id == null) throw new ArgumentException();
			map[node.Id] = node;
			foreach (var child in node.Children)
				Recurse(child);
		}
		foreach (var root in roots)
			Recurse(root);
		return map;
	}


	public static string[] GetAllChildrenIds(this IElement elt, bool includeRoot)
	{
		var list = new List<string>();

		if (includeRoot)
			list.Add(elt.Id);

		void Recurse(IElement e)
		{
			list.Add(e.Id);
			foreach (var ec in e.Children)
				Recurse(ec);
		}

		foreach (var eltChild in elt.Children)
			Recurse(eltChild);

		return list.ToArray();
	}



	/*private static IElement[] GetAllChildrenRecursively(this IElement node)
	{
		var list = new List<IElement>();
		void Recurse(IElement n)
		{
			if (n.Id != null)
				list.Add(n);
			foreach (var child in n.Children)
				Recurse(child);
		}

		foreach (var child in node.Children)
			Recurse(child);
		return list.ToArray();
	}*/
}