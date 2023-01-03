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
	private readonly IReadOnlyList<string> slnFolders;
	private readonly ILogr logr;
	private readonly Dictionary<string, IReg> regMap = new();

	public FileServer(
		DomOps domOps,
		IReadOnlyList<IServNfo> servNfos,
		string scssOutputFolder,
		IReadOnlyList<string> slnFolders,
		ILogr logr
	)
	{
		this.servNfos = servNfos;
		this.scssOutputFolder = scssOutputFolder;
		this.domOps = domOps;
		this.slnFolders = slnFolders;
		this.logr = logr;
	}

	public void Start()
	{
		var localFoldNfos = servNfos.OfType<LocalFolderServNfo>().ToArray();
		var (folds, foldsNotFound) = FuzzyFolderFinder.Find(localFoldNfos, slnFolders);


		var directFiles = servNfos.OfType<DirectFileServNfo>().ToArray();

		SetupLocalFolders(folds);
		SetupDirectFiles(directFiles);
		LogFoldsNotFound(foldsNotFound);
	}

	private void LogFoldsNotFound(LocalFolderServNfo[] folds)
	{
		if (folds.Length == 0) return;
		void L(string s) => Console.Error.WriteLine(s);
		var title = $"{folds.Length} folders not found:";
		L(title);
		L(new string('=', title.Length));
		foreach (var fold in folds)
			L($"  {fold.Cat} - '{fold.FuzzyFolder}'");
		L("");
		L("You need to specify the solution folders they are located in:");
		L("""    opt.AddSlnFolder(@"C:\git\MySolution");""");
		throw new ArgumentException("Resources not found");
	}

	public async Task<Maybe<RegData>> TryGetContent(string link)
	{
		link = link.RemoveQueryParams();
		if (!regMap.TryGetValue(link, out var reg)) return May.None<RegData>();
		return await reg.GetContent();
	}

	private void SetupLocalFolders(ServFold[] folds)
	{
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
			var link = file.Link;
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
}