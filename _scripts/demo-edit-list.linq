<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeExtrasLib\bin\Debug\net7.0\DynaServeExtrasLib.dll</Reference>
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <Namespace>DynaServeExtrasLib.Components.DlgInputLogic</Namespace>
  <Namespace>DynaServeExtrasLib.Components.DlgInputLogic.Comps</Namespace>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>PowMaybe</Namespace>
  <Namespace>static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>DynaServeExtrasLib.Components.EditListLogic</Namespace>
  <Namespace>DynaServeExtrasLib.Components.EditListLogic.StructsEnum</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

void Main()
{
	Demo_EditList.Run();
}

static class Demo_EditList
{
	public static void Run()
	{
		//Util.ReadLine();
		
		var list = Var.Make(recArr);
		var editList = new EditList<EditListRec>(
			list,
			"Items",
			DlgRec,
			opt =>
			{
				opt.SelectMode = EditListSelectMode.Single;
				opt.Width = 300;
			}
		);

		Serv.Start(
			opt =>
			{
				//opt.Logr = new LPLogger();
				opt.LINQPadRefs = Util.CurrentQuery.FileReferences;
				opt.RegisterEditList();
			},
			editList.UI
		);

		Console.WriteLine("Running ...");
	}


	private record EditListRec(
		string Name,
		bool Enabled
	)
	{
		public override string ToString() => Name;
	}

	private static readonly EditListRec[] recArr =
	{
		new("Vlad", false),
		new("Milou", false),
		new("Erik", false),
		new("Goncalo", false),
		new("Marek", false),
	};

	private static async Task<Maybe<EditListRec>> DlgRec(Maybe<EditListRec> prev)
	{
		const string keyName = "name";

		EditListRec Mk(IDlgReader r) => new(r.GetString(keyName), prev.Select(e => e.Enabled).FailWith(true));
	
		var mayRead = await DlgInput.Make(
			prev.IsSome() ? "Edit Rec" : "Add Rec",
			dlg =>
			{
				dlg.ValidFun = r => !string.IsNullOrWhiteSpace(Mk(r).Name);
				dlg.EditString(keyName, "Name", prev.Select(e => e.Name).FailWith(""));
			}
		);
	
		return mayRead.Select(Mk);
	}
}








class LPLogger : ILogr
{
	private const string colTransition = "#c9d450";
	private const string colDom = "#0FFC7E";
	
	private readonly DumpContainer dc;
	
	public LPLogger()
	{
		dc = new DumpContainer();
		var div = new Div(dc);
		div.Styles["font-family"] = "Consolas";
		div.Styles["font-weight"] = "bold";
		div.Styles["font-size"] = "12px";
		div.Styles["background-color"] = "#000935";
		div.Styles["color"] = "#0FFC7E";
		div.Styles["padding"] = "5px";
		div.Dump();
	}
	
	public void Log(string msg) => dc.AppendContent(new Div(new Span(msg)));
	
	public void LogTransition(string transition, string dom)
	{
		dc.AppendContent(new Div(new Span(" ")));
		WriteWithCol(transition, colTransition);
		WriteWithCol(dom, colDom);
		ScrollToBottom();
	}
	
	private void WriteWithCol(string str, string col)
	{
		var div = new Div(new Span(str));
		div.Styles["color"] = col;
		dc.AppendContent(div);
	}
	
	public void CssError(string msg) => dc.AppendContent($"[CssError]: {msg}");
	
	private static void ScrollToBottom() => Util.InvokeScript(false, "eval", "window.scrollTo(0, 1000000)");
}



