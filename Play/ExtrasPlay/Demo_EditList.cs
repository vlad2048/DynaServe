using DynaServeExtrasLib.Components.DlgInputLogic;
using DynaServeExtrasLib.Components.DlgInputLogic.Comps;
using DynaServeExtrasLib.Components.EditListLogic;
using DynaServeExtrasLib.Components.EditListLogic.EditListConstraints;
using DynaServeExtrasLib.Components.EditListLogic.StructsEnum;
using DynaServeLib;
using PowMaybe;
using PowRxVar;

namespace ExtrasPlay;

static class Demo_EditList
{
	public static void Run()
	{
		using var d = new Disp();

		var masters = Var.Make(masterArr).D(d);

		var listMaster = new EditList<RecMaster>(
			masters,
			"Masters",
			DlgMaster,
			opt =>
			{
				opt.SelectMode = EditListSelectMode.Single;
				opt.Width = 300;
			}
		).D(d);

		var slavesLinkNfo = EditListConstraintMaker.OwnsMultiple(
			listMaster,
			master => master.Slaves,
			(master, slaves) => master with { Slaves = slaves }
		).D(d);

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
		).D(d);

		Serv.Start(
			opt =>
			{
				opt.RegisterEditList();
				opt.AddScriptCss("test", TestCss);
			},
			listMaster.UI,
			listSlaves.UI
		);

		//slavesLinkNfo.Slaves.InspectArr();
		//listSlaves.SelItem.Inspect();

		Console.WriteLine("Running ...");
		Console.ReadKey();
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