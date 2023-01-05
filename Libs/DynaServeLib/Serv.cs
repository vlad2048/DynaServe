using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Intrinsics.X86;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using DynaServeLib.Security;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Debugging;
using DynaServeLib.Serving.FileServing;
using DynaServeLib.Serving.Repliers.ServeFiles;
using DynaServeLib.Serving.Repliers.ServePage;
using DynaServeLib.Serving.Syncing;
using DynaServeLib.Utils;
using PowRxVar;

namespace DynaServeLib;

public record ClientUserMsg(string Type, string Arg);

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
	public ServDbg ServDbg { get; }
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
		fileServer = new FileServer(
			DomOps,
			Opt.ServNfos,
			Opt.ScssOutputFolder,
			Opt.SlnFolders,
			Opt.Logr
		).D(d);
		ServDbg = new ServDbg(DomOps, Messenger).D(d);

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

	internal static ServInst? I { get; private set; }
	internal static bool IsStarted { get; private set; }

	public static void Start(params HtmlNode[] rootNodes) => Start(null, rootNodes);
	public static IDisposable Start(Action<ServOpt>? optFun, params HtmlNode[] rootNodes)
	{
		IsStarted = false;
		trackD?.Dispose();
		var d = trackD = new Disp();

		I = new ServInst(optFun, rootNodes).D(d);
		I.Start();
		IsStarted = true;

		return d;
	}
	public static ServDbg Dbg => I!.ServDbg;
	public static HtmlNode StatusEltManual => new HtmlNode("div").Id(ServInst.StatusEltId).Cls(ServInst.StatusEltClsManual);
	public static IDisposable AddNodeToBody(HtmlNode node)
	{
		if (I == null) throw new ArgumentException("Cannot call AddNodeToBody before the server is started");
		var d = new Disp();
		// TODO: con't access that var like this
		node.Id ??= $"id-{I.DomOps.idCnt++}";
		I.SignalDomEvt(new AddBodyNodeDomEvt(node));
		Disposable.Create(() =>
		{
			I.SignalDomEvt(new RemoveBodyNodeDomEvt(node.Id ?? throw new ArgumentException()));
		}).D(d);
		return d;
	}
}
