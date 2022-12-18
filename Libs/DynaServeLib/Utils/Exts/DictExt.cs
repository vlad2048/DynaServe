using System.Reactive.Disposables;
using PowRxVar;

namespace DynaServeLib.Utils.Exts;

public static class DictExt
{
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