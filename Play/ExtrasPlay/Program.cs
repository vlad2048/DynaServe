global using static DynaServeLib.Nodes.Ctrls;
global using static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers;
using DynaServeExtrasLib.Components.FontAwesomeLogic;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;
using PowRxVar;

namespace ExtrasPlay;

static class Program
{
	public static void Main()
	{
		//DispStats.SetBP(105);
		//DispStats.OnBPHit = () => { };

		/*var cont = Embedded.Read("fa-solid-900.woff2", typeof(FontAwesomeRegisterExt).Assembly);
		var bytes = cont.ToBytes();
		var lng = bytes.Length;*/
		
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


