using System.Runtime.CompilerServices;
using PowBasics.CollectionsExt;

namespace DynaServeExtrasLib.Utils;

public static class ObsDbgExt
{
	public static void Inspect<T>(this IObservable<T> obs, [CallerArgumentExpression(nameof(obs))] string? message = null) =>
		obs.Subscribe(e => L($"{message}: {e}"));

	public static void InspectArr<T>(this IObservable<T[]> obs, [CallerArgumentExpression(nameof(obs))] string? message = null) =>
		obs.Subscribe(e => L($"{message}: {e.JoinText()}"));

	private static void L(string s) => Console.WriteLine(s);
}