using System.Reactive.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Utils;
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
				var bodyFmt = dom.GetPageBodyNodes().Fmt();
				messenger.SendToClient(ServerMsg.MkFullUpdate(bodyFmt));
			}).D(d);

		messenger.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.ReqScriptsSync)
			.Select(e => e.ReqScriptsSyncMsg!)
			.Subscribe(msg =>
			{
				var cssDomLinks = dom.GetCssLinks();
				var cssWebLinks = msg.CssLinks;
				var cssDel = cssWebLinks.WhereNotToArray(cssDomLinks.Contains);
				var cssAdd = cssDomLinks.WhereNotToArray(cssWebLinks.Contains);

				var jsDomLinks = dom.GetJsLinks();
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


	private static IElement[] GetPageBodyNodes(this IHtmlDocument dom) =>
		dom
			.FindDescendant<IHtmlBodyElement>()!
			.Children
			.Where(e => e.Id != ServInst.StatusEltId)
			.ToArray();

	private static string[] GetCssLinks(this IHtmlDocument dom) =>
		dom.GetCssLinkNodes()
			.Select(e => e.Href!.CssNorm())
			.ToArray();

	private static string[] GetJsLinks(this IHtmlDocument dom) =>
		dom.GetJsLinkNodes()
			.Select(e => e.Source!.CssNorm())
			.ToArray();

	private static IHtmlLinkElement[] GetCssLinkNodes(this IHtmlDocument dom) =>
		dom
			.FindDescendant<IHtmlHeadElement>()!
			.Children
			.FilterCssLinks()
			.ToArray();

	private static IHtmlScriptElement[] GetJsLinkNodes(this IHtmlDocument dom) =>
		dom
			.FindDescendant<IHtmlHeadElement>()!
			.Children
			.FilterJsLinks()
			.ToArray();
}
