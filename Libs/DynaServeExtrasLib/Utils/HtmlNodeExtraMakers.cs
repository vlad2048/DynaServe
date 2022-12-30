using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Utils;

public static class HtmlNodeExtraMakers
{
	public static HtmlNode TBtn(string? cls = null) => new HtmlNode("button").Cls(cls);
	public static HtmlNode THeader(string? cls = null) => new HtmlNode("header").Cls(cls);
	public static HtmlNode TMain(string? cls = null) => new HtmlNode("main").Cls(cls);
	public static HtmlNode TFooter(string? cls = null) => new HtmlNode("footer").Cls(cls);
	public static HtmlNode TSection(string? cls = null) => new HtmlNode("section").Cls(cls);
	public static HtmlNode TAside(string? cls = null) => new HtmlNode("aside").Cls(cls);
	public static HtmlNode TH1(string? cls = null) => new HtmlNode("h1").Cls(cls);
	public static HtmlNode TH2(string? cls = null) => new HtmlNode("h2").Cls(cls);
	public static HtmlNode TH3(string? cls = null) => new HtmlNode("h3").Cls(cls);
	public static HtmlNode TH4(string? cls = null) => new HtmlNode("h4").Cls(cls);
	public static HtmlNode TH5(string? cls = null) => new HtmlNode("h5").Cls(cls);
	public static HtmlNode TH6(string? cls = null) => new HtmlNode("h6").Cls(cls);
	public static HtmlNode TDlg(string? cls = null) => new HtmlNode("dialog").Cls(cls);
}