using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowRxVar;

namespace DynaServeLib.Utils.Exts;

static class RxBufferSubTypeExt
{
	public static ((IObservable<T>, IObservable<B[]>), IDisposable) BufferSubType<T, B>(this IObservable<T> obs, TimeSpan period) where T : class where B : T
	{
		var d = new Disp();
		var whenElt = new Subject<T>().D(d);
		var whenBuf = new Subject<B[]>().D(d);
	
		var whenClose =
			Observable.Amb(
					obs.Where(e => e is not B).Take(1).ToUnit(),
					Observable.Timer(period).ToUnit()
				)
				.Repeat();
	
		obs
			.Buffer(whenClose)
			.Select(e => e.ToArray())
			.Where(e => e.Any())
			.Subscribe(arr =>
			{
				var buf = arr.TakeWhile(e => e is B).Select(e => (B)e).ToArray();
				if (buf.Any())
				{
					whenBuf.OnNext(buf);
				}

				foreach (var evt in arr.Skip(buf.Length))
				{
					whenElt.OnNext(evt);
				}
			}).D(d);
	
		return ((whenElt.AsObservable(), whenBuf.AsObservable()), d);
	}
}