using PowRxVar;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.Security.LogicUrlAcl;
using DynaServeLib.Serving.Repliers;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Serving.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving;

public sealed class Server : IDisposable
{
    private const int WsBufferSize = 64 * 1024;

    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly ISubject<Unit> whenWsOpen;
    private readonly IReadOnlyList<IReplier> repliers;
    private readonly HttpListener listener;
    private readonly ISubject<WsMsg> whenWsMsg;
    private readonly ConcurrentDictionary<WebSocket, WebSocket> sockets = new();

    public IObservable<Unit> WhenWsOpen => whenWsOpen.AsObservable();
    public IObservable<WsMsg> WhenWsMsg => whenWsMsg.AsObservable();

    public Server(int port, IReadOnlyList<IReplier> repliers)
    {
	    whenWsOpen = new Subject<Unit>().D(d);
	    this.repliers = repliers;
        whenWsMsg = new Subject<WsMsg>().D(d);
        listener = new HttpListener();
		Disposable.Create(listener.Stop).D(d);
        listener.Prefixes.Add(UrlAclOps.MkUrl(port));
    }

    public void Start()
    {
        listener.Start();
        listener.BeginGetContext(OnContext, null);
    }

    public async Task WsClose()
    {
	    var arr = sockets.Values.ToArray();
	    foreach (var socket in arr)
	    {
		    if (socket.State == WebSocketState.Open)
		    {
			    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
		    }
	    }
    }

    public async Task WsSend(Func<WebSocket, Task<string>> msgFun)
    {
        var arr = sockets.Values.ToArray();
        foreach (var socket in arr)
        {
	        if (socket.State == WebSocketState.Open)
	        {
		        var msg = await msgFun(socket);
		        var data = msg.ToBytes();
		        await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
	        }
        }
    }


    private void OnContext(IAsyncResult ar)
    {
	    try
	    {
		    var ctx = listener.EndGetContext(ar);
		    listener.BeginGetContext(OnContext, null);
		    Task.Run(async () =>
		    {
			    if (ctx.Request.IsWebSocketRequest)
			    {
				    var cancelToken = CancellationToken.None;
				    WebSocket? ws = null;
				    try
				    {
					    var wsCtx = await ctx.AcceptWebSocketAsync(null).WaitAsync(cancelToken);
					    ws = wsCtx.WebSocket;
					    sockets[ws] = ws;
					    await HandleWs(ws, cancelToken);
				    }
				    catch (OperationCanceledException)
				    {
				    }
				    finally
				    {
					    if (ws != null)
						    sockets.Remove(ws, out _);
				    }
			    }
			    else
			    {
				    var req = ctx.Request;
				    var res = ctx.Response;
				    var reqRes = new ReqRes(req, res);
					
				    try
				    {
					    foreach (var replier in repliers)
					    {
						    var handled = await replier.Reply(reqRes);
						    if (handled)
							    break;
					    }
				    }
				    // The specified network name is no longer available.
				    catch (HttpListenerException ex) when (ex.ErrorCode == 64)
				    {
				    }

				    try
				    {
					    res.Close();
				    }
					// Cannot close stream until all bytes are written.
				    catch (InvalidOperationException ex) when (ex.HResult == -2146233079)
				    {
				    }
			    }
		    });
	    }
		// The I/O operation has been aborted because of either a thread exit or an application request.
	    catch (HttpListenerException ex) when (ex.ErrorCode == 995)
	    {
	    }
	    // The specified network name is no longer available.
	    catch (HttpListenerException ex) when (ex.ErrorCode == 64)
	    {
	    }
    }



    private async Task HandleWs(WebSocket ws, CancellationToken cancelToken)
    {
	    try
	    {
			whenWsOpen.OnNext(Unit.Default);

		    var buffer = new byte[WsBufferSize];
		    while (ws.State == WebSocketState.Open)
		    {
			    var bufferSeg = new ArraySegment<byte>(buffer);
			    var recv = await ws.ReceiveAsync(bufferSeg, cancelToken);
			    cancelToken.ThrowIfCancellationRequested();
			    switch (recv.MessageType)
			    {
				    case WebSocketMessageType.Close:
					    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancelToken);
					    break;

				    case WebSocketMessageType.Binary:
					    throw new ArgumentException("Cannot handle binary websocket messages");
				    case WebSocketMessageType.Text:
					    var str = await ws.ReadString(buffer, recv, cancelToken);
					    var wsMsg = new WsMsg(ws, str);
					    whenWsMsg.OnNext(wsMsg);
					    break;

				    default:
					    throw new ArgumentException($"Invalid MessageType: {recv.MessageType}");
			    }
		    }
	    }
	    // The remote party closed the WebSocket connection without completing the close handshake.
	    catch (WebSocketException ex) when (ex.ErrorCode == 0)
	    {
	    }
    }
}
