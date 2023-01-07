using System.Reactive.Disposables;
using PowRxVar;

namespace DynaServeLib.Utils.Exts;

public static class DictExt
{
	public static IReadOnlyDictionary<K, V> MergeEnsure<K, V>(this IEnumerable<IReadOnlyDictionary<K, V>> source) where K : notnull
	{
		var res = new Dictionary<K, V>();
		foreach (var dict in source)
		foreach (var (k, v) in dict)
		{
			if (res.ContainsKey(k)) throw new ArgumentException();
			res[k] = v;
		}
		return res;
	}

	public static Dictionary<K, V> D<K, V>(this Dictionary<K, V> dict, IRoDispBase d) where K : notnull where V : IDisposable
	{
		Disposable.Create(() =>
		{
			foreach (var (_, val) in dict)
				val.Dispose();
		}).D(d);
		return dict;
	}

	public static List<T> D<T>(this List<T> list, IRoDispBase d) where T : IDisposable
	{
		Disposable.Create(() =>
		{
			foreach (var elt in list)
				elt.Dispose();
		}).D(d);
		return list;
	}
}