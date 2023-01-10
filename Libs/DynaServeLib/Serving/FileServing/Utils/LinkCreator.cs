using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using PowMaybe;

namespace DynaServeLib.Serving.FileServing.Utils;

static class LinkCreator
{
	public static ScriptNfo[] GetScripts(this IHtmlDocument dom) =>
		dom.FindDescendant<IHtmlHeadElement>()!.ChildNodes
			.Select(GetScript)
			.WhereSome()
			.ToArray();

	private static Maybe<ScriptNfo> GetScript(INode node) =>
		node switch
		{
			IHtmlLinkElement { Relation: "stylesheet" } e when e.Href.IsLinkRelevant() =>
				May.Some(new ScriptNfo(
					ScriptType.Css,
					e.Href.GetRelevantLinkEnsure().RemoveQueryParams()
				)),
			IHtmlScriptElement e =>
				May.Some(new ScriptNfo(
					e.Type == "module" ? ScriptType.JsModule : ScriptType.Js,
					e.Source!.RemoveQueryParams()
				)),
			/*IHtmlLinkElement { Relation: "manifest" } e when e.Href.IsLinkRelevant() =>
				May.Some(new ScriptNfo(
					ScriptType.Manifest,
					e.Href.GetRelevantLinkEnsure().RemoveQueryParams()
				)),*/
			_ => May.None<ScriptNfo>()
		};


	public static void AddScript(this IHtmlDocument dom, ScriptNfo nfo)
	{
		switch (nfo.Type)
		{
			case ScriptType.Css:
				dom.MkCssLink(nfo.Link);
				break;

			case ScriptType.Js:
			case ScriptType.JsModule:
				dom.MkJsLink(nfo.Link, nfo.Type == ScriptType.JsModule);
				break;

			case ScriptType.Manifest:
				dom.MkManifestLink(nfo.Link);
				break;

			default:
				throw new ArgumentException();
		};
	}


	private static void MkCssLink(this IHtmlDocument dom, string link)
	{
		var node = dom.CreateElement<IHtmlLinkElement>();
		node.Relation = "stylesheet";
		node.Type = "text/css";
		node.Href = link;
		dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
	}

	private static void MkJsLink(this IHtmlDocument dom, string link, bool isModule)
	{
		var node = dom.CreateElement<IHtmlScriptElement>();
		node.Source = link;
		node.Type = isModule ? "module" : null;
		dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
	}

	private static void MkManifestLink(this IHtmlDocument dom, string link)
	{
		var node = dom.CreateElement<IHtmlLinkElement>();
		node.Relation = "manifest";
		node.Href = link;
		dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
	}
}