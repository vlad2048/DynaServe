using DynaServeLib;
using DynaServeLib.Nodes;
using DynaServeLib.Serving.FileServing.StructsEnum;
using PowRxVar;

namespace ExtrasPlay;

static class Demo_LiveReload
{
	public static void Run(Disp d)
	{
		Serv.Start(
			opt =>
			{
				opt.Serve(FCat.Css, "demo-livereload");
				opt.Serve(FCat.Image, "demo-livereload");
				opt.Serve(FCat.Js, "demo-livereload");
			},
			Div("main").Wrap(
				Div().Txt("Hello"),
				Img("images/img.png"),
				Btn("Run").Attr("onclick", "myFun()")
			)
		).D(d);
	}
}