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
  <Namespace>DynaServeExtrasLib.Components.EditListLogic.EditListConstraints</Namespace>
</Query>

#load ".\libs\sys"


void Main()
{
	Demo_EditList.Run();
}

static class Demo_EditList
{
	public static void Run()
	{
		Util.ReadLine();

		var masters = Var.Make(masterArr).D(D);

		var listMaster = new EditList<RecMaster>(
			masters,
			"Masters",
			DlgMaster,
			opt =>
			{
				opt.SelectMode = EditListSelectMode.Single;
				opt.Width = 300;
			}
		).D(D);

		var slavesLinkNfo = EditListConstraintMaker.OwnsMultiple(
			listMaster,
			master => master.Slaves,
			(master, slaves) => master with { Slaves = slaves }
		).D(D);

		var listSlaves = new EditList<RecSlave>(
			slavesLinkNfo.Slaves,
			"Slaves",
			DlgSlave,
			opt =>
			{
				opt.SelectMode = EditListSelectMode.Multiple;
				opt.Width = 300;
				opt.WhenCanAdd = slavesLinkNfo.WhenSlaveCanAdd;
			}
		).D(D);

		Serv.Start(
			opt =>
			{
				opt.RegisterEditList();
				opt.ServeHardcoded("test.css", TestCss);
			},
			listMaster.UI,
			listSlaves.UI
		).D(D);

		Console.WriteLine("Running ...");
	}


	private record RecMaster(
		string Name,
		RecSlave[] Slaves
	)
	{
		public override string ToString() => $"{Name} ({Slaves.Length})";
	}

	private record RecSlave(
		string Name
	)
	{
		public override string ToString() => Name;
	}

	private static readonly RecMaster[] masterArr =
	{
		new("Vlad", new RecSlave[] { new("slave_0"), new("slave_1"), new("slave_2") }),
		new("Erik", new RecSlave[] { new("slave_0"), new("slave_1"), new("slave_2"), new("slave_3"), new("slave_4") }),
		new("Milou", new RecSlave[] { }),
	};

	private static async Task<Maybe<RecMaster>> DlgMaster(Maybe<RecMaster> prev)
	{
		const string keyName = "name";

		RecMaster Mk(IDlgReader r) => new(r.GetString(keyName), prev.Select(e => e.Slaves).FailWith(Array.Empty<RecSlave>()));

		var mayRead = await DlgInput.Make(
			prev.IsSome() ? "Edit Master" : "Add Master",
			dlg =>
			{
				dlg.ValidFun = r => !string.IsNullOrWhiteSpace(Mk(r).Name);
				dlg.EditString(keyName, "Name", prev.Select(e => e.Name).FailWith(""));
			}
		);

		return mayRead.Select(Mk);
	}

	private static async Task<Maybe<RecSlave>> DlgSlave(Maybe<RecSlave> prev)
	{
		const string keyName = "name";

		static RecSlave Mk(IDlgReader r) => new(r.GetString(keyName));

		var mayRead = await DlgInput.Make(
			prev.IsSome() ? "Edit Slave" : "Add Slave",
			dlg =>
			{
				dlg.ValidFun = r => !string.IsNullOrWhiteSpace(Mk(r).Name);
				dlg.EditString(keyName, "Name", prev.Select(e => e.Name).FailWith(""));
			}
		);

		return mayRead.Select(Mk);
	}

	private const string TestCss = """
		body {
			display: flex;
			flex-direction: row;
			gap: 10px;
		}
		""";
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



