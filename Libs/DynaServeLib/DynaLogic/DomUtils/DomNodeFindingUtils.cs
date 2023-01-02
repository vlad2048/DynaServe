using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using PowBasics.CollectionsExt;

namespace DynaServeLib.DynaLogic.DomUtils;

static class DomNodeFindingUtils
{
	public static string[] GetAllCssLinks(this IHtmlDocument dom) => dom
		.GetAllCssLinkNodes()
		.SelectToArray(e => e.Href.GetRelevantLinkEnsure());

	public static string[] GetAllJsLinks(this IHtmlDocument dom) => dom
		.GetAllJsLinkNodes()
		.SelectToArray(e => e.Source!);
		//.SelectToArray(e => e.Source.GetRelevantLinkEnsure());

	public static IElement[] GetUserBodyNodes(this IHtmlDocument dom) => dom
		.FindDescendant<IHtmlBodyElement>()!
		.Children
		.Where(e => e.Id != ServInst.StatusEltId)
		.ToArray();

	public static IHtmlImageElement[] GetAllImgNodes(this IHtmlDocument dom) => dom
		.GetRecursively<IHtmlImageElement>()
		.WhereToArray(e => e.Source.IsLinkRelevant());



	public static IHtmlLinkElement[] GetAllCssLinkNodes(this IHtmlDocument dom) => dom
		.FindDescendant<IHtmlHeadElement>()!
		.Children
		.OfType<IHtmlLinkElement>()
		.Where(e => e.Relation == "stylesheet")
		.Where(e => e.Href.IsLinkRelevant())
		.ToArray();

	public static IHtmlScriptElement[] GetAllJsLinkNodes(this IHtmlDocument dom) => dom
		.FindDescendant<IHtmlHeadElement>()!
		.Children
		.OfType<IHtmlScriptElement>()
		.Where(e => e.Source != null)
		//.Where(e => e.Source.IsLinkRelevant())
		.ToArray();
}