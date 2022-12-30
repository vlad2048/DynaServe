using System.Reactive.Linq;
using PowMaybe;
using PowRxVar;

namespace DynaServeExtrasLib.Components.EditListLogic.EditListConstraints;

public record OwnsMultipleSlavesLinkNfo<TSlave>(
	IRwVar<TSlave[]> Slaves,
	IObservable<bool> WhenSlaveCanAdd
);

public static class EditListConstraintMaker
{
	public static (OwnsMultipleSlavesLinkNfo<TSlave>, IDisposable) OwnsMultiple_And_EditTheSlavesOfTheSelectedMaster<TMaster, TSlave>(
		EditList<TMaster> listMaster,
		Func<TMaster, TSlave[]> getFun,
		Func<TMaster, TSlave[], TMaster> setFun
	)
	{
		var d = new Disp();

		var editedSlaves = Var.MakeBnd(Array.Empty<TSlave>()).D(d);

		// User changes SelMaster => EditSlaves.Inner
		listMaster.SelItem
			.SelectVar(mayMaster =>
				mayMaster
					.Select(getFun)
					.FailWith(Array.Empty<TSlave>())
			)
			.PipeToInner(editedSlaves);

		// User edits Slaves => EditSlaves.Outer => Update SelMaster
		editedSlaves
			.WhenOuter
			.Subscribe(slaves => listMaster.TransformSelItemEnsure(master => setFun(master, slaves))).D(d);

		var linkNfo = new OwnsMultipleSlavesLinkNfo<TSlave>(
			Slaves: editedSlaves.ToRwBndVar(),
			WhenSlaveCanAdd: listMaster.SelItem.Select(e => e.IsSome())
		);

		return (linkNfo, d);
	}
}