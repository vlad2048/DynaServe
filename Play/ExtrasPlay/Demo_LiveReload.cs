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
				opt.ServeFolder("demo-livereload", FCat.Css);
				opt.ServeFolder("demo-livereload", FCat.Image);
				opt.ServeFolder("demo-livereload", FCat.Js);
			},
			Div("main").Wrap(
				Div().Txt("Hello"),
				Img("images/img.png"),
				Btn("Run").Attr("onclick", "myFun()")
			)
		).D(d);
	}
}