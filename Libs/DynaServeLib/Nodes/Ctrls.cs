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

		var bndVar = Var.MakeBnd(rxVar.V).D(rxVar);
		rxVar.PipeTo(bndVar);
		bndVar.WhenInner.Subscribe(e => rxVar.V = e).D(rxVar);
		var checkedObs = bndVar.WhenOuter.Prepend(true);

		return new HtmlNode("input")
			.Attr("type", "checkbox")
			.Attr("checked", checkedObs.Select(Val2Web))
			.PropBool("checked", checkedObs)
			.HookArg("change", str => bndVar.SetInner(Web2Val(str)), "this.checked");
	}

		

	public static HtmlNode TextBox(IRwVar<string> rxVar)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input")
			.Attr("type", "text")
			.Attr("value", rxVar.Where(_ => !isUiUpdate).Prepend($"{rxVar.V}"))
			.HookArg("input", v =>
			{
				isUiUpdate = true;
				rxVar.V = v;
				isUiUpdate = false;
			}, "this.value");

		return node;
	}

	public static HtmlNode RangeSlider(IRwVar<int> rxVar, int min, int max)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input")
			.Attr("type", "range")
			.Attr("min", $"{min}")
			.Attr("max", $"{max}")
			.Attr("value", rxVar.Where(_ => !isUiUpdate).Select(e => $"{e}").Prepend($"{rxVar.V}"))
			.HookArg("change", valStr =>
			{
				var val = int.Parse(valStr);
				isUiUpdate = true;
				rxVar.V = val;
				isUiUpdate = false;
			}, "this.value");

		return node;
	}
}