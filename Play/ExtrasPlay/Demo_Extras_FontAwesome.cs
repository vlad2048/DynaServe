using DynaServeExtrasLib.Components.FontAwesomeLogic;
using DynaServeLib;
using PowRxVar;
using static DynaServeExtrasLib.Components.FontAwesomeLogic.FontAwesomeCtrls;

namespace ExtrasPlay;

static class Demo_Extras_FontAwesome
{
	public static void Run(Disp d)
	{
		Serv.Start(
			opt =>
			{
				opt.RegisterFontAwesome();
			},
			IconBtn("fa-solid fa-pen-to-square", () => { })
		).D(d);
	}
}