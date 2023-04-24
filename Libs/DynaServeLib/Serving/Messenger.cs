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

	public Messenger(Server server)
	{
		WhenClientConnects = server.WhenWsOpen;
		WhenClientMsg = server.WhenWsMsg.Select(e => SyncJsonUtils.DeserClientMsg(e.Msg));

		whenServerMsg = new Subject<IServerMsg>().D(d);

		/*WhenServerMsg
			.Synchronize()
			.SelectMany(msg => Observable.FromAsync(async () =>
			{
				try
				{
					var str = SyncJsonUtils.SerServerMsg(msg);
					await server.WsSend(_ => Task.FromResult(str));
				}
				catch (Exception ex)
				{
					L($"Exception sending to the client: {ex}");
				}
			}))
			.Subscribe().D(d);*/

		WhenServerMsg
			.Synchronize()
			.Subscribe(msg =>
			{
				try
				{
					var str = SyncJsonUtils.SerServerMsg(msg);
					server.WsSend(_ => Task.FromResult(str)).Wait();
				}
				catch (Exception ex)
				{
					L($"Exception sending to the client (subscribe): {ex}");
				}
			}).D(d);
	}

	private static void L(string s) => Console.WriteLine(s);
}