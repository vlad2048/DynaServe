using DynaServeExtrasLib.Components._ThemeLogic;
using DynaServeExtrasLib.Utils;
using DynaServeLib;

namespace DynaServeExtrasLib.Components.DlgInputLogic;

public static class DlgInputRegisterExt
{
    public static void RegisterDlgInput(this ServOpt opt)
    {
	    opt.RegisterTheme();
        opt.AddDynaServeExtraCssFolder("dlg-input");
    }
}