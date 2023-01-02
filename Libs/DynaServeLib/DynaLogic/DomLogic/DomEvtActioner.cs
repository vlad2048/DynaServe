using System.Reactive.Linq;
using DynaServeLib.DynaLogic.Events;
using PowRxVar;

namespace DynaServeLib.DynaLogic.DomLogic;

static class DomEvtActioner
{
	public static IDisposable Setup(
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

					case PropChangeDomEvt e:
						domOps.Handle_PropChangeDomEvt(e);
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
	}
}