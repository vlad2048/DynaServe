using DynaServeLib;
using DynaServeLib.Nodes;
using PowRxVar;
using static DynaServeLib.Nodes.Ctrls;

namespace ServPlay;

static class Program
{
	public static void Main()
	{
		var isOn = Var.Make(true);

		Serv.Start(
			Div().Wrap(
				Btn("Toggle", () => isOn.V = !isOn.V),
				Div().Wrap(
					isOn.ToUnit(),
					() => new[]
					{
						Btn("Check", () => { }).EnableWhen(isOn)
					}
				)
			)
		);


		Console.WriteLine("Listening on http://box-pc:7000/");
		Console.ReadKey();
	}

	private static void L(string s) => Console.WriteLine(s);

	private const string Css = """
		body {
			background-color: #333;
		}
		.vert {
			display: flex;
			flex-direction: column;
		}
		""";
}