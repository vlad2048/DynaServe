using PowBasics.CollectionsExt;

namespace DynaServeLib.Utils.Exts;

public static class ArrExt
{
	public static (T[], T[]) SplitInTwo<T>(this T[] arr, Func<T, bool> predicate) => (
		arr.WhereToArray(predicate),
		arr.WhereNotToArray(predicate)
	);

	public static T[] ConcatArr<T>(this T[] a, T[] b) => a.Concat(b).ToArray();

	//public static T[] AppendArr<T>(this T[] arr, T elt) => arr.Append(elt).ToArray();
}