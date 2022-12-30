using DynaServeLib;
using DynaServeLib.DynaLogic.DiffLogic;
using DynaServeLib.Gizmos;
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
				<span id="sp0" class="id_0"></span>
				<section id="sec0" class="id_2"></section>
				<span id="sp1" class="id_3">
					<div id="div0" class="id_30"></div>
				</span>
			</body>
			""";

		var htmlNext = """
			<body id="root">
				<span id="sp0" class="id_0"></span>
				<section id="sec0" class="id_2"></section>
				<span id="sp1" class="id_3">
					<div id="div0" class="id_37"></div>
				</span>
			</body>
			""";

		var domPrev = htmlPrev.Parse();
		var domNext = htmlNext.Parse();

		var childrenPrev = domPrev.Body!.Children.ToArray();
		var childrenNext = domNext.Body!.Children.ToArray();

		var identical = DiffAlgo.Are_DomNodeTrees_StructurallyIdentical(childrenPrev, childrenNext);
		L($"Identical: {identical}");
		L("");

		if (identical)
		{
			var chgs = DiffAlgo.ComputePropChanges_In_StructurallyIdentical_DomNodesTree(childrenPrev, childrenNext);
			L($"{chgs.Length} changes:");
			foreach (var chg in chgs)
				L($"  {chg}");
			L("");

			DiffAlgo.ApplyPropChanges_In_DomNodeTrees(childrenPrev, chgs);
			L(childrenPrev.Fmt());
		}
	}



	public static void Main()
	{

		//Console.WriteLine($"{DynaServVerDisplayer.GetVer()}");
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