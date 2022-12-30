using DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;
using DynaServeExtrasLib.Utils;
using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Edits;

record StringEdit(string Key, string Label, string InitVal) : EditBase(Key, Label, typeof(string))
{
    public override HtmlNode MakeUI(Action<object> setFun, bool isFirst)
    {
        setFun(InitVal);
        return new HtmlNode("input")
            .Attr("type", "text")
            .Attr("value", InitVal)
            .AutofocusIfFirst(isFirst)
            .HookArg("input", setFun, "this.value");
    }
}