using DynaServeLib.Nodes;
using PowRxVar;

namespace DynaServeExtrasLib.Components.FontAwesomeLogic;

public static class FontAwesomeCtrls
{
	public static HtmlNode IconToggle(IRwVar<bool> isOn, string cls) =>
		new HtmlNode("i")
			.Cls(isOn.SelectVar(on => on switch
			{
				true => $"{cls} base-icontoggle base-icontoggle-on",
				false => $"{cls} base-icontoggle base-icontoggle-off",
			}))
			.OnClick(() => isOn.V = !isOn.V);


	public static HtmlNode IconBtn(string cls, Action action) => IconBtn(cls, async () => action());

	public static HtmlNode IconBtn(string cls, Func<Task> action) =>
		Btn("", action).Cls("base-iconbtn").Wrap(
			new HtmlNode("i").Cls(cls)
		);
}