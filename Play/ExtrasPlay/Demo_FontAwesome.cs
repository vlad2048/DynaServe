using DynaServeExtrasLib.Components.FontAwesomeLogic;
using DynaServeLib;
using static DynaServeExtrasLib.Components.FontAwesomeLogic.FontAwesomeCtrls;

namespace ExtrasPlay;

static class Demo_FontAwesome
{
	public static void Run()
	{
		Serv.Start(
			opt =>
			{
				opt.RegisterFontAwesome();
			},
			IconBtn("fa-solid fa-pen-to-square", () => { })
		);

		Console.WriteLine("Running ...");
		Console.ReadKey();
	}
}