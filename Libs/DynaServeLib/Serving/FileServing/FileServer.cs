using DynaServeLib.DynaLogic;
using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Serving.FileServing;

class FileServer : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IReadOnlyList<Mount> mounts;
	private readonly DomOps domOps;
	private readonly ILogr logr;
	private readonly Dictionary<string, LoadedMount> loadedMounts;

	public FileServer(
		DomOps domOps,
		ILogr logr,
		IReadOnlyList<Mount> mounts
	)
	{
		this.logr = logr;
		this.domOps = domOps;
		this.mounts = mounts.Distinct().ToArray();
		loadedMounts = new Dictionary<string, LoadedMount>().D(d);
	}

	public void Start()
	{
		foreach (var mount in mounts)
			Mount(mount);
		//LogRegs();
	}

	private void Mount(Mount mount)
	{
		var scriptNfo = mount.GetScriptNfo();
		if (scriptNfo != null)
			domOps.Dom.AddScript(scriptNfo);
		var loadedMount = new LoadedMount(
			mount,
			logr,
			() =>
			{
				if (scriptNfo == null) return;
				domOps.SendToClient(new ScriptRefreshServerMsg(scriptNfo));
			}
		);

		loadedMounts[mount.Link] = loadedMount;

		if (mount.Cat == FCat.Js)
		{
			if (!mount.Link.EndsWith(".js")) throw new ArgumentException();
			loadedMounts[mount.Link[..^3]] = loadedMount;
		}
	}
	
	public async Task<Maybe<Reply>> TryGetReply(string link)
	{
		link = link.RemoveQueryParams();
		if (!loadedMounts.TryGetValue(link, out var loadedMount))
			return May.None<Reply>();
		return await loadedMount.GetReply();
	}


	private void LogRegs()
	{
		var maxLng = loadedMounts.Keys.Max(e => e.Length);
		foreach (var (link, reg) in loadedMounts)
		{
			L($"{link.PadRight(maxLng)} -> {reg}");
		}
	}
	private static void L(string s) => Console.Error.WriteLine(s);
}