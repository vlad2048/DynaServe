using AngleSharp.Html.Dom;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Repliers.DynaServe;

class DynaServeReplier : IReplier
{
	private readonly ResourceHolder resourceHolder;
	private readonly IHtmlDocument dom;

	public DynaServeReplier(ResourceHolder resourceHolder, IHtmlDocument dom)
	{
		this.resourceHolder = resourceHolder;
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

		if (resourceHolder.TryGetContent(url, out var resReply))
		{
			await resReply.WriteToResponse(res);
			return true;
		}

		return false;
	}
}
