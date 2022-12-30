using System.Reactive.Linq;
using PowBasics.CollectionsExt;
using PowMaybe;
using PowRxVar;

namespace DynaServeExtrasLib.Components.EditListLogic.EditListConstraints;

public record OwnsMultipleLinkNfo<TSlave>(
	IRwVar<TSlave[]> Slaves,
	IObservable<bool> WhenSlaveCanAdd
);

public static class EditListConstraintMaker
{
	public static (OwnsMultipleLinkNfo<TSlave>, IDisposable) OwnsMultiple<TMaster, TSlave>(
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

		var linkNfo = new OwnsMultipleLinkNfo<TSlave>(
			Slaves: editedSlaves.ToRwBndVar(),
			WhenSlaveCanAdd: listMaster.SelItem.Select(e => e.IsSome())
		);

		return (linkNfo, d);
	}


	public static IDisposable ReferencesMultiple<TMaster, TSlave, TSlaveKey>(
		EditList<TMaster> listMaster,
		EditList<TSlave> listSlave,
		Func<TSlave, TSlaveKey> slaveKeyFun,
		Func<TMaster, TSlaveKey[]> getFun,
		Func<TMaster, TSlaveKey[], TMaster> setFun
	) =>
		listSlave.List
			.Subscribe(slavesExisting =>
			{
				var slavesExistingKeys = slavesExisting.SelectToArray(slaveKeyFun);
				var slavesReferencedKeys = (
					from master in listMaster.List.V
					from slaveKey in getFun(master)
					select slaveKey
				).Distinct().ToArray();
				var slaveKeysToRemove = slavesReferencedKeys.WhereNotToArray(slavesExistingKeys.Contains);
				foreach (var masterPrev in listMaster.List.V)
				{
					var masterSlavesPrev = getFun(masterPrev);
					if (masterSlavesPrev.Any(slaveKeysToRemove.Contains))
					{
						var masterSlavesNext = masterSlavesPrev.WhereNotToArray(slaveKeysToRemove.Contains);
						var masterNext = setFun(masterPrev, masterSlavesNext);
						listMaster.ReplaceItem(masterPrev, masterNext);
					}
				}
			});
}