using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.FileServing.Structs;

interface IServNfo
{
	FCat Cat { get; }
}

record LocalFolderServNfo(
	FCat Cat,
	string FuzzyFolder
) : IServNfo;

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


record ServLinkFile(
	string Filename,
	FCat Cat
);


record WatchFold(
	string Folder,
	string Ext,
	FileReg[] Regs
);