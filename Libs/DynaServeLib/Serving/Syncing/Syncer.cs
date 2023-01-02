using System.Reactive.Linq;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomUtils;
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
				messenger.SendToClient(ServerMsg.MkFullUpdate(bodyFmt));
			}).D(d);

		messenger.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.ReqScriptsSync)
			.Select(e => e.ReqScriptsSyncMsg!)
			.Subscribe(msg =>
			{
				var cssDomLinks = dom.GetAllCssLinks();
				var cssWebLinks = msg.CssLinks;
				var cssDel = cssWebLinks.WhereNotToArray(cssDomLinks.Contains);
				var cssAdd = cssDomLinks.WhereNotToArray(cssWebLinks.Contains);

				var jsDomLinks = dom.GetAllJsLinks();
				var jsWebLinks = msg.JsLinks;
				var jsDel = jsWebLinks.WhereNotToArray(jsDomLinks.Contains);
				var jsAdd = jsDomLinks.WhereNotToArray(jsWebLinks.Contains);

				messenger.SendToClient(ServerMsg.MkReplyScriptsSync(new ReplyScriptsSyncMsg(
					cssDel,
					cssAdd,
					jsDel,
					jsAdd
				)));
			}).D(d);

		return d;
	}
}
