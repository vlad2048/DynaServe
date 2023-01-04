global using static DynaServeLib.Nodes.Ctrls;
global using static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers;
using PowRxVar;

namespace ExtrasPlay;

static class Program
{
	public static void Main()
	{
		//DispStats.SetBP(105);
		//DispStats.OnBPHit = () => { };
		
		MainInner();

		DispStats.Log();

		Console.WriteLine("Press a key to exit ...");
		Console.ReadKey();
	}

	private static void MainInner()
	{
		//Demo_LiveReload.Run();
		//Demo_BoundCtrls.Run();

		//Demo_Extras_DlgInput.Run();
		Demo_Extras_EditList.Run();
		//Demo_Extras_FontAwesome.Run();
	}
}


