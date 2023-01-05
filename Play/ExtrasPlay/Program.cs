global using static DynaServeLib.Nodes.Ctrls;
global using static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers;
using PowRxVar;

namespace ExtrasPlay;

static class Program
{
	private static void MainInner(Disp d)
	{
		//Demo_Basics.Minimal(d);
		//Demo_Basics.RefreshChildren(d);

		//Demo_LiveReload.Run(d);
		//Demo_BoundCtrls.Run(d);

		//Demo_Extras_DlgInput.Run(d);
		Demo_Extras_EditList.Run(d);
		//Demo_Extras_FontAwesome.Run(d);
	}


	public static void Main()
	{
		//DispStats.SetBP(105);
		//DispStats.OnBPHit = () => { };

		using (var d = new Disp())
		{
			MainInner(d);

			Console.WriteLine("Running");
			Console.ReadKey();
		}

		DispStats.Log();

		Console.WriteLine("Finished");
		Console.ReadKey();
	}
}


