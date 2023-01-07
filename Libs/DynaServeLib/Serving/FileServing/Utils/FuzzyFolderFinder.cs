using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;

namespace DynaServeLib.Serving.FileServing.Utils;

record FolderFindResult(
	ServFold[] Folds,
	LocalFolderServNfo[] FoldsNotFound
);

static class FuzzyFolderFinder
{
	public static (IReadOnlyDictionary<string, string>, string[]) MakeFolderMap(IEnumerable<LocalFileServNfo> fileNfos, IReadOnlyList<string> slnFolders)
	{
		var foldersToFind = fileNfos.Select(e => e.FuzzyFolder).Distinct();
		var foldersNotFound = new List<string>();
		var map = new Dictionary<string, string>();
		foreach (var folderToFind in foldersToFind)
		{
			var res = Find(new[] { new LocalFolderServNfo(0, folderToFind) }, slnFolders);
			if (res.Folds.Length > 1) throw new ArgumentException();
			if (res.Folds.Length == 1)
				map[folderToFind] = res.Folds[0].Folder;
			foldersNotFound.AddRange(res.FoldsNotFound.Select(e => e.FuzzyFolder));
		}
		return (map, foldersNotFound.ToArray());
	}

	public static FolderFindResult Find(IEnumerable<LocalFolderServNfo> foldNfosSource, IReadOnlyList<string> slnFolders)
	{
		var foldNfos = foldNfosSource.ToArray();
		var fuzzyFolderSet = foldNfos.SelectToHashSet(e => e.FuzzyFolder);
		var folderMap = (
				from slnFolder in slnFolders
				from dir in Directory.GetDirectories(slnFolder, "*.*", SearchOption.AllDirectories)
				let dirName = Path.GetFileName(dir)
				where fuzzyFolderSet.Contains(dirName)
				select (dirName, dir)
			)
			.Where(t => t.dirName != null)
			.Select(t => (dirName: t.dirName!, t.dir))
			.GroupBy(t => t.dirName)
			.Select(grp => grp.First())
			.ToDictionary(t => t.dirName, t => t.dir);
		var folds = foldNfos
			.Where(e => folderMap.ContainsKey(e.FuzzyFolder))
			.SelectToArray(e => new ServFold(folderMap[e.FuzzyFolder], e.Cat));
		var foldsNotFound = foldNfos
			.WhereToArray(e => !folderMap.ContainsKey(e.FuzzyFolder));
		return new FolderFindResult(
			folds,
			foldsNotFound
		);
	}
}
