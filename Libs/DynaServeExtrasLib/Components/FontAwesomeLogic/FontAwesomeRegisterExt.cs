using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components.FontAwesomeLogic;

public static class FontAwesomeRegisterExt
{
	public static void RegisterFontAwesome(this ServOpt opt)
	{
		opt.ServeFolder("fontawesome-webfonts", FCat.Font, fopt =>
		{
			fopt.MountLocation = "fonts";
		});
		opt.ServeFolder("fontawesome-css", FCat.Css, fopt =>
		{
			fopt.MountLocation = "css";
		});
		opt.ServeFolder("fontawesome-custom-css", FCat.Css, fopt =>
		{
			fopt.MountLocation = "css";
		});
	}
}