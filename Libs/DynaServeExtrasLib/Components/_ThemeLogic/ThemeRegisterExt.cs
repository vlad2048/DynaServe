using DynaServeExtrasLib.Utils;
using DynaServeLib;

namespace DynaServeExtrasLib.Components._ThemeLogic;

public static class ThemeRegisterExt
{
	public static void RegisterTheme(this ServOpt opt)
	{
		opt.AddDynaServeExtraCssFolder("_theme");
	}
}