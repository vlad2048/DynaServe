using DynaServeLib.Serving.FileServing.StructsEnum;
using System.Reflection;
using DynaServeLib.Utils;
using PowBasics.CollectionsExt;

namespace DynaServeLib.Serving.FileServing.Utils;

static class FileFinder
{
    private static readonly Cache fileCache = new(t => Directory.GetFiles(t, "*.*", SearchOption.AllDirectories).WhereNotToArray(e => e.Split('\\').Any(forbiddenDirNames.Contains)));
    private static readonly Cache foldCache = new(t => Directory.GetDirectories(t, "*.*", SearchOption.AllDirectories).WhereNotToArray(e => e.Split('\\').Any(forbiddenDirNames.Contains)));
    private static readonly string[] forbiddenDirNames = { "_old" };

    public static string GetSearchRoot(string srcFile, string? searchFolder) =>
	    (Path.GetFileName(srcFile) == "LINQPadQuery") switch
	    {
		    true => searchFolder ?? throw new ArgumentException("You're running in LINQPad, you need to set opt.SearchFolder to find resources"),
		    false => FindPrjFolder(srcFile)
	    };

    public static string[] ResolveShort(
        string @short,
        Assembly? ass,
        FCat cat,
        ServRange range,
        string searchFolder
    ) =>
        ResolveShortAsFile(@short, cat, range, searchFolder)
            .SelectToArray(file => file.File2EmbeddedIFN(ass));


    private static string[] ResolveShortAsFile(
        string @short,
        FCat cat,
        ServRange range,
        string searchFolder
    )
    {
        switch (range)
        {
            case ServRange.File:
                return new[] { FindFile(@short, searchFolder) };

            case ServRange.Folder:
            case ServRange.FolderWithRecursion:
                var isRecursive = range == ServRange.FolderWithRecursion;
                var folder = FindFolder(@short, searchFolder);
                return folder.ListFiles(cat, isRecursive);

            default:
                throw new ArgumentException($"Invalid ServRange: {range.GetType().Name}");
        }
    }

    private static string FindFile(string shortFile, string searchFolder)
    {
        var matches = (
            from file in fileCache.Get(searchFolder)
            where file.EndsWith($@"\{shortFile}")
            select file
        ).ToArray();

        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new ArgumentException($"Mount error. Cannot find file '{shortFile}'"),
            _ => throw new ArgumentException(GetAmbiguousErrorMessage(shortFile, matches, "files")),
        };
    }

    private static string FindFolder(string shortFolder, string searchFolder)
    {
        var matches = (
            from fold in foldCache.Get(searchFolder)
            where fold.EndsWith($@"\{shortFolder}")
            select fold
        ).ToArray();

        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new ArgumentException($"Mount error. Cannot find folder '{shortFolder}'"),
            _ => throw new ArgumentException(GetAmbiguousErrorMessage(shortFolder, matches, "folders")),
        };
    }




	

    private static string FindPrjFolder(string srcFile)
    {
	    var folder = Path.GetDirectoryName(srcFile)!;
	    while (folder != null && Directory.GetFiles(folder, "*.csproj").Length == 0)
		    folder = Path.GetDirectoryName(folder);
	    return folder ?? throw new ArgumentException($"Cannot find project folder containing: '{srcFile}'");
    }


    private static string[] ListFiles(this string folder, FCat cat, bool isRecursive) => (
        from type in cat.ToFTypes()
        from ext in type.ToExts()
        let pattern = $"*{ext}"
        from file in Directory.GetFiles(folder, pattern, isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
        select file
    ).ToArray();



    private static string GetAmbiguousErrorMessage(string @short, string[] matches, string typeStr)
    {
        var pad = matches.Max(e => e.Length);
        return $"""
				Mount error. Found too many {typeStr} ({matches.Length}x) '{@short}'
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

