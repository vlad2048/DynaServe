using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.Nodes;
using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;
using PowRxVar;
using PowRxVar.Utils;

namespace ExtrasPlay;

static class Demo_Basics
{
	public static void Minimal(Disp d) =>
		Serv.Start(
			opt =>
			{
				opt.ServeString(TestCss, "test.css");
			},
			Div("main").Wrap(
				Div().Txt("Minimal"),
				Div().Wrap(
					Div().Txt("Sub 1"),
					Div().Txt("Sub 2"),
					Div().Txt("Sub 3"),
					Div().Txt("Sub 4")
				)
			)
		).D(d);

	public static void Buttons(Disp d)
	{
		var rxVar = Var.Make(0).D(d);

		Serv.Start(
			opt =>
			{
				opt.ServeString(TestCss, "test.css");
			},
			Div("main").Wrap(
				rxVar.ToUnit(),
				() => new []
				{
					Btn("Inc", () => rxVar.V++),
					Div().Txt($"cnt_{rxVar.V}")
				}
			)
		).D(d);
	}


	public static void Counter(Disp d)
	{
		var (when, act) = MkEvt().D(d);
		var cnt = 0;

		Serv.Start(
			opt =>
			{
				opt.ServeString(TestCss, "test.css");
			},
			Div().Txt("Counter"),
			Div().Txt(when.Select(_ => $"cnt_{cnt++}").StartWith("cnt_start"))
		).D(d);

		while (true)
		{
			var key = Console.ReadKey().Key;
			switch (key)
			{
				case ConsoleKey.Spacebar:
					act();
					break;

				case ConsoleKey.Q:
					return;
			}
		}
	}


	public static void RefreshChildren(Disp d)
	{
		var (when, act) = MkEvt().D(d);
		var cnt = 0;
		Serv.Start(
			opt =>
			{
				opt.ServeString(TestCss, "test.css");
			},
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
		).D(d);

		while (true)
		{
			var key = Console.ReadKey().Key;
			switch (key)
			{
				case ConsoleKey.Spacebar:
					act();
					break;

				case ConsoleKey.Q:
					return;
			}
		}
	}

	private record Num(int Val, int Ver);

	public static void RefreshComplex(Disp d)
	{
		var rxNum = Var.Make(new Num(0, 0)).D(d);

		Serv.Start(
			opt =>
			{
				opt.ServeString(TestCss, "test.css");
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
		).D(d);

		Console.WriteLine("Press 1 or 2");
		while (true)
		{
			var key = Console.ReadKey().Key;
			switch (key)
			{
				case ConsoleKey.D1:
					rxNum.V = new Num(0, rxNum.V.Ver + 1);
					break;

				case ConsoleKey.D2:
					rxNum.V = new Num(1, rxNum.V.Ver + 1);
					break;

				case ConsoleKey.Q:
					return;
			}
		}
	}



	private static (IObservable<Unit>, Action, IDisposable) MkEvt()
	{
		var d = new Disp();
		ISubject<Unit> when = new Subject<Unit>().D(d);
		return (
			when.AsObservable(),
			() => when.OnNext(Unit.Default),
			d
		);
	}


	private const string TestCss = """
		.main {
			display: flex;
			flex-direction: column;
			width: 200px;
			align-items: flex-start;
		}
		""";
}