using DynaServeLib.Gizmos;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Utils;

namespace DynaServeLib;

static class ServRegisterExt
{
	public static void Register_WebsocketScripts(this ServOpt opt)
	{
		opt.ServeEmbedded("websockets-utils.js",
			("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port))
		);
		opt.ServeEmbedded("websockets.js",
			("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)),
			("{{StatusEltId}}", ServInst.StatusEltId)
		);

		// TODO: live reloading is not working
		//   - I don't think it can work for the websocket connection itself
		//   -> I need to try separating it out better to check if I can live reload the rest
		// opt.Serve(FCat.Js, "_embedded");
		opt.ServeEmbedded("websockets.css",
			("StatusEltClsAuto", ServInst.StatusEltClsAuto),
			("StatusEltClsManual", ServInst.StatusEltClsManual)
		);
		if (!opt.PlaceWebSocketsHtmlManually)
			opt.AddEmbeddedHtml("websockets.html",
				("{{StatusEltId}}", ServInst.StatusEltId),
				("{{StatusEltClsAuto}}", ServInst.StatusEltClsAuto)
			);
	}

	public static void Register_VersionDisplayer(this ServOpt opt)
	{
		if (!opt.ShowDynaServLibVersion) return;
		var version = DynaServVerDisplayer.GetVer();
		opt.ServeEmbedded("dynaservver.css",
			("DynaServVerCls", ServInst.DynaServVerCls)
		);
		opt.AddEmbeddedHtml("dynaservver.html",
			("{{DynaServVerCls}}", ServInst.DynaServVerCls),
			("{{DynaServVerVersion}}", $"{version}")
		);
	}
}