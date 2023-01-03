using System.Runtime.CompilerServices;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.Utils.Exts;

public static class ObsDbgExt
{
	public static void InspectBnd<T>(this IFullRwBndVar<T> rxVar, [CallerArgumentExpression(nameof(rxVar))] string? message = null)
	{
		rxVar.Subscribe(e => L($"{message}.V <- {e}"));
		rxVar.WhenOuter.Subscribe(e => L($"{message}.WhenOuter <- {e}"));
		rxVar.WhenInner.Subscribe(e => L($"{message}.WhenInner<- {e}"));
	}

	public static void Inspect<T>(this IObservable<T> obs, [CallerArgumentExpression(nameof(obs))] string? message = null) =>
		obs.Subscribe(e => L($"{message}: {e}"));

	public static void InspectArr<T>(this IObservable<T[]> obs, [CallerArgumentExpression(nameof(obs))] string? message = null) =>
		obs.Subscribe(e => L($"{message}: {e.JoinText()}"));

	private static void L(string s) => Console.WriteLine(s);
}