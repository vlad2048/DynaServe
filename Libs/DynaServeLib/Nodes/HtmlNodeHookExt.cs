using DynaServeLib.DynaLogic.Refreshers;

namespace DynaServeLib.Nodes;

public static class HtmlNodeHookExt
{
	public static HtmlNode Hook(this HtmlNode node, string evtName, Action action, bool stopPropagation = false) =>
		node.Hook(evtName, async () => action(), stopPropagation);
	public static HtmlNode HookArg(this HtmlNode node, string evtName, Action<string> action, string argExpr, bool stopPropagation = false) =>
		node.HookArg(evtName, async str => action(str), argExpr, stopPropagation);

	public static HtmlNode Hook(this HtmlNode node, string evtName, Func<Task> action, bool stopPropagation = false)
	{
		/*var stopStr = stopPropagation switch
		{
			true => "event.stopPropagation();",
			false => null
		};

		node.Attrs[$"on{evtName}"] = $"{stopStr}sockEvt('{node.Id}', '{evtName}')";*/
		node.AddRefresher(new EvtRefresher(node.Id, evtName, action, stopPropagation));
		return node;
	}
	
	public static HtmlNode HookArg(this HtmlNode node, string evtName, Func<string, Task> action, string argExpr, bool stopPropagation = false)
	{
		//node.Attrs[$"on{evtName}"] = $"sockEvtArg('{node.Id}', '{evtName}', {argExpr})";
		node.AddRefresher(new EvtArgRefresher(node.Id, evtName, action, argExpr, stopPropagation));
		return node;
	}

	
	
	//public static HtmlNode HookArgJS(this HtmlNode node, string evtName, string jsCode)


	public static HtmlNode OnClick(this HtmlNode node, Action action, bool stopPropagation = false) =>
		node.Hook("click", action, stopPropagation);
}