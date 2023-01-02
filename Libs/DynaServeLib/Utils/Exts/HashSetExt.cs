namespace DynaServeLib.Utils.Exts;

static class HashSetExt
{
	public static HashSet<U> SelectToHashSet<T, U>(this IEnumerable<T> source, Func<T, U> mapFun) =>
		source
			.Select(mapFun)
			.ToHashSet();
}