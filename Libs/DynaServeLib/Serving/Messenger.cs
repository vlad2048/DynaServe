using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Serving.Syncing.Utils;
using PowRxVar;

namespace DynaServeLib.Serving;

class Messenger : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<ServerMsg> whenServerMsg;
	private IObservable<ServerMsg> WhenServerMsg => whenServerMsg.AsObservable();

	public IObservable<Unit> WhenClientConnects { get; }
	public IObservable<ClientMsg> WhenClientMsg { get; }
	public void SendToClient(ServerMsg msg) => whenServerMsg.OnNext(msg);

	public Messenger(Server server)
	{
		WhenClientConnects = server.WhenWsOpen;
		WhenClientMsg = server.WhenWsMsg.Select(e => SyncJsonUtils.Deser<ClientMsg>(e.Msg));

		whenServerMsg = new Subject<ServerMsg>().D(d);

		WhenServerMsg
			.Synchronize()
			.Subscribe(msg =>
			{
				var str = SyncJsonUtils.Ser(msg);
				server.WsSend(_ => Task.FromResult(str)).Wait();
			}).D(d);
	}
}