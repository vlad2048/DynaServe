using System.Reactive;
using System.Reactive.Linq;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class HtmlNodeExt
{
	public static HtmlNode Id(this HtmlNode node, string id)
	{
		node.Id = id;
		return node;
	}

	public static HtmlNode Cls(this HtmlNode node, string? cls)
	{
		node.Cls = cls;
		return node;
	}

	public static HtmlNode Txt(this HtmlNode node, string? txt)
	{
		node.Txt = txt;
		return node;
	}

	public static HtmlNode Txt(this HtmlNode node, IRoVar<string?> txt) =>
		node.Wrap(
			txt.ToUnit(),
			() => new []
			{
				new HtmlNode("div").Txt(txt.V)
			}
		);

	public static HtmlNode Hook(this HtmlNode node, string evtName, Action action)
	{
		node.AddEvtHook(evtName, action);
		return node;
	}

	public static HtmlNode OnClick(this HtmlNode node, Action action) =>
		node.Hook("click", action);

	public static HtmlNode HookArg(this HtmlNode node, string evtName, Action<string> action, string argExpr)
	{
		node.AddEvtHookArg(evtName, action, argExpr);
		return node;
	}

	public static HtmlNode Wrap(this HtmlNode parent, IEnumerable<HtmlNode> children) => parent.Wrap(children.ToArray());

	public static HtmlNode Wrap(this HtmlNode parent, params HtmlNode[] children)
	{
		parent.SetChildren(children);
		return parent;
	}

	public static HtmlNode Wrap(this HtmlNode parent, IObservable<Unit> when, Func<IEnumerable<HtmlNode>> fun)
	{
		parent.SetChildrenUpdater(when, fun);
		return parent;
	}

	public static HtmlNode Ref(this HtmlNode node, Ref @ref)
	{
		@ref.Hook(node);
		return node;
	}



	/*public static void RecurseRun(this HtmlNode root, Action<HtmlNode> action)
	{
		action(root);
		foreach (var child in root.Children)
			child.RecurseRun(action);
	}

	public static HtmlNode[] GetAllDescendents(this HtmlNode root)
	{
		var list = new List<HtmlNode>();
		root.RecurseRun(list.Add);
		return list.ToArray();
	}*/
}