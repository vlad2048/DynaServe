using DynaServeLib.Serving.Structs;

namespace DynaServeLib.Serving.Repliers;

public interface IReplier
{
	Task<bool> Reply(ReqRes reqRes);
}