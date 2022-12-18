using System.Net;

namespace DynaServeLib.Serving.Structs;

public record ReqRes(HttpListenerRequest Req, HttpListenerResponse Res);