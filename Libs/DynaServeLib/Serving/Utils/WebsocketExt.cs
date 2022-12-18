using System.Net.WebSockets;
using System.Text;

namespace DynaServeLib.Serving.Utils;

static class WebsocketExt
{
	public static async Task<string> ReadString(this WebSocket ws, ArraySegment<byte> buffer, WebSocketReceiveResult recv, CancellationToken cancelToken)
	{
		using var ms = new MemoryStream();
		await ms.WriteAsync(buffer.Array!, buffer.Offset, recv.Count, cancelToken);
		while (!recv.EndOfMessage)
		{
			recv = await ws.ReceiveAsync(buffer, CancellationToken.None);
			await ms.WriteAsync(buffer.Array!, buffer.Offset, recv.Count, cancelToken);
		}

		ms.Seek(0, SeekOrigin.Begin);
		using var sr = new StreamReader(ms, Encoding.UTF8);
		return await sr.ReadToEndAsync(cancelToken);
	}
}