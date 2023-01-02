﻿using DynaServeExtrasLib.Components._ThemeLogic;
using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components.DlgInputLogic;

public static class DlgInputRegisterExt
{
    public static void RegisterDlgInput(this ServOpt opt)
    {
	    opt.RegisterTheme();
		opt.Serve(FCat.Css, "dlg-input");
    }
}