/*
using System.Reactive.Linq;
using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.Serving.FileServing.Logic;

static class LiveReloader
{
	private static readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(200);

	public static IDisposable Setup(
		WatchFold[] folds,
		DomOps domOps
	)
	{
		var d = new Disp();

		var watchers = folds.SelectToArray(fold =>
			new FolderWatcher(fold.Folder, fold.Pattern, debounceTime).D(d)
		);
		Observable.Merge(
			watchers.Select((watcher, idx) =>
				watcher.WhenChange
					.Select(file => (file, folds[idx]))
			)
		)
			.Subscribe(evt =>
			{
				var (file, fold) = evt;
				var reg = fold.Regs.FirstOrDefault(e => AreNamesEqual(e.Filename, file));
				if (reg == null) return;
				reg.Invalidate();

				var cat = reg.Filename.ToCat();
				var link = reg.Filename.ToLink();

				switch (cat)
				{
					case FCat.Css:
						domOps.SendToClient(new ScriptRefreshServerMsg(ScriptType.Css, link));
						break;

					case FCat.Js:
						domOps.SendToClient(new ScriptRefreshServerMsg(ScriptType.Js, link));
						break;

					case FCat.Image:
						domOps.BumpImageUrl(link);
						break;
				}
			}).D(d);


		return d;
	}

	private static bool AreNamesEqual(string f1, string f2) => Path.GetFileName(f1) == Path.GetFileName(f2);
}
*/
