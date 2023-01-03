using PowMaybe;

namespace DynaServeLib.Utils;

static class MaybeDynaExt
{
	public static T EnsureEx<T>(this Maybe<T> may, Exception ex) => may.IsSome(out var val) switch
	{
		true => val!,
		false => throw ex
	};
}