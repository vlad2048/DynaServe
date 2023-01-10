using System.Reactive;
using System.Reactive.Subjects;
using AngleSharp.Html.Dom;
using DynaServeLib.Serving.FileServing.StructsEnum;
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
			var reply = new Reply(FType.Html, ReplyData.MkString(replyTxt, Array.Empty<(string, string)>()));
			await reply.WriteToResponse(res);
			return true;
		}

		return false;
	}
}
