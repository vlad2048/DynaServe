using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.FileServing.Structs;

enum MountJsFlags
{
	ServeOnly,
	AddScript,
	AddModuleScript
}

record Mount(
	string Filename,
	string MountLocation,
	(string, string)[] Substs,
	MountJsFlags? JsFlags
)
{
	public FCat Cat => Filename.ToCat();
}

/*interface IMount
{
	FCat Cat { get; }
	string Filename { get; }
	string MountLocation { get; }
}

record 



/*
interface IServNfo
{
	FCat Cat { get; }
}

record LocalFolderServNfo(
	FCat Cat,
	string FuzzyFolder
) : IServNfo;

record LocalFileServNfo(
	string FuzzyFolder,
	string File,
	(string, string)[] Substitutions
) : IServNfo
{
	public FCat Cat => File.ToCat();
}

record DirectFileServNfo(
	FCat Cat,
	string Name,
	byte[] Content
) : IServNfo
{
	public static DirectFileServNfo FromString(FCat cat, string name, string contentString, params (string, string)[] substitutions)
	{
		foreach (var (key, val) in substitutions)
			contentString = contentString.Replace(key, val);
		return new DirectFileServNfo(cat, name, contentString.ToBytes());
	}

	public string Link
	{
		get
		{
			if (Name.Count(e => e == '.') <= 1) return Name.ToLink();
			var idx = Name.LastIndexOf('.');
			if (idx == -1) throw new ArgumentException();
			var t = Name[..idx];
			idx = t.LastIndexOf('.');
			if (idx == -1) throw new ArgumentException();
			return Name[(idx + 1)..].ToLink();
		}
	}
}




record ServFold(
	string Folder,
	FCat FCat
);

record ServFile(
	string File,
	(string, string)[] Substitutions
);


record WatchFold(
	string Folder,
	string Pattern,
	FileReg[] Regs
);
*/