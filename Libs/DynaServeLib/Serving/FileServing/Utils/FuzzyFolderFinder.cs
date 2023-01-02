using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowMaybe;

namespace DynaServeLib.Serving.FileServing.Utils;

static class FuzzyFolderFinder
{
	private const string SlnName = @"DynaServe";


	public static ServFold[] Find(LocalFolderServNfo[] folds, IEnumerable<string>? linqPadRefs)
	{
		var fuzzyFolderSet = folds.SelectToHashSet(e => e.FuzzyFolder);
		var folderMap = (
				from dir in Directory.GetDirectories(FindSlnFolder(linqPadRefs), "*.*", SearchOption.AllDirectories)
				let dirName = Path.GetFileName(dir)
				where fuzzyFolderSet.Contains(dirName)
				select (dirName, dir)
			)
			.Where(t => t.dirName != null)
			.Select(t => (dirName: t.dirName!, t.dir))
			.ToDictionary(t => t.dirName, t => t.dir);
		return folds.SelectToArray(e => new ServFold(folderMap[e.FuzzyFolder], e.Cat));
	}



	private static string FindSlnFolder(IEnumerable<string>? LINQPad_Util_CurrentQuery_FileReferences = null)
	{
		var slnFolder = LINQPad_Util_CurrentQuery_FileReferences switch
		{
			null => FindFrom(Environment.CurrentDirectory).Ensure(),
			not null => FindFrom(LINQPad_Util_CurrentQuery_FileReferences.First(e => e.Contains(SlnName))).Ensure(),
		};
		var slnFile = Path.Combine(slnFolder, $"{SlnName}.sln");
		if (!File.Exists(slnFile)) throw new ArgumentException();
		return slnFolder;
	}


	private static Maybe<string> FindFrom(string folder) =>
		from dllFile in May.Some(folder)
		from idx in dllFile.IndexOfMaybe(SlnName)
		select dllFile[..(idx + 9)];


	private static Maybe<int> IndexOfMaybe(this string str, string s)
	{
		var idx = str.IndexOf(s, StringComparison.Ordinal);
		return (idx != -1) switch
		{
			true => May.Some(idx),
			false => May.None<int>()
		};
	}

}