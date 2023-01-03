using System.Reactive.Linq;
using DynaServeExtrasLib.Utils;
using DynaServeLib;
using DynaServeLib.Nodes;
using DynaServeLib.Utils.Exts;
using PowRxVar;

namespace ExtrasPlay;

static class Demo_BoundCtrls
{
	public static void Run()
	{
		var varBool = Var.Make(true);
		var varTxt = Var.Make("Hello there");
		var varNum = Var.Make(22);

		varBool.Inspect();
		varTxt.Inspect();
		varNum.Inspect();

		Serv.Start(
			opt =>
			{
				opt.ServeHardcoded("test.css", TestCss);
			},
			Div("main").Wrap(
				CheckBox(varBool),
				TextBox(varTxt),
				RangeSlider(varNum, 0, 30)
			)
		);
		
		Console.WriteLine("Running ...");
		Console.WriteLine("Press:");
		Console.WriteLine("  1 - Toggle CheckBox");
		Console.WriteLine("  2 - Append To TextBox");
		Console.WriteLine("  3 - Increment RangeSlider");
		Console.WriteLine("  Q - Quit");
		while (true)
		{
			var key = Console.ReadKey().Key;
			switch (key)
			{
				case ConsoleKey.D1:
					varBool.V = !varBool.V;
					break;

				case ConsoleKey.D2:
					varTxt.V += "C";
					break;

				case ConsoleKey.D3:
					varNum.V++;
					break;

				case ConsoleKey.Q:
					break;
			}
		}
	}

	private const string TestCss = """
		.main {
			display: flex;
			flex-direction: column;
			width: 200px;
			align-items: flex-start;
		}
		""";
}