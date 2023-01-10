using DynaServeLib.Gizmos;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib;

static class ServRegisterExt
{
	private const string WebsocketsHtml = """
		<div id="{{StatusEltId}}"
			 class="{{StatusEltClsAuto}}"
		>
			Undef
		</div>
		""";

	public static void Register_WebsocketScripts(this ServOpt opt)
	{
		var substs = new[]
		{
			("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)),
			("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port)),
			("{{StatusEltId}}", ServInst.StatusEltId),
			("{{StatusEltClsAuto}}", ServInst.StatusEltClsAuto),
			("{{StatusEltClsManual}}", ServInst.StatusEltClsManual)
		};

		void AddJs(string name, bool isModule) =>
			opt.ServeFile(name, fopt =>
			{
				fopt.JsFlags = isModule ? MountJsFlags.AddModuleScript : MountJsFlags.ServeOnly;
				fopt.Substs = substs;
			});

		AddJs(@"websockets\dom-utils.js", false);
		AddJs(@"websockets\script-utils.js", false);
		AddJs(@"websockets\websockets-handlers.js", false);
		AddJs(@"websockets\websockets.js", true);
		if (opt.PlaceWebSocketsHtmlManually)
		{
			opt.ServeFile(@"websockets\websockets.css");
			opt.AddHtmlToBody(WebsocketsHtml.ApplySubsts(substs));
		}
	}



	private const string DynaservverHtml = """
		<div class="DynaServVerCls">
			DynaServ ver DynaServVerVersion
		</div>
		""";

	public static void Register_VersionDisplayer(this ServOpt opt)
	{
		if (!opt.ShowDynaServLibVersion) return;
		var version = DynaServVerDisplayer.GetVer();
		var substs = new[]
		{
			("DynaServVerCls", ServInst.DynaServVerCls),
			("DynaServVerVersion", $"{version}")
		};

		opt.ServeFile("dynaservver.css", fopt => fopt.Substs = substs);
		opt.AddHtmlToBody(DynaservverHtml.ApplySubsts(substs));
	}





	/*public static void Register_WebsocketScripts(this ServOpt opt)
	{
		var repls = new[]
		{
			("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)),
			("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port)),
			("{{StatusEltId}}", ServInst.StatusEltId)
		};
		//opt.ServeEmbedded("link-utils.js", repls);
		//opt.ServeEmbedded("websockets-utils.js", repls);
		//opt.ServeEmbedded("websockets-handlers.js", repls);


		opt.ServeFile("websockets", "link-utils.js", repls);
		opt.ServeFile("websockets", "websockets-utils.js", repls);
		opt.ServeFile("websockets", "websockets-handlers.js", repls);
		opt.ServeEmbedded("websockets.js", repls);

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
	}*/
}