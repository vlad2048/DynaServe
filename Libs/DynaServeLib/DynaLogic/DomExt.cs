using AngleSharp.Dom;
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
}