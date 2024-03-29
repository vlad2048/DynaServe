﻿using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace DynaServeLib.Utils.Exts;

public static class HtmlExt
{
	public static IHtmlDocument Parse(this string html) => new HtmlParser().ParseDocument(html);

	public static IElement[] ParseFrag(this string html) => new HtmlParser().ParseDocument(html).FindDescendant<IHtmlBodyElement>()!.Children.ToArray();

	public static string FmtBody(this IHtmlDocument dom) => dom
		.FindDescendant<IHtmlBodyElement>()!
		.Children
		.Where(e => e.ClassName != ServInst.DynaServVerCls)
		.ToArray()
		.Fmt();

	public static string Fmt(this INode root)
	{
		using var writer = new StringWriter();
		root.ToHtml(writer, new PrettyMarkupFormatter
		{
			Indentation = "\t",
			NewLine = "\n",
		});
		return writer.ToString();
	}

	public static string Fmt(this IElement[] nodes)
	{
		using var writer = new StringWriter();
		foreach (var node in nodes)
			node.ToHtml(writer, new PrettyMarkupFormatter
			{
				Indentation = "\t",
				NewLine = "\n",
			});
		return writer.ToString();
	}
}