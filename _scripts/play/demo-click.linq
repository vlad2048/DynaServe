<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>System.Reactive</Namespace>
</Query>

void Main()
{
	var varBool = Var.Make(true).D(D);
	var whenChange = new Subject<Unit>().D(D);
	Serv.Start(
		opt =>
		{
			opt.Logr = new LPLogger();
			opt.LINQPadRefs = Util.CurrentQuery.FileReferences;
			opt.ServeHardcoded("test.css", TestCss);
		},
		Div("main").Wrap(
			Div().Wrap(
				whenChange,
				() => new[] { Div().Txt("Inside") }
			),
			CheckBox(varBool),
			Btn("Change", () => whenChange.OnNext(Unit.Default))
		)
	).D(D);
		
	var btn = new Button("Toggle", _ => varBool.V = !varBool.V);
	btn.Styles["position"] = "fixed";
	btn.Styles["top"] = "0px";
	btn.Styles["right"] = "0px";
	btn.Dump();
}


private static readonly SerialDisp<Disp> serD = new();
public static Disp D = null!;
void OnStart() { serD.Value = null; serD.Value = D = new Disp(); }

private const string TestCss = """
	.main {
		display: flex;
		flex-direction: column;
		width: 200px;
		align-items: flex-start;
	}
	""";


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
	
	public void Log(string msg) => dc.AppendContent(msg);
	
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
















