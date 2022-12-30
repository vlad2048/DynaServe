using System.Reactive.Linq;
using DynaServeLib;
using PowMaybe;
using PowRxVar;
using DynaServeLib.Nodes;
using DynaServeExtrasLib.Components.DlgInputLogic.Comps;

namespace DynaServeExtrasLib.Components.DlgInputLogic;

public static class DlgInput
{
    public static async Task<Maybe<IDlgReader>> Make(string title, Action<IDlgSetup> setupFun)
    {
        var d = new Disp();
        var slim = new SemaphoreSlim(0).D(d);
        using var dlgSetup = new DlgSetup();
        setupFun(dlgSetup);
        var edits = dlgSetup.Edits;
        var valMap = new ValMap(edits).D(d);
        var resultReader = May.None<IDlgReader>();
        var reader = new DlgReader(valMap);

        void ExecFinish()
        {
            slim.Release();
            d.Dispose();
        }

        void OnClick_Cancel()
        {
            resultReader = May.None<IDlgReader>();
            ExecFinish();
        }


        void OnClick_OK()
        {
            if (dlgSetup.ValidFun != null && !dlgSetup.ValidFun(reader)) throw new ArgumentException();
            resultReader = May.Some<IDlgReader>(reader);
            ExecFinish();
        }



        Serv.AddNodeToBody(
            TDlg("dlginput")
                .Attr("onkeyup", "{ if (event.keyCode === 13) { const elt = document.querySelector('.dlginput-btn-ok'); if (!!elt) elt.click(); } } ")
                .Wrap(Div().Wrap(

                    THeader().Txt(title),

                    TMain().Wrap(edits.Select((edit, editIdx) =>
                        Div("dlginput-edit").Wrap(
                            Div("dlginput-edit-label").Txt(edit.Label),
                            edit.MakeUI(val => valMap.SetVal(edit.Key, val), editIdx == 0)
                        )
                    )),

                    TFooter().Wrap(
                        Div().Wrap(
                            Btn("Cancel", OnClick_Cancel).Cls("dlginput-btn-cancel")
                        ),
                        Div().Wrap(
                            Btn("OK", OnClick_OK).Cls("dlginput-btn-ok")
                                // ReSharper disable AccessToDisposedClosure
                                .EnableWhen(valMap.WhenChanged.Select(_ => IsFinishedAndValid(valMap, dlgSetup.ValidFun, reader)))
                        // ReSharper restore AccessToDisposedClosure
                        )
                    )
                ))
        ).D(d);

        await slim.WaitAsync();
        return resultReader;
    }

    private static bool IsFinishedAndValid(ValMap valMap, Func<IDlgReader, bool>? validFun, IDlgReader reader) =>
        valMap.IsFinished() &&
        (
            validFun == null ||
            validFun(reader)
        );
}