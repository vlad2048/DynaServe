using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.DynaLogic;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Logging;
using DynaServeLib.Nodes;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Repliers;
using DynaServeLib.Serving.Repliers.DynaServe;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Syncing;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using PowRxVar;

namespace DynaServeLib;

public class ServOpt
{
	public int Port { get; set; } = 7000;
	public ILogr Logr { get; set; } = new ConsoleLogr();
	internal List<IReplier> Repliers { get; set; } = new();
	internal List<string> CssFolders { get; } = new();
	internal List<string> ImgFolders { get; } = new();

	private ServOpt() {}

	public void AddRepliers(params IReplier[] repliers) => Repliers.AddRange(repliers);
	public void AddCss(params string[] cssFolders) => CssFolders.AddRange(cssFolders);
	public void AddImg(params string[] imgFolders) => ImgFolders.AddRange(imgFolders);

	internal static ServOpt Build(Action<ServOpt>? optFun)
	{
		var opt = new ServOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

class ServInst : IDisposable
{
	internal const string StatusEltId = "syncserv-status";

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Server server;
	private readonly ISubject<LogEvt> whenLogEvt;

	public Syncer Syncer { get; }
	public Dom Dom { get; }
	public IObservable<LogEvt> WhenLogEvt => whenLogEvt.AsObservable();

	public ServInst(Action<ServOpt>? optFun, HtmlNode[] rootNodes)
	{
		var opt = ServOpt.Build(optFun);
		var whenDomEvt = new Subject<IDomEvt>().D(d);
		whenLogEvt = new ReplaySubject<LogEvt>().D(d);

		var resourceHolder = new ResourceHolder();
		var domTweakers = new IDomTweaker[]
		{
			new ImgDomTweaker(opt.ImgFolders, resourceHolder, opt.Logr)
		};
		Dom = new Dom(rootNodes, domTweakers, whenDomEvt.AsObservable(), resourceHolder, opt.Logr).D(d);

		Dom.AddScriptJS("css-links", Embedded.Read("css-links.js", ("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port))));
		Dom.AddScriptJS("websockets", Embedded.Read("websockets.js", ("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)), ("{{StatusEltId}}", StatusEltId)));
		Dom.AddScriptCss("websockets", Embedded.Read("websockets.css", ("StatusEltId", StatusEltId)));
		Dom.AddHtml(Embedded.Read("websockets.html", ("{{StatusEltId}}", StatusEltId)));

		var repliers = opt.Repliers
			.Append(new DynaServeReplier(resourceHolder, Dom))
			.ToArray();
		server = new Server(opt.Port, repliers).D(d);
		Syncer = new Syncer(server, Dom).D(d);
		Dom.LogEvt("initial");
		Dom.Start(new RefreshCtx(
			whenDomEvt.OnNext,
			Syncer.WhenClientMsg,
			Syncer.SendToClient		// TODO: not needed
		));
		Dom.LogEvt("after start");
		ServCssWatcher.Setup(opt.CssFolders, Dom, opt.Logr).D(d);

		opt.Logr.OnSimpleMsg($"Listening on: {UrlUtils.GetLocalLink(opt.Port)}");
	}

	public void Start()
	{
		server.Start();
		//Dom.Start();
	}
}


public static class Serv
{
	//internal static Disp D { get; private set; } = null!;

	private static Disp? trackD;
	internal static ServInst? ServInst { get; private set; }
	internal static bool IsStarted { get; private set; }

	public static void Start(params HtmlNode[] rootNodes) => Start(null, rootNodes);
	
	public static IDisposable Start(Action<ServOpt>? optFun, params HtmlNode[] rootNodes)
	{
		IsStarted = false;
		trackD?.Dispose();
		var d = trackD = new Disp();

		ServInst = new ServInst(optFun, rootNodes).D(d);
		ServInst.Start();
		IsStarted = true;

		return d;
	}

	public static void Css(string name, string css) => St.I.Dom.AddScriptCss(name, css);
}


static class St
{
	internal static ServInst I => Serv.ServInst ?? throw new ArgumentException("not initialized");

	public static void SendToClientHack(ServerMsg serverMsg)
	{
		if (!Serv.IsStarted) return;
		I.Syncer.SendToClient(serverMsg);
	}
}
