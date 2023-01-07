using System.Runtime.CompilerServices;

namespace DynaServeLib.Utils.Exts;

public static class RxExt
{
	public static IDisposable SubscribeSafe<T>(
		this IObservable<T> obs,
		Action<T> onNext,
		[CallerArgumentExpression(nameof(obs))] string? obsName = null
	)
		=> obs.Subscribe(e =>
		{
			try
			{
				onNext(e);
			}
			catch (Exception ex)
			{
				var delLng = obsName.GetLongestLineLength();
				var del = new string('=', delLng);
				L("Exception while listening to");
				L(del);
				L($"{obsName}");
				L(del);
				L($"  Type   : {ex.GetType().Name}");
				L($"  Message: {ex.Message}");
				L("");
				L($"{ex}");
			}
		});

	public static IObservable<T> ThrowIf_Observable_IsNot_Derived_From_RxVar<T>(this IObservable<T> obs, [CallerArgumentExpression(nameof(obs))] string? obsName = null)
	{
		if (!obs.IsObservableDerivedFromRxVar()) throw new ArgumentException($"This Observable: '{obsName}' is not derived from an RxVar (it does not provide a value immediately)");
		return obs;
	}

	private static bool IsObservableDerivedFromRxVar<T>(this IObservable<T> obs)
	{
		var res = false;
		using var _ = obs.Subscribe(_ => res = true);
		return res;
	}

	private static void L(string s) => Console.WriteLine(s);
}