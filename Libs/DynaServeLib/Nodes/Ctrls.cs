using System.Reactive;
using System.Reactive.Linq;
using DynaServeLib.Serving.Syncing.Structs;
using PowRxVar;

namespace DynaServeLib.Nodes;

public static class Ctrls
{
	public static HtmlNode Div(string? cls = null) => new HtmlNode("div").Cls(cls);
	public static HtmlNode Img(string src) => new HtmlNode("img").Attr("src", src);

	public static HtmlNode Btn(string txt, Action action) => new HtmlNode("button").Txt(txt).Hook("click", action);

	public static HtmlNode TextBox(IRwVar<string> rxVar)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input").Attr("type", "text")
			.HookArg("input", v =>
			{
				isUiUpdate = true;
				rxVar.V = v;
				isUiUpdate = false;
			}, "this.value");

		rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "value", val));
			}).D(node.D);

		return node;
	}

	public static HtmlNode CheckBox(IRwVar<bool> rxVar)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input").Attr("type", "checkbox")
			.HookArg("change", valStr =>
			{
				var val = bool.Parse(valStr);
				isUiUpdate = true;
				rxVar.V = val;
				isUiUpdate = false;
			}, "this.checked");

		rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				var valStr = val ? "true" : null;
				St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "checked", valStr));
			}).D(node.D);

		return node;
	}

	public static HtmlNode RangeSlider(IRwVar<int> rxVar, int min, int max)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input").Attr("type", "range").Attr("min", $"{min}").Attr("max", $"{max}")
			.HookArg("change", valStr =>
			{
				var val = int.Parse(valStr);
				isUiUpdate = true;
				rxVar.V = val;
				isUiUpdate = false;
			}, "this.value");

		rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				var valStr = $"{val}";
				St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "value", valStr));
			}).D(node.D);

		return node;
	}

	/*private static D DD<D>(this D dispDst, params IRoDispBase[] dispSrcs) where D : IDisposable
	{
		Observable.Merge(dispSrcs.Select(e =>
		{
			return e.WhenDisposed;
		})).Subscribe(_ =>
		{
			dispDst.Dispose();
		});
		return dispDst;
	}*/
}