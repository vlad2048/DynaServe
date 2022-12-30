using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Components.EditListLogic.Utils;

static class EditListHtmlNodeExt
{
    public static HtmlNode SetWidthOpt(this HtmlNode node, int? width) => width switch
    {
        null => node,
        int.MaxValue => node.Attr("style", "width: 100%"),
        not null => node.Attr("style", $"width: {width}px"),
    };
}
