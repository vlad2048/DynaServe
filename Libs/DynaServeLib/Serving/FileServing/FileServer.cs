using DynaServeLib.DynaLogic;
using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.Logic;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Utils;
using PowBasics.CollectionsExt;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Serving.FileServing;

class FileServer : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly IReadOnlyList<IServNfo> servNfos;
	private readonly string scssOutputFolder;
	private readonly DomOps domOps;
	private readonly IEnumerable<string>? linqPadRefs;
	private readonly ILogr logr;
	private readonly Dictionary<string, IReg> regMap = new();

	public FileServer(
		DomOps domOps,
		IReadOnlyList<IServNfo> servNfos,
		string scssOutputFolder,
		IEnumerable<string>? linqPadRefs,
		ILogr logr
	)
	{
		this.servNfos = servNfos;
		this.scssOutputFolder = scssOutputFolder;
		this.domOps = domOps;
		this.linqPadRefs = linqPadRefs;
		this.logr = logr;
	}

	public void Start()
	{
		var localFolds = servNfos.OfType<LocalFolderServNfo>().ToArray();
		var directFiles = servNfos.OfType<DirectFileServNfo>().ToArray();

		SetupLocalFolders(localFolds);
		SetupDirectFiles(directFiles);
		//LogRegs();
	}

	public async Task<Maybe<RegData>> TryGetContent(string link)
	{
		link = link.RemoveQueryParams();
		if (!regMap.TryGetValue(link, out var reg)) return May.None<RegData>();
		return await reg.GetContent();
	}



	private void SetupLocalFolders(LocalFolderServNfo[] localFolds)
	{
		var folds = FuzzyFolderFinder.Find(localFolds, linqPadRefs);
		var filesToLink = GetFilesToLink(folds);
		var filesToServe = GetFilesToServe(folds);
		var foldersToCompile = folds.Where(e => e.FCat == FCat.Css).SelectToArray(e => e.Folder);

		// Create links
		// ============
		LinkCreator.CreateLink(domOps, filesToLink);

		// Compile scss
		// ============
		ScssWatchCompiler.Start(foldersToCompile, scssOutputFolder, logr).D(d);

		// Serve files
		// ===========
		var regs = filesToServe.SelectToArray(e => new FileReg(e));
		foreach (var reg in regs)
			regMap[reg.Filename.ToLink()] = reg;

		// Watch files
		// ===========
		var watchFolds = GetWatchFolds(regs);
		LiveReloader.Setup(watchFolds, domOps).D(d);
	}


	private WatchFold[] GetWatchFolds(FileReg[] regs) =>
		regs
			.GroupBy(e => new
			{
				Folder = Path.GetDirectoryName(e.Filename)!,
				Ext = Path.GetExtension(e.Filename)
			})
			.SelectToArray(grp => new WatchFold(
				grp.Key.Folder,
				grp.Key.Ext,
				grp.ToArray()
			));

	private void SetupDirectFiles(DirectFileServNfo[] files)
	{
		var filesToLink = files
			.Where(e => e.Cat.NeedsLinking())
			.SelectToArray(e => new ServLinkFile(e.Name, e.Cat));

		// Create links
		// ============
		LinkCreator.CreateLink(domOps, filesToLink);

		// Serve files
		// ===========
		foreach (var file in files)
		{
			var link = file.Name.ToLink();
			regMap[link] = new DirectReg(file.Name, file.Content);
		}
	}

	private string[] GetFilesToServe(ServFold[] folds) => (
		from fold in folds
		from fileType in fold.FCat.ToFTypes()
		from fileExt in fileType.ToExts()
		from file in Directory.GetFiles(fold.Folder, $"*{fileExt}")
		select file.TransposeFileToCompiledFolderIFN(scssOutputFolder)
	).ToArray();


	private ServLinkFile[] GetFilesToLink(ServFold[] folds) => (
		from fold in folds
		where fold.FCat.NeedsLinking()
		from fileType in fold.FCat.ToFTypes()
		from fileExt in fileType.ToExts()
		from file in Directory.GetFiles(fold.Folder, $"*{fileExt}")
		let finalFile = file.TransposeFileToCompiledFolderIFN(scssOutputFolder)
		select new ServLinkFile(finalFile, fold.FCat)
	).ToArray();


	/*private void LogRegs()
	{
		static void L(string s) => Console.WriteLine(s);

		foreach (var (link, reg) in regMap)
		{
			var sb = new StringBuilder($"'{link}'".PadRight(60));
			switch (reg)
			{
				case FileReg e:
					//sb.Append($"exists:{File.Exists(e.Filename)}  ({e.Filename})");
					sb.Append($"name:{e.Name}  type:{e.Name.ToType()}  mime:{e.Name.ToType().ToMime()}");
					break;
				case DirectReg e:
					//sb.Append($"name:'{e.Name}'");
					break;
			}
			L(sb.ToString());
		}
	}*/
}