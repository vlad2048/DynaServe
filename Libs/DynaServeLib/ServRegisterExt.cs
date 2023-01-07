using DynaServeLib.Gizmos;
using DynaServeLib.Utils;

namespace DynaServeLib;

static class ServRegisterExt
{
	public static void Register_WebsocketScripts(this ServOpt opt)
	{
		opt.ServeEmbedded("link-utils.js",
			("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port))
		);
		opt.ServeEmbedded("websockets-utils.js");
		opt.ServeEmbedded("websockets-handlers.js",
			("{{StatusEltId}}", ServInst.StatusEltId)
		);
		opt.ServeEmbedded("websockets.js",
			("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)),
			("{{StatusEltId}}", ServInst.StatusEltId)
		);

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
		opt.ServeFile(
			"dynaservver",
			"dynaservver.css",
			("DynaServVerCls", ServInst.DynaServVerCls)
		);
		opt.AddEmbeddedHtml("dynaservver.html",
			("{{DynaServVerCls}}", ServInst.DynaServVerCls),
			("{{DynaServVerVersion}}", $"{version}")
		);
	}
}