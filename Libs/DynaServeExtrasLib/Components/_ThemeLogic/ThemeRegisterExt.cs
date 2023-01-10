using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components._ThemeLogic;

public static class ThemeRegisterExt
{
	public static void RegisterTheme(this ServOpt opt)
	{
		opt.ServeFolder("_theme", FCat.Css);
	}
}