using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DynaServeLib.Utils;

namespace DynaServeLib.DynaLogic.Utils;

// TODO: code duplication with CssLinkUtils.cs
static class DomUtils
{
	public static void AddExtraHtmlNodes(
		IHtmlDocument doc,
		IReadOnlyList<string> extraHtmlNodes
	)
	{
		var body = doc.FindDescendant<IHtmlBodyElement>()!;
		var parser = new HtmlParser();
		foreach (var html in extraHtmlNodes)
		{
			var nodes = parser.ParseFragment(html, body);
			body.PrependNodes(nodes.ToArray());
		}
	}

	private const string ImgUrlPrefix = "about:///";
	public static bool IsImgSrcMatchingLink(IHtmlImageElement img, string link)
	{
		var src = img.Source;
		if (src == null) return false;
		if (!src.StartsWith(ImgUrlPrefix)) return false;
		var lnk = src[ImgUrlPrefix.Length..].RemoveQueryParams();
		return lnk == link;
	}

	public static string RemoveImgSrcPrefix(this string? src)
	{
		if (src == null || !src.StartsWith(ImgUrlPrefix)) throw new ArgumentException();
		return src[ImgUrlPrefix.Length..];
	}


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



	/*public static IHtmlElement[] GetAllHtmlElements(this IElement elt)
	{
		var list = new List<IHtmlElement>();

		void Recurse(INode n)
		{
			if (n is IHtmlElement e)
				list.Add(e);
			foreach (var child in n.ChildNodes)
				Recurse(child);
		}

		Recurse(elt);

		return list.ToArray();
	}*/
}