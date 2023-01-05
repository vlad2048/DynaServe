global using Headers = System.Collections.Generic.Dictionary<string, string>;
global using RefreshMap = System.Collections.Generic.IReadOnlyDictionary<AngleSharp.Dom.IElement, DynaServeLib.DynaLogic.Refreshers.IRefresher[]>;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("ServPlay")]


// ReSharper disable once CheckNamespace
public static class LinqpadDump
{
	public static Func<object, object> DmpFun { get; set; } = e => e;

	public static object Dmp(this object e) => DmpFun(e);
}
