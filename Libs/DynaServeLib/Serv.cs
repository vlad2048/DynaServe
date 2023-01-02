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
	public DomOps DomOps { get;  }
	public ServDbg ServDbg { get; }
	public IObservable<IDomEvt> WhenDomEvt => whenDomEvt.AsObservable();
	public void SignalDomEvt(IDomEvt evt) => whenDomEvt.OnNext(evt);

	public ServInst(Action<ServOpt>? optFun, HtmlNode[] rootNodes)
	{
		Opt = ServOpt.Build(optFun);
		SecurityChecker.CheckPort(Opt.CheckSecurity, Opt.Port);
		whenDomEvt = new Subject<IDomEvt>().D(d);

		Opt.Register_WebsocketScripts();
		Opt.Register_VersionDisplayer();

		server = new Server(Opt.Port).D(d);
		Messenger = new Messenger(server).D(d);
		Dom = DomCreator.Create(Opt.ExtraHtmlNodes).D(d);
		DomOps = new DomOps(Dom, SignalDomEvt, Messenger);
		fileServer = new FileServer(
			DomOps,
			Opt.ServNfos,
			Opt.ScssOutputFolder,
			Opt.LINQPadRefs,
			Opt.Logr
		);
		ServDbg = new ServDbg(DomOps, Messenger).D(d);

		DomOps.AddInitialNodes(rootNodes);
		DomEvtActioner.Setup(WhenDomEvt, DomOps).D(d);
		server.AddRepliers(
			new ServePageReplier(Dom),
			new ServeFilesReplier(fileServer)
		);
		server.AddRepliers(Opt.Repliers);
		Syncer.Setup(Dom, Messenger).D(d);
		ServSetupUtils.HookClientUserMessages(Messenger, Opt).D(d);

		Opt.Logr.OnSimpleMsg($"Listening on: {UrlUtils.GetLocalLink(Opt.Port)}");
	}

	public void Start()
	{
		fileServer.Start();
		server.Start();
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
		I.SignalDomEvt(new AddBodyNodeDomEvt(node));
		Disposable.Create(() =>
		{
			I.SignalDomEvt(new RemoveBodyNodeDomEvt(node.Id));
		}).D(d);
		return d;
	}
}
