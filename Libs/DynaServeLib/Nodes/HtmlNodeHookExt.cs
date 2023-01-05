using DynaServeLib.DynaLogic.Refreshers;

namespace DynaServeLib.Nodes;

public static class HtmlNodeHookExt
{
	public static HtmlNode Hook(this HtmlNode node, string evtName, Action action, bool stopPropagation = false) =>
		node.Hook(evtName, async () => action(), stopPropagation);
	public static HtmlNode HookArg(this HtmlNode node, string evtName, Action<string> action, string argExpr, bool stopPropagation = false) =>
		node.HookArg(evtName, async str => action(str), argExpr, stopPropagation);

	public static HtmlNode Hook(this HtmlNode node, string evtName, Func<Task> action, bool stopPropagation = false) => node
		.AddRefr(new EvtRefresher(evtName, action, stopPropagation));

	public static HtmlNode HookArg(this HtmlNode node, string evtName, Func<string, Task> action, string argExpr, bool stopPropagation = false) => node
		.AddRefr(new EvtArgRefresher(evtName, action, argExpr, stopPropagation));

	
	
	//public static HtmlNode HookArgJS(this HtmlNode node, string evtName, string jsCode)


	public static HtmlNode OnClick(this HtmlNode node, Action action, bool stopPropagation = false) =>
		node.Hook("click", action, stopPropagation);




	private static HtmlNode AddRefr(this HtmlNode node, IRefresher refresher)
	{
		node.AddRefresher(refresher);
		return node;
	}

	private static string? MkStopPropagationStr(bool stopPropagation) => stopPropagation switch
	{
		true => "event.stopPropagation();",
		false => null
	};
}