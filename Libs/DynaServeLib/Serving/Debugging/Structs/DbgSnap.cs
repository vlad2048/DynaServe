using AngleSharp.Html.Dom;

namespace DynaServeLib.Serving.Debugging.Structs;

public record DbgSnap(
	IHtmlDocument ServerDom,
	IHtmlDocument ClientDom,
	Dictionary<string, string[]> RefreshMap
);