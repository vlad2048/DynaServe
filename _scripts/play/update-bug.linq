<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
</Query>

#load "..\libs\sys"


void Main()
{
	var idx = Var.Make(0).D(D);
	
	Serv.Start(
		opt =>
		{
			opt.Logr = new ConsoleLogr();
			opt.ServeString(TestCss, "test.css");
		},
		Div("main").Wrap(
			Div("btnrow").Wrap(
				//Btn("-", () => idx.V = (idx.V - 1).Wrap()),
				Btn("+", () => idx.V = (idx.V + 1).Wrap())
			),
			Div("panel").Wrap(
				idx.ToUnit(),
				() =>
				{
					var item = Items[idx.V];
					return new[]
					{
						Div().Txt(item).OnClick(() => $"Clicked: {item}".Dump())
					};
				}
			)
		)
	).D(D);
}

public static readonly string[] Items = { "Vlad", "Erik" };

const string TestCss = """
	.main {
		display: flex;
		flex-direction: column;
	}
	.btnrow {
		display: flex;
	}
	""";

public static class Utils
{
	public static int Wrap(this int idx)
	{
		if (idx < 0) idx += Items.Length;
		if (idx >= Items.Length) idx -= Items.Length;
		return idx;
	}
}

void OnStart() => Util.HtmlHead.AddStyles("""
	body {
		font-family: consolas;
	}
	"""
);