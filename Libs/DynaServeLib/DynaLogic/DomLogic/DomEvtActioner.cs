using System.Reactive.Linq;
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.DynaLogic.DomLogic;

static class DomEvtActioner
{
	private static readonly TimeSpan bufferPeriod = TimeSpan.FromMilliseconds(5);

	public static IDisposable Setup(
		IObservable<IDomEvt> whenDomEvt,
		DomOps domOps
	)
	{
		var d = new Disp();

		var whenDomEvtSync = whenDomEvt.Synchronize();

		var (whenEvtElt, whenEvtBuf) = whenDomEvtSync.BufferSubType<IDomEvt, ChgDomEvt>(bufferPeriod).D(d);
		//var (whenEvtElt, whenEvtBuf) = (whenDomEvtSync, Observable.Never<ChgDomEvt[]>());

		whenEvtElt.Subscribe(evt =>
		{
			switch (evt)
			{
				case UpdateChildrenDomEvt e:
					domOps.Handle_UpdateChildrenDomEvt(e);
					break;

				case ChgDomEvt e:
					// TODO: this should never be called
					// throw new ArgumentException();
					domOps.Handle_ChgDomEvt(new [] { e });
					break;

				case AddBodyNodeDomEvt e:
					domOps.Handle_AddBodyNodeDomEvt(e);
					break;

				case RemoveBodyNodeDomEvt e:
					domOps.Handle_RemoveBodyNodeDomEvt(e);
					break;

				default:
					throw new ArgumentException();
			}

			//domOps.Logr.LogTransition($"DomEvt[{evt.GetType().Name}] {evt}", domOps.Dom.FmtBody());
		}).D(d);


		whenEvtBuf.Subscribe(evtBuf =>
		{
			domOps.Handle_ChgDomEvt(evtBuf);
			var str = evtBuf.Select(e => $"{e}").JoinText(";");
			//domOps.Logr.LogTransition($"DomEvt[--Batch--] {str}", domOps.Dom.FmtBody());
		}).D(d);


		return d;
	}



	/*public static IDisposable Setup(
		IObservable<IDomEvt> whenDomEvt,
		DomOps domOps
	)
	{
		var d = new Disp();

		whenDomEvt
			.Synchronize()
			.Subscribe(evt =>
			{
				switch (evt)
				{
					case UpdateChildrenDomEvt e:
						domOps.Handle_UpdateChildrenDomEvt(e);
						break;

					case ChgDomEvt e:
						domOps.Handle_ChgDomEvt(e);
						break;

					case AddBodyNodeDomEvt e:
						domOps.Handle_AddBodyNodeDomEvt(e);
						break;

					case RemoveBodyNodeDomEvt e:
						domOps.Handle_RemoveBodyNodeDomEvt(e);
						break;

					default:
						throw new ArgumentException();
				}
			}).D(d);

		return d;
	}*/
}