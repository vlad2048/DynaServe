using System.Net;
using DynaServeLib.Serving.Repliers.VideoForwarder.Structs;
using DynaServeLib.Serving.Repliers.VideoForwarder.Utils;

namespace DynaServeLib.Serving.Repliers.VideoForwarder.Events;

public interface IVidFwEvt
{
	int ReqId { get; }
}

/*public record MsgVidFwEvt(
	int ReqId,
	string Msg
) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] '{Msg}'";
}*/

public record StartVidFwEvt(
	int ReqId,
	string Name,
	VidSrc Src,
	RngReq RngReq
) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] Start {Name}  rng:{RngReq}";
}

public record HeadersVidFwEvt(
	int ReqId,
	RngRes RngRes,
	long Length,
	TimeSpan Time
) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] Headers  rng:{RngRes}  lng:{Length.FmtSize()} time:{(int)Time.TotalMilliseconds}ms";
}

/*public record ChunkVidFwEvt(
	int ReqId,
	long Length,
	TimeSpan Time
) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] Chunk lng:{Length.FmtSize()} time:{(int)Time.TotalMilliseconds}ms";
}*/

public record ErrHeadersVidFwEvt(int ReqId, HttpStatusCode StatusCode) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] ERR Headers {StatusCode}";
}

public record ErrTimeoutVidFwEvt(int ReqId) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] ERR Timeout";
}

public record ErrClientClosedConnection(int ReqId) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] ERR ClientClosedConnection";
}

/*public record ErrNetworkUnavailableVidFwEvt(int ReqId) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] ERR NetworkUnavailable";
}*/

public record ErrUnexpectedException(int ReqId, Exception Ex) : IVidFwEvt
{
	public override string ToString() => $"[{ReqId}] ERR Unexpected '{Ex.Message}' ({Ex.GetType().Name})";
}

