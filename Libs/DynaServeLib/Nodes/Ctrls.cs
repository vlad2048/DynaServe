using System.Reactive.Linq;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class Ctrls
{
	public static HtmlNode Div(string? cls = null) => new HtmlNode("div").Cls(cls);
	public static HtmlNode Img(string src) => new HtmlNode("img").Attr("src", src);

	public static HtmlNode Btn(string txt, Action action) =>
		Btn(txt, async () => action());

	public static HtmlNode Btn(string txt, Func<Task> action) =>
		new HtmlNode("button").Txt(txt).Hook("click", action);

	public static HtmlNode Btn(string txt) => new HtmlNode("button").Txt(txt);




	public static HtmlNode CheckBox(IRwVar<bool> rxVar)
	{
		static string? Val2Web(bool v) => v ? "" : null;
		static bool Web2Val(string v) => bool.Parse(v);

		var (bndVar, whenChanged) = MkBndVar(rxVar);

		return new HtmlNode("input").Attr("type", "checkbox")
			.Attr("checked", whenChanged.Select(Val2Web))
			.PropBool("checked", whenChanged)
			.HookArg("change", v => bndVar.SetInner(Web2Val(v)), "this.checked");
	}

	public static HtmlNode TextBox(IRwVar<string> rxVar)
	{
		static string Val2Web(string v) => v;
		static string Web2Val(string v) => v;

		var (bndVar, whenChanged) = MkBndVar(rxVar);

		return new HtmlNode("input").Attr("type", "text")
			.Attr("value", whenChanged.Select(Val2Web))
			.PropStr("value", whenChanged)
			.HookArg("input", v => bndVar.SetInner(Web2Val(v)), "this.value");
	}

	public static HtmlNode RangeSlider(IRwVar<int> rxVar, int min, int max)
	{
		static string Val2Web(int v) => $"{v}";
		static int Web2Val(string v) => int.Parse(v);

		var (bndVar, whenChanged) = MkBndVar(rxVar);

		return new HtmlNode("input").Attr("type", "range").Attr("min", $"{min}").Attr("max", $"{max}")
			.Attr("value", whenChanged.Select(Val2Web))
			.PropInt("value", whenChanged)
			.HookArg("input", v => bndVar.SetInner(Web2Val(v)), "this.value");
	}

	private static (IFullRwBndVar<T>, IObservable<T>) MkBndVar<T>(IRwVar<T> rxVar)
	{
		var bndVar = Var.MakeBnd(rxVar.V).D(rxVar);
		rxVar.PipeTo(bndVar);
		bndVar.WhenInner.Subscribe(e => rxVar.V = e).D(rxVar);
		var whenChanged = bndVar.WhenOuter.Prepend(rxVar.V);
		return (bndVar, whenChanged);
	}
}