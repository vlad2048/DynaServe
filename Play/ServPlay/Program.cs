using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib;
using DynaServeLib.DynaLogic.DiffLogic;
using DynaServeLib.Nodes;
using DynaServeLib.Utils.Exts;
using PowRxVar;
using static DynaServeLib.Nodes.Ctrls;

namespace ServPlay;

static class Program
{
	static void Test()
	{
		var htmlPrev = """
			<body id="root">
				<span class="id_0"></span>
				<span class="id_2"></span>
				<span class="id_3">
					<div></div>
				</span>
			</body>
			""";
		var htmlNext = """
			<body id="root">
				<span class="id_0"></span>
				<section class="id_2"></section>
				<span class="id_3">
					<div id="ffk"></div>
				</span>
			</body>
			""";

		var domPrev = htmlPrev.Parse();
		var domNext = htmlNext.Parse();

		var childrenPrev = domPrev.Body!.Children.ToArray();
		var childrenNext = domNext.Body!.Children.ToArray();

		var diffs = DiffAlgo.ComputeDiffs(childrenPrev, childrenNext);
		foreach (var diff in diffs)
			Console.WriteLine($"{diff}");

		Console.WriteLine();

		var domCheck = htmlPrev.Parse();
		DiffAlgo.ApplyDiffsToDom(diffs, "root", domCheck);
		var htmlCheck = domCheck.Body!.Fmt();

		Console.WriteLine(htmlCheck);
	}



	public static void Main()
	{
		Test();
		return;

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