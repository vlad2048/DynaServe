using PowMaybe;

namespace DynaServeExtrasLib.Utils;

public static class MaybeExt
{
	public static bool IsSomeAndEqualTo<T>(this Maybe<T> may, T val) => may.IsSome(out var mayVal) switch
	{
		true => mayVal!.Equals(val),
		false => false
	};

	public static bool IsSomeAndVerifies<T>(this Maybe<T> may, Func<T, bool> predicate) => may.Select(predicate).FailWith(false);
}