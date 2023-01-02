using AngleSharp.Html.Dom;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Repliers.ServePage;

class ServePageReplier : IReplier
{
	private readonly IHtmlDocument dom;

	public ServePageReplier(IHtmlDocument dom)
	{
		this.dom = dom;
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
			return true;
		}

		return false;
	}
}
