using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Serving.FileServing.Structs;

class LoadedMount : IDisposable
{
	private static readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(200);

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Mount mount;
	private readonly Func<string, Task<string>>? compileFun;
	private Maybe<Reply> mayReply = May.None<Reply>();

	public LoadedMount(Mount mount, ILogr logr, Action onInvalidate)
	{
		this.mount = mount;
		compileFun = mount.GetCompileFun(logr);

		if (mount.Src is FileMountSrc fileSrc)
		{
			var watcher = MakeWatcher(fileSrc.Filename).D(d);
			watcher.WhenChange.Subscribe(_ =>
			{
				mayReply = May.None<Reply>();
				onInvalidate.Invoke();
			}).D(d);
		}
	}

	public override string ToString() => $"{mount}";

	public async Task<Maybe<Reply>> GetReply()
	{
		if (mayReply.IsNone())
		{
			var reply = mount.Src switch
			{
				FileMountSrc e => await Reply.LoadFromFile(mount.Type, e.Filename, mount.Substs, compileFun),
				EmbeddedMountSrc e => await Reply.LoadFromEmbedded(mount.Type, e.EmbeddedName, e.Ass, mount.Substs, compileFun),
				StringMountSrc e => await Reply.LoadFromString(mount.Type, e.String, mount.Substs, compileFun),
				_ => throw new ArgumentException()
			};
			mayReply = May.Some(reply);
		}

		return mayReply;
	}

	private static FolderWatcher MakeWatcher(string filename) =>
		new(Path.GetDirectoryName(filename)!, Path.GetFileName(filename), debounceTime);
}