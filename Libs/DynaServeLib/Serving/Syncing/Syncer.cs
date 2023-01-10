using System.Reactive.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.Serving.Syncing;

static class Syncer
{
	public static IDisposable Setup(IHtmlDocument dom, Messenger messenger)
	{
		var d = new Disp();

		messenger.WhenClientConnects
			.Subscribe(_ =>
			{
				var bodyFmt = dom.GetUserBodyNodes().Fmt();
				var scripts = dom.GetScripts();
				messenger.SendToClient(new FullUpdateServerMsg(bodyFmt, scripts));
			}).D(d);

		/*messenger.WhenClientMsg
			.OfType<ReqScriptsSyncClientMsg>()
			.SubscribeSafe(msg =>
			{
				var cssDomLinks = dom.GetAllCssLinks();
				var cssWebLinks = msg.CssLinks;
				var cssDel = cssWebLinks.WhereNotToArray(cssDomLinks.Contains);
				var cssAdd = cssDomLinks.WhereNotToArray(cssWebLinks.Contains);

				var jsDomLinks = dom.GetAllJsLinks();
				var jsWebLinks = msg.JsLinks;
				var jsDel = jsWebLinks.WhereNotToArray(jsDomLinks.Contains);
				var jsAdd = jsDomLinks.WhereNotToArray(jsWebLinks.Contains);

				messenger.SendToClient(new ReplyScriptsSyncServerMsg(
					cssDel,
					cssAdd,
					jsDel,
					jsAdd
				));
			}).D(d);*/

		return d;
	}

	private static void L(string s) => Console.WriteLine(s);
}
