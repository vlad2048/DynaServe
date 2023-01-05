using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeLib.Nodes;
using DynaServeLib;
using PowRxVar;
using PowRxVar.Utils;

namespace ExtrasPlay;

static class Demo_Basics
{
	public static void Minimal(Disp d) =>
		Serv.Start(
			opt =>
			{
				opt.ServeHardcoded("test.css", TestCss);
			},
			Div("main").Wrap(
				Div().Txt("Minimal")
			)
		).D(d);


	public static void RefreshChildren(Disp d)
	{
		var (when, act) = MkEvt().D(d);
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