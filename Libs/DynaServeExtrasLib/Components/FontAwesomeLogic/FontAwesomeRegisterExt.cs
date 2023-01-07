using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components.FontAwesomeLogic;

public static class FontAwesomeRegisterExt
{
	public static void RegisterFontAwesome(this ServOpt opt)
	{
		opt.Serve(FCat.Font, "fontawesome-webfonts");
		//opt.Serve(FCat.Css, "fontawesome-css");
		//opt.Serve(FCat.Css, "fontawesome-custom-css");
		var ass = typeof(FontAwesomeRegisterExt).Assembly;
		//opt.ServeEmbedded("fa-brands-400.woff2", ass);
		//opt.ServeEmbedded("fa-regular-400.woff2", ass);
		//opt.ServeEmbedded("fa-solid-900.woff2", ass);
		opt.ServeEmbedded("brands.css", ass);
		opt.ServeEmbedded("fontawesome.css", ass);
		opt.ServeEmbedded("regular.css", ass);
		opt.ServeEmbedded("solid.css", ass);
		opt.ServeEmbedded("fontawesome-ctrls.css", ass);
	}
}