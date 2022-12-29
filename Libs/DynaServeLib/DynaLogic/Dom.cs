using System.Reactive;
using System.Reactive.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DynaServeLib.DynaLogic.DiffLogic;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.DynaLogic.Refreshers;
using DynaServeLib.DynaLogic.Utils;
using DynaServeLib.Logging;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;
using PowTrees.Algorithms;

namespace DynaServeLib.DynaLogic;


class Dom : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly HtmlNode[] initRootNodes;
	private readonly IDomTweaker[] domTweakers;
	private readonly ResourceHolder resourceHolder;
	private readonly RefreshTracker refreshTracker;
	private readonly ILogr logr;
	private Action<ServerMsg>? sendServerMsg;
	private Action<ServerMsg> SendServerMsg => sendServerMsg ?? throw new ArgumentException("Start not called");

	public IHtmlDocument Doc { get; }

	public void LogFull(string clientHtml) => DomLogUtils.LogFull(clientHtml, Doc, refreshTracker.DbgGetRefresherIds(), logr);

	public void LogEvt(string message) => logr.OnLogEvt(new LogEvt(
		message,
		GetCssLinkNodes().SelectToArray(e => e.Href!.CssNorm()),
		GetPageBodyNodes().Fmt(),
		null
	));


	public IElement[] GetPageBodyNodes() =>
		Doc
			.FindDescendant<IHtmlBodyElement>()!
			.Children
			.Where(e => e.Id != ServInst.StatusEltId)
			.ToArray();

	private IHtmlLinkElement[] GetCssLinkNodes() =>
		Doc
			.FindDescendant<IHtmlHeadElement>()!
			.Children
			.FilterCssLinks()
			.ToArray();

	public string[] GetCssLinks() =>
		GetCssLinkNodes()
			.Select(e => e.Href!.CssNorm())
			.ToArray();

	public Dom(
		HtmlNode[] rootNodes,
		IDomTweaker[] domTweakers,
		IObservable<IDomEvt> whenDomEvt,
		ResourceHolder resourceHolder,
		ILogr logr
	)
	{
		initRootNodes = rootNodes;
		this.domTweakers = domTweakers;
		this.resourceHolder = resourceHolder;
		this.logr = logr;
		refreshTracker = new RefreshTracker().D(d);
		Doc = new HtmlParser().ParseDocument(InitialHtml).D(d);

		whenDomEvt.Subscribe(evt =>
		{
			switch (evt)
			{
				case ReplaceChildrenDomEvt e:
				{
					var node = this.GetById(e.NodeId);
					
					// Unhook refreshers & Remove children
					refreshTracker.RemoveChildrenRefreshers(node, false);
					node.RemoveAllChildren();

					// Create children and their refreshers
					var (childrenNext, refreshersNext) = Doc.CreateNodes(e.Children, domTweakers);

					// Add children & Hook their refreshers
					node.AppendChildren(childrenNext);
					refreshTracker.AddRefreshers(refreshersNext);

					// Notify the frontend
					SendServerMsg(ServerMsg.MkReplaceChildren(childrenNext.Fmt(), e.NodeId));
					break;
				}

				case DiffChildrenDomEvt e:
				{
					var node = this.GetById(e.NodeId);
					var childrenPrev = node.Children.ToArray();
					var (childrenNext, refreshersNext) = Doc.CreateNodes(e.Children, domTweakers);

					var diffs = DiffAlgo.ComputeDiffs(childrenPrev, childrenNext);

					SendServerMsg(ServerMsg.MkDiffUpdate(diffs));
					throw new NotImplementedException();
				}

				case AddBodyNode e:
				{
					var node = e.Node;

					var (domNode, refreshers) = Doc.CreateNode(node, domTweakers);
					var body = Doc.FindDescendant<IHtmlBodyElement>()!;

					body.AppendChild(domNode);
					refreshTracker.AddRefreshers(refreshers);

					SendServerMsg(ServerMsg.MkAddChildToBody(domNode.Fmt()));
					break;
				}

				case RemoveBodyNode e:
				{
					var nodeId = e.NodeId;

					var domNode = this.GetById(nodeId);
					refreshTracker.RemoveChildrenRefreshers(domNode, true);
					domNode.Remove();

					SendServerMsg(ServerMsg.MkRemoveChildFromBody(nodeId));

					break;
				}
			}
		}).D(d);
	}

	public void Start(RefreshCtx refreshCtx)
	{
		sendServerMsg = refreshCtx.SendServerMsg;
		refreshTracker.Start(refreshCtx);
		initRootNodes.ForEach(AddNodeToBodyOnStart);
	}



	private void AddNodeToBodyOnStart(HtmlNode htmlNode)
	{
		var body = Doc.FindDescendant<IHtmlBodyElement>()!;
		var (node, refreshers) = Doc.CreateNode(htmlNode, domTweakers);
		body.AppendChild(node);
		refreshTracker.AddRefreshers(refreshers);
	}

	
	

	public void AddScriptCss(string name, string script)
	{
		var head = Doc.FindDescendant<IHtmlHeadElement>()!;
		var node = Doc.CreateElement<IHtmlLinkElement>();
		var link = ScriptUtils.MkLink(ScriptType.Css, name);
		node.Relation = "stylesheet";
		node.Type = "text/css";
		node.Href = link;
		head.AppendNodes(node);
		resourceHolder.AddContent(link, Reply.MkTxt(ReplyType.ScriptCss, script));
	}

	public void AddScriptManifest(string manifestFile)
	{
		var name = Path.GetFileName(manifestFile);
		var link = name;
		var head = Doc.FindDescendant<IHtmlHeadElement>()!;
		var node = Doc.CreateElement<IHtmlLinkElement>();
		node.Relation = "manifest";
		node.Href = link;
		head.AppendNodes(node);
		resourceHolder.AddContent(link, Reply.MkTxt(ReplyType.Json, File.ReadAllText(manifestFile)));
	}

	public void AddScriptJS(string name, string script)
	{
		var head = Doc.FindDescendant<IHtmlHeadElement>()!;
		var node = Doc.CreateElement<IHtmlScriptElement>();
		var link = ScriptUtils.MkLink(ScriptType.Js, name);
		node.Source = link;
		head.AppendNodes(node);
		resourceHolder.AddContent(link, Reply.MkTxt(ReplyType.ScriptJs, script));
	}

	public void AddOrRefreshCss(string name, string script)
	{
		var link = ScriptUtils.MkLink(ScriptType.Css, name);
		var domLinks = GetCssLinkNodes();

		var domLink = domLinks.SingleOrDefault(e => e.Href!.CssNorm().CssDemake().Item1 == link);

		if (domLink == null)
		{
			AddScriptCss(name, script);
		}
		else
		{
			var cssLinkPrev = domLink.Href!;
			var cssLinkRefresh = cssLinkPrev.CssNorm().CssInc();
			domLink.Href = cssLinkRefresh;
			resourceHolder.RemoveLink(cssLinkPrev);
			resourceHolder.AddContent(cssLinkRefresh, Reply.MkTxt(ReplyType.ScriptCss, script));
			SendServerMsg(ServerMsg.MkRefreshCss(cssLinkRefresh));
		}
	}

	public void AddHtml(string html)
	{
		if (Serv.IsStarted) throw new ArgumentException();
		var body = Doc.FindDescendant<IHtmlBodyElement>()!;
		var parser = new HtmlParser();
		var nodes = parser.ParseFragment(html, body);
		body.PrependNodes(nodes.ToArray());
	}


	private const string InitialHtml = """
		<!DOCTYPE html>
		<html>
			<head>
				<link rel="icon" type="image/png" href="image/creepy-icon.png" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<title>DynaServe</title>
				<link href='http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900' rel='stylesheet' type='text/css'>
			</head>
			<body>
			</body>
		</html>
		""";
}
