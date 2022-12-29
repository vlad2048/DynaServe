using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Nodes;

namespace DynaServeLib.DynaLogic.Utils;

static class NodeMaker
{
	public static (IElement[], IRefresher[]) CreateNodes(this IHtmlDocument doc, HtmlNode[] nodes, IDomTweaker[] domTweakers)
	{
		var list = new List<IElement>();
		var refreshers = new List<IRefresher>();
		foreach (var node in nodes)
		{
			var (nodeElt, nodeRefreshers) = doc.CreateNode(node, domTweakers);
			list.Add(nodeElt);
			refreshers.AddRange(nodeRefreshers);
		}
		return (list.ToArray(), refreshers.ToArray());
	}

	public static (IElement, IRefresher[]) CreateNode(this IHtmlDocument doc, HtmlNode node, IDomTweaker[] domTweakers)
	{
		var refreshers = new List<IRefresher>();

		INode Recurse(HtmlNode n)
		{
			if (n.IsTxt)
				return doc.CreateTextNode(n.Txt!);
			var elt = n.MakeElt(doc, domTweakers);
			refreshers.AddRange(n.Refreshers);
			foreach (var child in n.Children)
			{
				var childElt = Recurse(child);
				elt.AppendNodes(childElt);
			}

			return elt;
		}

		var nodeElt = Recurse(node);

		return ((IElement)nodeElt, refreshers.ToArray());
	}
}