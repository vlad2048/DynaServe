using System.Reflection;
using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Serving.Syncing.Structs;

namespace DynaServeLib.Serving.FileServing.Structs;

interface IMountSrc
{
	string Name { get; }
}
record FileMountSrc(string Filename) : IMountSrc
{
	public string Name => Path.GetFileName(Filename);
	public override string ToString() => $"file:'{Filename}'";
}
record EmbeddedMountSrc(string EmbeddedName, Assembly Ass) : IMountSrc
{
	public string Name => Path.GetFileName(EmbeddedName);
	public override string ToString() => $"embedded:'{EmbeddedName}'";
}

record StringMountSrc(string String, string Name) : IMountSrc
{
	public override string ToString() => $"string: name='{Name}' (length={String.Length})";
}

static class MountSrcExt
{
	public static FType Type(this IMountSrc src) => src.Name.ToType();
	public static FCat Cat(this IMountSrc src) => src.Type().ToCat();
	public static bool IsBinary(this IMountSrc src) => src.Type().IsBinary();
}

public enum MountJsFlags
{
	ServeOnly,
	AddScript,
	AddModuleScript
}

class Mount : IEquatable<Mount>
{
	public IMountSrc Src { get;}
	public string MountLocation { get; }
	public (string, string)[] Substs { get; }
	public MountJsFlags? JsFlags { get; }

	public Mount(
		IMountSrc src,
		string mountLocation,
		(string, string)[] substs,
		MountJsFlags? jsFlags
	)
	{
		Src = src;
		MountLocation = mountLocation;
		Substs = substs;
		JsFlags = jsFlags;

		if (Cat == FCat.Js && JsFlags == null) throw new ArgumentException("You need to specify jsFlags when mounting .js files");
		if (IsBinary && substs.Any()) throw new ArgumentException("You cannot apply text substitutions on binary files");
	}

	public override string ToString() => $"{Src}";

	public FType Type => Src.Type();
	public FCat Cat => Src.Cat();
	public bool IsBinary => Src.IsBinary();
	public string Link => (MountLocation == string.Empty) switch
	{
		true => Src.Name,
		false => $"{MountLocation}/{Src.Name}"
	};

	public bool Equals(Mount? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Src.Equals(other.Src);
	}
	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Mount)obj);
	}
	public override int GetHashCode() => Src.GetHashCode();
	public static bool operator ==(Mount? left, Mount? right) => Equals(left, right);
	public static bool operator !=(Mount? left, Mount? right) => !Equals(left, right);
}

static class MountUtils
{
	public static ScriptNfo? GetScriptNfo(this Mount mount) =>
		mount.Cat switch
		{
			FCat.Css => new ScriptNfo(ScriptType.Css, mount.Link),
			FCat.Js => mount.JsFlags!.Value switch
			{
				MountJsFlags.ServeOnly => null,
				MountJsFlags.AddScript => new ScriptNfo(ScriptType.Js, mount.Link),
				MountJsFlags.AddModuleScript => new ScriptNfo(ScriptType.JsModule, mount.Link),
				_ => throw new ArgumentException($"Invalid JsFlags: {mount.JsFlags}")
			},
			FCat.Manifest => new ScriptNfo(ScriptType.Manifest, mount.Link),
			_ => null,
		};

	public static Func<string, Task<string>>? GetCompileFun(this Mount mount, ILogr logr) => mount.Type switch
	{
		FType.Scss => str => ScssCompiler.Compile(str, logr),
		_ => null
	};


	/*public static Action GetReloadAction(this Mount mount, DomOps domOps) => () =>
	{
		var scriptNfo = mount.Cat switch
		{
			FCat.Css => new ScriptNfo(ScriptType.Css, mount.Link),
			FCat.Js => mount.JsFlags!.Value switch
			{
				MountJsFlags.ServeOnly => null,
				MountJsFlags.AddScript => new ScriptNfo(ScriptType.Js, mount.Link),
				MountJsFlags.AddModuleScript => new ScriptNfo(ScriptType.JsModule, mount.Link),
				_ => throw new ArgumentException()
			},
			FCat.Manifest => new ScriptNfo(ScriptType.Manifest, mount.Link),
			_ => null,
		};

		if (scriptNfo != null)
		{
			domOps.SignalDomEvt(new BumpHeadScript(scriptNfo));
		}
	};*/
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