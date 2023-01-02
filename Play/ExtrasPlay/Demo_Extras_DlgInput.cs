using DynaServeLib.Nodes;
using DynaServeLib;
using PowMaybe;
using DynaServeExtrasLib.Components.DlgInputLogic.Comps;
using DynaServeExtrasLib.Components.DlgInputLogic;

namespace ExtrasPlay;

static class Demo_Extras_DlgInput
{
	public static void Run()
	{
		var mayRec = May.None<Rec>();
	
		Serv.Start(
			opt =>
			{
				opt.RegisterDlgInput();
				opt.ServeHardcoded("demo.css", DemoCss);
			},
			Div("btnrow").Wrap(
				Btn("Edit", async () =>
				{
					static Rec Mk(IDlgReader r) => new(r.GetString(Consts.NameKey), r.GetMultipleChoices(Consts.HobbiesKey), r.GetString(Consts.ZodiacKey));
					var mayRead = await DlgInput.Make("Edit", dlg =>
					{
						dlg.EditString(Consts.NameKey, "Name", mayRec.Select(e => e.Name).FailWith(""));
						dlg.EditSingleChoice(Consts.ZodiacKey, "Zodiac sign", mayRec.Select(e => e.Zodiac), Consts.ZodiacChoices);
						dlg.EditMultipleChoices(Consts.HobbiesKey, "Hobbies", mayRec.Select(e => e.Hobbies).FailWith(Array.Empty<string>()), Consts.HobbyChoices);
						dlg.ValidFun = r => !string.IsNullOrEmpty(Mk(r).Name);
					});
					var mayRecNext = mayRead.Select(Mk);
					if (mayRecNext.IsSome())
						mayRec = mayRecNext;
					Console.WriteLine($"{mayRec}");
				})
			)
		);

		Console.WriteLine("Running ...");
		Console.ReadKey();
	}



	private record Rec(string Name, string[] Hobbies, string Zodiac);

	private static class Consts
	{
		public const string NameKey = "name";
		public const string HobbiesKey = "hobbies";
		public const string ZodiacKey = "zodiac";
	
		public static readonly string[] HobbyChoices = { "Fishing", "Coding", "Chess", "Scrabble", "Golfing" };
		public static readonly string[] ZodiacChoices = { "Capricorn", "Gemini", "Leo", "Sagitarius" };
	}


	private const string DemoCss = """
	.btnrow {
		display: flex;
		gap: 10px;
		align-items: center;
	}
	""";
}