using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using PowBasics.CollectionsExt;

namespace DynaServeLib.Serving.FileServing.Logic;

static class LinkCreator
{
	private static readonly Dictionary<FCat, ICreator> creatorMap = new()
	{
		{ FCat.Css, new CssCreator() },
		{ FCat.Js, new JsCreator() },
		{ FCat.Manifest, new ManifestCreator() },
	};

	
	public static void CreateLink(DomOps domOps, IEnumerable<ServLinkFile> linkFiles) =>
		linkFiles.ForEach(e => CreateLink(domOps, e));



	private static void CreateLink(DomOps domOps, ServLinkFile linkFile)
	{
		var creator = creatorMap[linkFile.Cat];
		creator.Create(domOps, linkFile.Filename.ToLink());
	}

	private interface ICreator
	{
		void Create(DomOps domOps, string link);
	}

	private class CssCreator : ICreator
	{
		public void Create(DomOps domOps, string link)
		{
			var node = domOps.Dom.CreateElement<IHtmlLinkElement>();
			node.Relation = "stylesheet";
			node.Type = "text/css";
			node.Href = link;
			domOps.Dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
		}
	}

	private class JsCreator : ICreator
	{
		public void Create(DomOps domOps, string link)
		{
			var node = domOps.Dom.CreateElement<IHtmlScriptElement>();
			node.Source = link;
			domOps.Dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
		}
	}

	private class ManifestCreator : ICreator
	{
		public void Create(DomOps domOps, string link)
		{
			var node = domOps.Dom.CreateElement<IHtmlLinkElement>();
			node.Relation = "manifest";
			node.Href = link;
			domOps.Dom.FindDescendant<IHtmlHeadElement>()!.AppendElement(node);
		}
	}
}
