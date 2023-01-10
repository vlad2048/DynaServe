using DynaServeLib.Serving.FileServing;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using PowMaybe;

namespace DynaServeLib.Serving.Repliers.ServeFiles;

class ServeFilesReplier : IReplier
{
	private readonly FileServer fileServer;

	public ServeFilesReplier(FileServer fileServer)
	{
		this.fileServer = fileServer;
	}

	public async Task<bool> Reply(ReqRes reqRes)
	{
		var (req, res) = reqRes;
		var url = req.GetUrl();

		var mayReply = await fileServer.TryGetReply(url);
		if (mayReply.IsSome(out var reply))
		{
			await reply.WriteToResponse(res);
			return true;
		}

		return false;
	}
}