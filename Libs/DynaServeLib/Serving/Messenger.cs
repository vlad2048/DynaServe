using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.Serving.Syncing.Structs;
using PowRxVar;

namespace DynaServeLib.Serving;

class Messenger : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<IServerMsg> whenServerMsg;
	private IObservable<IServerMsg> WhenServerMsg => whenServerMsg.AsObservable();

	public IObservable<Unit> WhenClientConnects { get; }
	public IObservable<IClientMsg> WhenClientMsg { get; }
	public void SendToClient(IServerMsg msg) => whenServerMsg.OnNext(msg);
	public DateTime LastTimePageServed { get; private set; } = DateTime.MinValue;

	public Messenger(Server server, IObservable<Unit> whenPageServed)
	{
		WhenClientConnects = server.WhenWsOpen;
		WhenClientMsg = server.WhenWsMsg.Select(e => SyncJsonUtils.DeserClientMsg(e.Msg));

		whenServerMsg = new Subject<IServerMsg>().D(d);

		whenPageServed.Subscribe(_ => LastTimePageServed = DateTime.Now).D(d);

		WhenServerMsg
			.Synchronize()
			.Subscribe(msg =>
			{
				var str = SyncJsonUtils.SerServerMsg(msg);
				server.WsSend(_ => Task.FromResult(str)).Wait();
			}).D(d);
	}
}