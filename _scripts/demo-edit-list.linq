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
</Query>

void Main()
{
	Demo_EditList.Run();
}

static class Demo_EditList
{
	public static void Run()
	{
		var list = Var.Make(recArr);
		var editList = new EditList<EditListRec>(
			list,
			"Items",
			DlgRec,
			opt =>
			{
				opt.SelectMode = EditListSelectMode.Multiple;
				opt.Width = 300;
			}
		);

		Serv.Start(
			opt =>
			{
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