using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Repliers.DynaServe;

class DynaServeReplier : IReplier
{
	private readonly ResourceHolder resourceHolder;
	private readonly Dom dom;

	public DynaServeReplier(ResourceHolder resourceHolder, Dom dom)
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
			var replyTxt = dom.Doc.Fmt();
			var reply = Structs.Reply.MkTxt(ReplyType.Html, replyTxt);
			await reply.WriteToResponse(res);
			dom.LogEvt("sent index.html");
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
