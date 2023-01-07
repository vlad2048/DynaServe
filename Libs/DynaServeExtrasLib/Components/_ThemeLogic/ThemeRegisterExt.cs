using DynaServeExtrasLib.Components.DlgInputLogic;
using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components._ThemeLogic;

public static class ThemeRegisterExt
{
	public static void RegisterTheme(this ServOpt opt)
	{
		//opt.Serve(FCat.Css, "_theme");
		var ass = typeof(ThemeRegisterExt).Assembly;
		opt.ServeEmbedded("0_vars.css", ass);
		opt.ServeEmbedded("1_reset.css", ass);
		opt.ServeEmbedded("2_ctrls.css", ass);
	}
}