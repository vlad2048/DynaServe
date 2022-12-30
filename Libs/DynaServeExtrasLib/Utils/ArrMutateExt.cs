namespace DynaServeExtrasLib.Utils;

public static class ArrMutateExt
{
	public static T[] ArrAdd<T>(this T[] arr, T elt) { var list = arr.ToList(); list.Add(elt); return list.ToArray(); }
	public static T[] ArrDel<T>(this T[] arr, T elt) { var list = arr.ToList(); var idx = list.IndexOf(elt); if (idx == -1) throw new ArgumentException("element not found"); list.RemoveAt(idx); return list.ToArray(); }
	public static T[] ArrRepl<T>(this T[] arr, T eltPrev, T eltNext) { var list = arr.ToList(); var idx = list.IndexOf(eltPrev); if (idx == -1) throw new ArgumentException("element not found"); list.RemoveAt(idx); list.Insert(idx, eltNext); return list.ToArray(); }
	public static T[] ArrToggle<T>(this T[] arr, T elt) => arr.Contains(elt) switch
	{
		false => arr.ArrAdd(elt),
		true => arr.ArrDel(elt)
	};
	
	public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		var idx = 0;
		foreach (var elt in source)
		{
			if (predicate(elt)) return idx;
			idx++;
		}
		throw new ArgumentException("Cannot find element");
	}
	
	private static readonly Random random = new();

	public static T[] Shuffle<T>(this T[] array)
	{
		var n = array.Length;
		for (var i = 0; i < n; i++)
		{
			var r = i + random.Next(n - i);
			(array[r], array[i]) = (array[i], array[r]);
		}
		return array;
	}
}