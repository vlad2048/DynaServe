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

		

	public static HtmlNode TextBox(IRwVar<string> rxVar)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input")
			.Attr("type", "text")
			//.Attr("value", rxVar.V)
			.Attr("value", rxVar.Where(_ => !isUiUpdate))
			.HookArg("input", v =>
			{
				isUiUpdate = true;
				rxVar.V = v;
				isUiUpdate = false;
			}, "this.value");

		/*rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				//St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "value", val));
				St.SendToClientHack(ServerMsg.MkPropChangesDomUpdate(new []
				{
					PropChange.MkAttrChange(node.Id, "value", val),
				}));
			}).D(node.D);*/

		return node;
	}

	public static HtmlNode CheckBox(IRwVar<bool> rxVar)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input")
			.Attr("type", "checkbox")
			//.Attr("checked", rxVar.V ? "" : null)
			.Attr("checked", rxVar.Where(_ => !isUiUpdate).Select(e => e ? "" : null))
			.HookArg("change", valStr =>
			{
				var val = bool.Parse(valStr);
				isUiUpdate = true;
				rxVar.V = val;
				isUiUpdate = false;
			}, "this.checked");

		/*rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				var valStr = val ? "" : null;
				//St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "checked", valStr));
				St.SendToClientHack(ServerMsg.MkPropChangesDomUpdate(new []
				{
					PropChange.MkAttrChange(node.Id, "checked", valStr),
				}));
			}).D(node.D);*/

		return node;
	}

	public static HtmlNode RangeSlider(IRwVar<int> rxVar, int min, int max)
	{
		var isUiUpdate = false;

		var node = new HtmlNode("input")
			.Attr("type", "range")
			.Attr("min", $"{min}")
			.Attr("max", $"{max}")
			//.Attr("value", $"{rxVar.V}")
			.Attr("value", rxVar.Where(_ => !isUiUpdate).Select(e => $"{e}"))
			.HookArg("change", valStr =>
			{
				var val = int.Parse(valStr);
				isUiUpdate = true;
				rxVar.V = val;
				isUiUpdate = false;
			}, "this.value");

		/*rxVar
			.Where(_ => !isUiUpdate)
			.Subscribe(val =>
			{
				var valStr = $"{val}";
				//St.SendToClientHack(ServerMsg.MkSetAttr(node.Id, "value", valStr));
				St.SendToClientHack(ServerMsg.MkPropChangesDomUpdate(new []
				{
					PropChange.MkAttrChange(node.Id, "value", valStr),
				}));
			}).D(node.D);*/

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