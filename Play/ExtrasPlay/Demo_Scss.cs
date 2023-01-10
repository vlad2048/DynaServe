using DynaServeLib;
using DynaServeLib.Nodes;
using PowRxVar;

namespace ExtrasPlay;

static class Demo_Scss
{
	public static void Scss(Disp d) =>
		Serv.Start(
			opt =>
			{
				opt.ServeFile("demo-scss.scss");
			},
			Div("main").Wrap(
				Div().Txt("div_1"),
				Div().Txt("div_2")
			)
		).D(d);
}