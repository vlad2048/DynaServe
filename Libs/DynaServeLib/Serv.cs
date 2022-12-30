using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.DynaLogic;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.Gizmos;
using DynaServeLib.Logging;
using DynaServeLib.Nodes;
using DynaServeLib.Security;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Repliers;
using DynaServeLib.Serving.Repliers.DynaServe;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Serving.Syncing;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib;

public record ClientUserMsg(string Type, string Arg);

public class ServOpt
{
	internal record ScriptJs(string Name, string Script);
	internal record ScriptCss(string Name, string Script);
	internal record ServResource(string Link, Reply Resource);
	
	//public ILogr Logr { get; set; } = new ConsoleLogr();
	//public bool PlaceWebSocketsHtmlManually { get; set; }
	//public bool ShowDynaServLibVersion { get; set; }

	public bool CheckSecurity { get; set; } = true;
	public int Port { get; set; } = 7000;
	//public bool CheckSecurity { get; set; };
	public ILogr Logr { get; set; } = new NullLogr();
	public bool PlaceWebSocketsHtmlManually { get; set; } = true;
	public bool ShowDynaServLibVersion { get; set; } = true;
	public Action<ClientUserMsg> OnClientUserMsg { get; set; } = _ => { };
	internal List<IReplier> Repliers { get; set; } = new();
	internal List<string> CssFolders { get; } = new();
	internal List<string> ImgFolders { get; } = new();
	internal List<string> FontFolders { get; } = new();
	internal List<ScriptJs> JsScripts { get; } = new();
	internal List<ScriptCss> CssScripts { get; } = new();
	internal List<ServResource> ServResources { get; } = new();
	internal Maybe<string> ManifestFile { get; private set; } = May.None<string>();

	internal void KeepUniqueOnly()
	{
		static void Do<T>(List<T> list)
		{
			var listNext = list.Distinct().ToList();
			list.Clear();
			list.AddRange(listNext);
		}

		Do(CssFolders);
		Do(ImgFolders);
		Do(FontFolders);
		Do(ServResources);
	}

	private ServOpt() {}

	public void AddRepliers(params IReplier[] repliers) => Repliers.AddRange(repliers);
	public void AddCss(params string[] cssFolders) => CssFolders.AddRange(cssFolders);
	public void AddImg(params string[] imgFolders) => ImgFolders.AddRange(imgFolders);
	public void AddFonts(params string[] fontFolders) => FontFolders.AddRange(fontFolders);
	public void SetManifest(string manifestFile) => ManifestFile = May.Some(manifestFile);
	public void AddScriptJs(string name, string script) => JsScripts.Add(new ScriptJs(name, script));
	public void AddScriptCss(string name, string script) => CssScripts.Add(new ScriptCss(name, script));

	public void ServEmbeddedFontResources(EmbeddedFile[] files) => ServResources.AddRange(files.Select(file => new ServResource(
		$"fonts/{file.Name}",
		Reply.Mk(ReplyType.FontWoff2, file.Content)
	)));

	internal static ServOpt Build(Action<ServOpt>? optFun)
	{
		var opt = new ServOpt();
		optFun?.Invoke(opt);
		opt.KeepUniqueOnly();
		return opt;
	}
}

class ServInst : IDisposable
{
	internal const string StatusEltId = "syncserv-status";
	internal const string StatusEltClsAuto = "syncserv-status-auto";
	internal const string StatusEltClsManual = "syncserv-status-manual";

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Server server;
	private readonly ISubject<IDomEvt> whenDomEvt;
	private readonly ISubject<LogEvt> whenLogEvt;

	public Syncer Syncer { get; }
	public Dom Dom { get; }
	public IObservable<LogEvt> WhenLogEvt => whenLogEvt.AsObservable();
	public void SignalDomEvt(IDomEvt evt) => whenDomEvt.OnNext(evt);

	public ServInst(Action<ServOpt>? optFun, HtmlNode[] rootNodes)
	{
		var opt = ServOpt.Build(optFun);
		whenDomEvt = new Subject<IDomEvt>().D(d);
		whenLogEvt = new ReplaySubject<LogEvt>().D(d);

		if (opt.CheckSecurity)
			SecurityChecker.CheckPort(opt.Port);

		var resourceHolder = new ResourceHolder();
		resourceHolder.AddFolders(opt.FontFolders, "*.woff2", "fonts", ReplyType.FontWoff2);
		var domTweakers = new IDomTweaker[]
		{
			new ImgDomTweaker(opt.ImgFolders, resourceHolder, opt.Logr)
		};
		Dom = new Dom(rootNodes, domTweakers, whenDomEvt.AsObservable(), resourceHolder, opt.Logr).D(d);

		Dom.AddScriptJS("css-links", Embedded.Read("css-links.js", ("{{HttpLink}}", UrlUtils.GetLocalLink(opt.Port))));
		Dom.AddScriptJS("websockets", Embedded.Read("websockets.js", ("{{WSLink}}", UrlUtils.GetWSLink(opt.Port)), ("{{StatusEltId}}", StatusEltId)));
		Dom.AddScriptCss("websockets", Embedded.Read("websockets.css", ("StatusEltClsAuto", StatusEltClsAuto), ("StatusEltClsManual", StatusEltClsManual)));
		if (!opt.PlaceWebSocketsHtmlManually)
			Dom.AddHtml(Embedded.Read("websockets.html", ("{{StatusEltId}}", StatusEltId), ("{{StatusEltClsAuto}}", StatusEltClsAuto)));
		if (opt.ManifestFile.IsSome(out var manifestFile))
		{
			Dom.AddScriptManifest(manifestFile);
		}
		foreach (var jsScript in opt.JsScripts)
			Dom.AddScriptJS(jsScript.Name, jsScript.Script);
		foreach (var cssScript in opt.CssScripts)
			Dom.AddOrRefreshCss(cssScript.Name, cssScript.Script);
		foreach (var servResource in opt.ServResources)
			resourceHolder.AddContent(servResource.Link, servResource.Resource);
		DynaServVerDisplayer.Show(opt.ShowDynaServLibVersion, Dom);

		var repliers = opt.Repliers
			.Append(new DynaServeReplier(resourceHolder, Dom))
			.ToArray();
		server = new Server(opt.Port, repliers).D(d);
		Syncer = new Syncer(server, Dom).D(d);
		Syncer.WhenClientMsg
			.Where(msg => msg.Type == ClientMsgType.User)
			.Subscribe(msg => opt.OnClientUserMsg(new ClientUserMsg(msg.UserType!, msg.UserArg!))).D(d);
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

	public static HtmlNode StatusEltManual => new HtmlNode("div").Id(ServInst.StatusEltId).Cls(ServInst.StatusEltClsManual);

	public static void Css(string name, string css) => St.I.Dom.AddScriptCss(name, css);

	public static IDisposable AddNodeToBody(HtmlNode node)
	{
		var addD = new Disp();
		ServInst!.SignalDomEvt(new AddBodyNode(node));
		Disposable.Create(() =>
		{
			ServInst.SignalDomEvt(new RemoveBodyNode(node.Id));
		}).D(addD);
		return addD;
	}
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
