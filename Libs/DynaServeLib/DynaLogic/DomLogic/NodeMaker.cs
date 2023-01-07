using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Nodes;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DomLogic;

record NodRef(IElement[] Nods, RefreshMap RefreshMap);

static class NodeMaker
{
	public static NodRef Create(this HtmlNode node, IHtmlDocument doc) =>
		new[] { node }.Create(doc);

	public static NodRef Create(this HtmlNode[] nodes, IHtmlDocument doc)
	{
		var arr = nodes.SelectToArray(node => node.CreateNode(doc));
		return new NodRef(
			arr.SelectToArray(e => e.Item1),
			arr.Select(e => e.Item2).MergeEnsure()
		);
	}


    private static (IElement, RefreshMap) CreateNode(this HtmlNode node, IHtmlDocument doc)
    {
        var refreshMap = new Dictionary<IElement, IRefresher[]>();

        INode Recurse(HtmlNode n)
        {
            if (n.IsTxt)
                return doc.CreateTextNode(n.Txt!);
            var elt = n.MakeElt(doc);
			if (n.Refreshers.Any())
				refreshMap[elt] = n.Refreshers;
            foreach (var child in n.Children)
            {
                var childElt = Recurse(child);
                elt.AppendNodes(childElt);
            }

            return elt;
        }

        var nodeElt = Recurse(node);

		// Cast works because we don't allow the root node to be a text node
        return ((IElement)nodeElt, refreshMap);
    }
}