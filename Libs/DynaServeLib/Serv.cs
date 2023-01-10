using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Security;
using DynaServeLib.Serving;
using DynaServeLib.Serving.FileServing;
using DynaServeLib.Serving.Repliers.ServeFiles;
using DynaServeLib.Serving.Repliers.ServePage;
using DynaServeLib.Serving.Syncing;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using PowRxVar;

namespace DynaServeLib;

class ServInst : IDisposable
{
	internal const string StatusEltId = "syncserv-status";
	internal const string StatusEltClsAuto = "syncserv-status-auto";
	internal const string StatusEltClsManual = "syncserv-status-manual";
	internal const string DynaServVerCls = "DynaServVerCls";

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Server server;
	public readonly FileServer fileServer;
	private readonly ISubject<IDomEvt> whenDomEvt;

	public Messenger Messenger { get; }
	public ServOpt Opt { get; }
	public IHtmlDocument Dom { get;  }
	public DomOps DomOps { get; }
	public IObservable<IDomEvt> WhenDomEvt => whenDomEvt.AsObservable();
	public void SignalDomEvt(IDomEvt evt) => whenDomEvt.OnNext(evt);




	public ServInst(Action<ServOpt>? optFun, HtmlNode[] rootNodes)
	{
		Opt = ServOpt.Build(optFun);
		SecurityChecker.CheckPort(Opt.CheckSecurity, Opt.Port);

		Opt.Register_WebsocketScripts();
		Opt.Register_VersionDisplayer();

		server = new Server(Opt.Port).D(d);
		Messenger = new Messenger(server).D(d);
		Dom = DomCreator.Create(Opt.ExtraHtmlNodes).D(d);
		DomOps = new DomOps(Dom, Opt.Logr, SignalDomEvt, Messenger).D(d);
		whenDomEvt = new Subject<IDomEvt>().D(d);
		DomEvtActioner.Setup(WhenDomEvt, DomOps).D(d);
		fileServer = new FileServer(DomOps, Opt.Logr, Opt.Mounts).D(d);

		DomOps.AddInitialNodes(rootNodes);
		server.AddRepliers(
			new ServePageReplier(Dom),
			new ServeFilesReplier(fileServer)
		);
		server.AddRepliers(Opt.Repliers);
		Syncer.Setup(Dom, Messenger).D(d);
		ServSetupUtils.HookClientUserMessages(Messenger, Opt).D(d);

		//Messenger.WhenClientConnects.Subscribe(_ => Opt.Logr.Log("[WebSockets] ClientConnected")).D(d);
		//Messenger.WhenClientMsg.Subscribe(msg => Opt.Logr.Log($"[WebSockets] Msg:{msg.Type}")).D(d);

		//Messenger.WhenClientMsg.Subscribe(evt => Opt.Logr.LogTransition($"WsEvt[{evt.GetType().Name}] {evt}", Dom.FmtBody())).D(d);

		Opt.Logr.Log("Serv.new() finished");
	}

	public void Start()
	{
		fileServer.Start();
		server.Start();
		Opt.Logr.Log($"Serv.Start() finished ({UrlUtils.GetLocalLink(Opt.Port)})");
	}
}


public static class Serv
{
	private static Disp? trackD;
	private static readonly BehaviorSubject<bool> isStarted = new(false);

	internal static ServInst? I { get; private set; }
	internal static bool IsStarted => isStarted.Value;

	public static void Start(params HtmlNode[] rootNodes) => Start(null, rootNodes);
	public static IDisposable Start(Action<ServOpt>? optFun, params HtmlNode[] rootNodes)
	{
		isStarted.OnNext(false);
		trackD?.Dispose();
		var d = trackD = new Disp();

		I = new ServInst(optFun, rootNodes).D(d);
		I.Start();
		isStarted.OnNext(true);

		return d;
	}

	public static IObservable<IClientMsg> WhenMsg => isStarted
		.Select(on => on switch
		{
			true => I!.Messenger.WhenClientMsg,
			false => Observable.Never<IClientMsg>()
		})
		.Switch();


	public static void Send(IServerMsg msg)
	{
		if (I == null) throw new ArgumentException("Server not ready -> cannot send messages to the frontend");
		I.Messenger.SendToClient(msg);
	}




	public static HtmlNode StatusEltManual => new HtmlNode("div").Id(ServInst.StatusEltId).Cls(ServInst.StatusEltClsManual);
	public static IDisposable AddNodeToBody(HtmlNode node)
	{
		if (I == null) throw new ArgumentException("Cannot call AddNodeToBody before the server is started");
		var d = new Disp();
		// TODO: con't access that var like this
		node.Id ??= I.DomOps.GetNextNodeId();
		I.SignalDomEvt(new AddBodyNodeDomEvt(node));
		Disposable.Create(() =>
		{
			I.SignalDomEvt(new RemoveBodyNodeDomEvt(node.Id ?? throw new ArgumentException()));
		}).D(d);
		return d;
	}
}
