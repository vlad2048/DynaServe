using DynaServeExtrasLib.Utils;
using DynaServeLib;

namespace DynaServeExtrasLib.Components.FontAwesomeLogic;

public static class FontAwesomeRegisterExt
{
	public static void RegisterFontAwesome(this ServOpt opt)
	{
		opt.AddDynaServeEmbeddedFontFolder("fontawesome-webfonts");
		opt.AddDynaServeExtraCssFolder("fontawesome-css");
		opt.AddDynaServeExtraCssFolder("fontawesome-custom-css");
	}
}