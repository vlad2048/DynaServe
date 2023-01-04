using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Nodes;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DomLogic;

record NodsRefs(IElement[] Nods, IRefresher[] AllRefs);
record NodRefs(IElement Nod, IRefresher[] AllRefs);

static class NodeMaker
{
	public static NodsRefs CreateNodes(this IHtmlDocument doc, HtmlNode[] nodes)
	{
		var arr = nodes.SelectToArray(doc.CreateNode);
		return new NodsRefs(
			arr.SelectToArray(e => e.Nod),
			arr.SelectMany(e => e.AllRefs).ToArray()
		);
	}

    public static NodRefs CreateNode(this IHtmlDocument doc, HtmlNode node)
    {
        var refreshers = new List<IRefresher>();

        INode Recurse(HtmlNode n)
        {
            if (n.IsTxt)
                return doc.CreateTextNode(n.Txt!);
            var elt = n.MakeElt(doc);
            refreshers.AddRange(n.Refreshers);
            foreach (var child in n.Children)
            {
                var childElt = Recurse(child);
                elt.AppendNodes(childElt);
            }

            return elt;
        }

        var nodeElt = Recurse(node);

		// Cast works because we don't allow the root node to be a text node
        return new NodRefs((IElement)nodeElt, refreshers.ToArray());
    }
}