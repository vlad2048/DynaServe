namespace DynaServeLib.Serving.FileServing.Utils;

static class FileFinder
{
	private static readonly Cache fileCache = new(t => Directory.GetFiles(t, "*.*", SearchOption.AllDirectories));
	private static readonly Cache foldCache = new(t => Directory.GetDirectories(t, "*.*", SearchOption.AllDirectories));
	
	public static string FindFile(string shortFilename, IReadOnlyList<string> searchFolders)
	{
		var matches = (
			from searchFolder in searchFolders
			from file in fileCache.Get(searchFolder)
			select file
		).ToArray();

		return matches.Length switch
		{
			1 => matches[0],
			0 => throw new ArgumentException($"Mount error. Cannot find file '{shortFilename}'"),
			_ => throw new ArgumentException(GetAmbiguousErrorMessage(shortFilename, matches, "files")),
		};
	}
	
	public static string FindFolder(string shortFoldername, IReadOnlyList<string> searchFolders)
	{
		var matches = (
			from searchFolder in searchFolders
			from fold in foldCache.Get(searchFolder)
			select fold
		).ToArray();

		return matches.Length switch
		{
			1 => matches[0],
			0 => throw new ArgumentException($"Mount error. Cannot find folder '{shortFoldername}'"),
			_ => throw new ArgumentException(GetAmbiguousErrorMessage(shortFoldername, matches, "folders")),
		};
	}

	
	private static string GetAmbiguousErrorMessage(string shortFilename, string[] matches, string typeStr)
	{
		var pad = matches.Max(e => e.Length);
		return $"""
				Mount error. Found too many {typeStr} ({matches.Length}x) '{shortFilename}'
				Matches:
				
				""" + string.Join(Environment.NewLine, matches.Select(e => $"\t{e.PadLeft(pad)}"));
	}

	private class Cache
	{
		private readonly Dictionary<string, string[]> cache = new();
		private readonly Func<string, string[]> fun;

		public Cache(Func<string, string[]> fun)
		{
			this.fun = fun;
		}

		public string[] Get(string key)
		{
			if (!cache.TryGetValue(key, out var arr))
				arr = cache[key] = fun(key);
			return arr;
		}
	}
}

