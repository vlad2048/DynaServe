global using Headers = System.Collections.Generic.Dictionary<string, string>;
global using RefreshMap = System.Collections.Generic.IReadOnlyDictionary<AngleSharp.Dom.IElement, DynaServeLib.DynaLogic.Refreshers.IRefresher[]>;
using System.Runtime.CompilerServices;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

[assembly:InternalsVisibleTo("ServPlay")]


// ReSharper disable once CheckNamespace
public static class LinqpadDump
{
	public static Func<object, object> DmpFun { get; set; } = e => e;
	public static Action ClearFun { get; set; } = () => { };

	public static object Dmp(this object o, bool clear = false)
	{
		if (clear)
			ClearFun();
		if (o is IHtmlDocument doc)
			o = doc.FindDescendant<IHtmlBodyElement>()!;
		DmpFun(o);
		return o switch
		{
			INode e => e.NodeName,
			_ => o
		};
	}
}
