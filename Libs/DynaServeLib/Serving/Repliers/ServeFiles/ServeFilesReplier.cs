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

		var mayData = await fileServer.TryGetContent(url);
		if (mayData.IsSome(out var data))
		{
			await data.WriteToResponse(res);
			return true;
		}

		return false;
	}
}