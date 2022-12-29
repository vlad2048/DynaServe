using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Serving.Syncing.Utils;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.Serving.Syncing;


class Syncer : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<ClientMsg> whenClientMsg;
	private readonly ISubject<ServerMsg> whenServerMsg;

	public IObservable<ClientMsg> WhenClientMsg => whenClientMsg.AsObservable();
	public IObservable<ServerMsg> WhenServerMsg => whenServerMsg.AsObservable();

	public void SendToClient(ServerMsg msg) => whenServerMsg.OnNext(msg);
	private static void L(string s) => Console.WriteLine(s);

	public Syncer(Server server, Dom dom)
	{
		whenClientMsg = new Subject<ClientMsg>().D(d);
		whenServerMsg = new Subject<ServerMsg>().D(d);

		server.WhenWsOpen
			.Subscribe(_ =>
			{
				var bodyFmt = dom.GetPageBodyNodes().Fmt();
				SendToClient(ServerMsg.MkFullUpdate(bodyFmt));
			}).D(d);

		server.WhenWsMsg
			.Select(e => SyncJsonUtils.Deser<ClientMsg>(e.Msg))
			.Subscribe(msg =>
			{
				switch (msg.Type)
				{
					case ClientMsgType.ReqCssSync:
					{
						var domLinks = dom.GetCssLinks();
						var webLinks = msg.CssLinks!;

						static void L(string s) => Console.WriteLine(s);
						static void LogArr(string header, string[] arr)
						{
							L(header);
							L(new string('=', header.Length));
							foreach (var elt in arr) L($"  {elt}");
						}

						//L("ReqCssSync");
						//LogArr("Dom Links", domLinks);
						//LogArr("Web Links", webLinks);

						var remove = webLinks.WhereNotToArray(domLinks.Contains);
						var add = domLinks.WhereNotToArray(webLinks.Contains);

						SendToClient(ServerMsg.MkCssSync(remove, add));

						break;
					}

					case ClientMsgType.ReqFullLog:
					{
						dom.LogFull(msg.Html!);
						break;
					}

					default:
					{
						whenClientMsg.OnNext(msg);
						break;
					}
				}
			}).D(d);

		WhenServerMsg
			.Synchronize()
			.Subscribe(msg =>
			{
				var str = SyncJsonUtils.Ser(msg);
				server.WsSend(_ => Task.FromResult(str)).Wait();
			}).D(d);
	}
}