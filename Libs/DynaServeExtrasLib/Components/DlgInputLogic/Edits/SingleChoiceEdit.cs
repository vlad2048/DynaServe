using DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;
using DynaServeExtrasLib.Utils;
using DynaServeLib.Nodes;
using PowMaybe;
using PowRxVar;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Edits;

record SingleChoiceEdit(string Key, string Label, Maybe<string> InitVal, string[] Choices) : EditBase(Key, Label, typeof(string))
{
    public override HtmlNode MakeUI(Action<object> setFun, bool isFirst)
    {
        if (InitVal.IsSome(out var initValVal)) setFun(initValVal);
        var mayVar = Var.Make(InitVal).D(D);
        mayVar
            .WhenSome()
            .Subscribe(val => setFun(val)).D(D);
        return
            Div("dlginput-multiplechoices").Wrap(
                mayVar.ToUnit(),
                () => Choices.Select(choice =>
                    Div().Txt(choice).ClsOnIf("dlginput-multiplechoices-item", mayVar.V.IsSomeAndEqualTo(choice))
                        .OnClick(() => mayVar.V = May.Some(choice))
                )
            );
    }
}