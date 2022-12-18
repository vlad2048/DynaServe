using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace DynaServeLib.Serving.Structs;

enum ScriptType
{
	Js,
	Css
}

static class ScriptUtils
{
	public static string MkLink(ScriptType type, string name) => type switch
	{
		ScriptType.Js => $"js/{name}.js",
		ScriptType.Css => $"css/{name}.css",
		_ => throw new ArgumentException()
	};
}

/*
record ScriptNfo(
	ScriptType Type,
	string Name,
	string Script
)
{
	public string Link => Type switch
	{
		ScriptType.Js => $"js/{Name}.js",
		ScriptType.Css => $"css/{Name}.css",
		_ => throw new ArgumentException()
	};

	public Reply Reply => Type switch
	{
		ScriptType.Js => Reply.MkTxt(ReplyType.ScriptJs, Script),
		ScriptType.Css => Reply.MkTxt(ReplyType.ScriptCss, Script),
		_ => throw new ArgumentException()
	};

	//private static int cssIdx;

	public IHtmlElement CreateNode(IHtmlDocument doc)
	{
		switch (Type)
		{
			case ScriptType.Js:
			{
				var node = doc.CreateElement<IHtmlScriptElement>();
				node.Source = Link;
				return node;
			}
			case ScriptType.Css:
			{
				var node = doc.CreateElement<IHtmlLinkElement>();
				node.Relation = "stylesheet";
				node.Type = "text/css";
				//node.Href = $"{Link}?c={cssIdx++}";
				node.Href = Link;
				return node;
			}
			default:
				throw new ArgumentException();
		}
	}
}
*/
