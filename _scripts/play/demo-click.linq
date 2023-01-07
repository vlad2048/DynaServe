<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <NuGetReference>AngleSharp.XPath</NuGetReference>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Utils.Exts</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>AngleSharp.XPath</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
</Query>

void Chk()
{
	var dom = html.Parse();
	var start = dom.GetElementById("start");
	
	var xpath = "./div[2]/span[2]/span[1]/div[1]";
	
	var end = start.SelectSingleNode(xpath);
	
	if (end == null)
	{
		"not found".Dump();
		return;
	}

	$"found id='{((IElement)end).Id}'".Dump();
}

const string html = """
	<!DOCTYPE html>
	<html>
		<head>
			<link rel="icon" type="image/png" href="image/creepy-icon.png" />
			<meta name="viewport" content="width=device-width, initial-scale=1.0" />
			<title>DynaServe</title>
			<link href='http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900' rel='stylesheet' type='text/css'>
		</head>
		<body id="body">
		
			<div>
				<div id="start">
					<div>
					</div>
					<div>
						<span>
						</span>
						<div>
						</div>
						<span>
							<span>
								<div id="end">
								</div>
								<div id="other">
								</div>
							</span>
							<div>
							</div>
						</span>
						<span>
						<span>
					</div>
					<div>
					</div>
				</div>
			</div>
			<div>
			</div>
		
		</body>
	</html>
	""";

void Main()
{
	Chk(); return;
	var varBool = Var.Make(true).D(D);
	var whenChange = new Subject<Unit>().D(D);
	Serv.Start(
		opt =>
		{
			opt.Logr = new LPLogger();
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
















