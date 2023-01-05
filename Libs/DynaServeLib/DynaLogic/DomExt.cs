using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic;

static class DomExt
{
	public static string GetIdEnsure(this IElement node) => node.Id ?? throw new ArgumentException();

	public static IElement GetById(this IHtmlDocument dom, string id) => dom.GetElementById(id) ?? throw new ArgumentException($"Cannot find node {id}");

	public static void RemoveAllChildren(this IElement elt)
	{
		var children = elt.Children.ToArray();
		foreach (var child in children)
			child.Remove();
			//elt.RemoveChild(child);
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
			//if (node.Id == null) throw new ArgumentException();
			if (node.Id != null)
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
		if (elt.Id == null) throw new ArgumentException();
		var list = new List<string>();

		if (includeRoot)
			list.Add(elt.Id);

		void Recurse(IElement e)
		{
			if (e.Id == null) throw new ArgumentException();
			list.Add(e.Id);
			foreach (var ec in e.Children)
				Recurse(ec);
		}

		foreach (var eltChild in elt.Children)
			Recurse(eltChild);

		return list.ToArray();
	}
}