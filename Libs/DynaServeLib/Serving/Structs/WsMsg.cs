using System.Net.WebSockets;

namespace DynaServeLib.Serving.Structs;

public record WsMsg(WebSocket Socket, string Msg);