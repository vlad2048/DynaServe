<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <NuGetReference>PowTrees.LINQPad</NuGetReference>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
  <Namespace>C = LINQPad.Controls.Control</Namespace>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Utils.Exts</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>PowBasics.Geom</Namespace>
  <Namespace>PowBasics.StringsExt</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>PowTrees.LINQPad</Namespace>
  <Namespace>PowTrees.LINQPad.DrawerLogic</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
</Query>

#load ".\libs\sys"

void Main()
{
	//Util.ReadLine();
	
	RefreshComplex();
	
	//DynamicAttr();
	//RefreshChildren();
}

private record Num(int Val, int Ver);

public static void RefreshComplex()
{
	var rxNum = Var.Make(new Num(0, 0)).D(D);
	MkBtn(0, "Set 0", () => rxNum.V = new Num(0, rxNum.V.Ver + 1));
	MkBtn(1, "Set 1", () => rxNum.V = new Num(1, rxNum.V.Ver + 1));

	Serv.Start(
		opt => {
			//opt.Port = 7001;
			//opt.CheckSecurity = true;
			opt.ServeHardcoded("test.css", TestCss);
		},
		Div("main").Wrap(
			Div().Txt("RefreshChildren"),
			Btn("Btn", async () =>
			{
				Console.WriteLine("Throwing exception");
				throw new ArgumentException("Btn Ex");
			}),
			Div().Wrap(
				rxNum.ToUnit(),
				() => rxNum.V.Val switch
				{
					0 => new[]
					{
						Div().Wrap(
							Div().Txt("Layout 1"),
							Div().Txt(rxNum.Select(e => $"cnt_{e.Ver}"))
						)
					},
					1 => new[]
					{
						Div().Wrap(
							Div().Txt("Layout 2"),
							Div().Wrap(
								Div().Txt("Inner"),
								Div().Txt(rxNum.Select(e => $"cnt_{e.Ver}"))
							)
						)
					},
					_ => throw new ArgumentException()
				}
			)
		)
	).D(D);
}



static void DynamicAttr()
{
	var when = MkEvt().D(D);
	var cntNum = 0;
	var cntStr = 0;
	var rxNum = Var.Make(0, when.Select(_ => cntNum++)).D(D);
	var rxTxt = Var.Make("cnt", when.Select(_ => $"cnt_{cntStr++}")).D(D);
	Serv.Start(
		opt => { opt.ServeHardcoded("test.css", TestCss); },
		Div("main").Wrap(
			rxNum.Where(e => e % 2 == 0).ToUnit(),
			() => new[]
			{
				Div().Txt("DynamicAttr"),
				Div().Txt(rxTxt)
			}
		)
	).D(D);
}

static void RefreshChildren()
{
	var when = MkEvt().D(D);
	var cnt = 0;
	Serv.Start(
		opt => { opt.ServeHardcoded("test.css", TestCss); },
		Div("main").Wrap(
			Div().Txt("RefreshChildren"),
			Div().Wrap(
				when,
				() => new[]
				{
					Div().Txt($"cnt={cnt++}")
				}
			)
		)
	).D(D);
}



private const string html = """
	<!DOCTYPE html>
	<html>
		<head>
			<link rel="icon" type="image/png" href="image/creepy-icon.png" />
			<meta name="viewport" content="width=device-width, initial-scale=1.0" />
			<title>DynaServe</title>
			<link href='http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900' rel='stylesheet' type='text/css'>
		</head>
		<body>
			<div id="id-1"></div>
			<span class="cls"></span>
		</body>
	</html>
	""";


private static (IObservable<Unit>, IDisposable) MkEvt()
{
	var d = new Disp();
	ISubject<Unit> when = new Subject<Unit>().D(d);
	var btn = new Button("Act", _ => when.OnNext(Unit.Default));
	btn.Styles["position"] = "fixed";
	btn.Styles["right"] = "10px";
	btn.Styles["top"] = "10px";
	btn.Dump();
	return (when.AsObservable(), d);
}

private static void MkBtn(int slot, string text, Action action)
{
	var btn = new Button(text, _ => action());
	btn.Styles["position"] = "fixed";
	btn.Styles["right"] = $"{10 + (slot * 50)}px";
	btn.Styles["top"] = "10px";
	btn.Dump();
}


private const string TestCss = """
	.main {
		display: flex;
		flex-direction: column;
		width: 200px;
		align-items: flex-start;
	}
	""";