using System.Reactive.Concurrency;
using System.Reactive.Linq;
using PowRxVar;

namespace DynaServeLib.Utils;

public class FolderWatcher : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	public IObservable<string> WhenChange { get; }

	public FolderWatcher(string folder, string filter, TimeSpan debounceTime)
	{
		var watcher = new FileSystemWatcher(folder)
		{
			IncludeSubdirectories = false,
			NotifyFilter = NotifyFilters.LastWrite,
			Filter = filter,
		}.D(d);

		WhenChange = Observable.FromEventPattern<FileSystemEventArgs>(watcher, "Changed")
			.Select(e => e.EventArgs)
			.Where(e => e.Name != null)
			.Select(e => e.FullPath)
			.Throttle(debounceTime, TaskPoolScheduler.Default);

		watcher.EnableRaisingEvents = true;
	}
}