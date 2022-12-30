<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeExtrasLib\bin\Debug\net7.0\DynaServeExtrasLib.dll</Reference>
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <Namespace>DynaServeExtrasLib.Components.DlgInputLogic</Namespace>
  <Namespace>DynaServeExtrasLib.Components.DlgInputLogic.Comps</Namespace>
  <Namespace>DynaServeExtrasLib.Components.FontAwesomeLogic</Namespace>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>PowMaybe</Namespace>
  <Namespace>static DynaServeExtrasLib.Components.FontAwesomeLogic.FontAwesomeCtrls</Namespace>
  <Namespace>static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
</Query>

void Main()
{
	Demo_FontAwesome.Run();
}

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
	}
}