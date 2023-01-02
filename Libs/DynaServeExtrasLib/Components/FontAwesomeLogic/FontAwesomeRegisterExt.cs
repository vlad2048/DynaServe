using DynaServeExtrasLib.Utils;
using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components.FontAwesomeLogic;

public static class FontAwesomeRegisterExt
{
	public static void RegisterFontAwesome(this ServOpt opt)
	{
		opt.Serve(FCat.Font, "fontawesome-webfonts");
		opt.Serve(FCat.Css, "fontawesome-css");
		opt.Serve(FCat.Css, "fontawesome-custom-css");
	}
}