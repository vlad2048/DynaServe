using AngleSharp.Text;
using DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;
using DynaServeExtrasLib.Utils;
using DynaServeLib.Nodes;
using PowRxVar;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Edits;

record MultipleChoicesEdit(string Key, string Label, string[] InitVal, string[] Choices) : EditBase(Key, Label, typeof(string[]))
{
    public override HtmlNode MakeUI(Action<object> setFun, bool isFirst)
    {
        setFun(InitVal);
        var arrVar = Var.Make(InitVal).D(D);
        arrVar.Subscribe(arr => setFun(arr)).D(D);
        return
            Div("dlginput-multiplechoices").Wrap(
                arrVar.ToUnit(),
                () => Choices.Select(choice =>
                    Div().Txt(choice).ClsOnIf("dlginput-multiplechoices-item", arrVar.V.Contains(choice))
                        .OnClick(() => arrVar.V = arrVar.V.ArrToggle(choice))
                )
            );
    }
}