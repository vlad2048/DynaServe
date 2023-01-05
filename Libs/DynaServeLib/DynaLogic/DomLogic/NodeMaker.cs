using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Nodes;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DomLogic;

record NodsRefs(IElement[] Nods, RefreshMap RefreshMap);
record NodRefs(IElement Nod, RefreshMap RefreshMap);

static class NodeMaker
{
	public static NodsRefs CreateNodes(this HtmlNode[] nodes, IHtmlDocument doc)
	{
		var arr = nodes.SelectToArray(node => node.CreateNode(doc));
		return new NodsRefs(
			arr.SelectToArray(e => e.Nod),
			arr.Select(e => e.RefreshMap).MergeEnsure()
		);
	}

    public static NodRefs CreateNode(this HtmlNode node, IHtmlDocument doc)
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
        return new NodRefs((IElement)nodeElt, refreshMap);
    }
}