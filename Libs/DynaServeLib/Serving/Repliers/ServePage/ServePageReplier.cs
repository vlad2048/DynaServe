using System.Reactive;
using System.Reactive.Subjects;
using AngleSharp.Html.Dom;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Repliers.ServePage;

class ServePageReplier : IReplier
{
	private readonly IHtmlDocument dom;
	private readonly ISubject<Unit> whenPageServedSig;

	public ServePageReplier(IHtmlDocument dom, ISubject<Unit> whenPageServedSig)
	{
		this.dom = dom;
		this.whenPageServedSig = whenPageServedSig;
	}

	public async Task<bool> Reply(ReqRes reqRes)
	{
		var (req, res) = reqRes;
		var url = req.GetUrl();
		if (url == string.Empty)
			url = "index.html";

		if (url == "index.html")
		{
			var replyTxt = dom.Fmt();
			var reply = Structs.Reply.MkTxt(ReplyType.Html, replyTxt);
			await reply.WriteToResponse(res);
			whenPageServedSig.OnNext(Unit.Default);
			return true;
		}

		return false;
	}
}
