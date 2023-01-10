using System.Reflection;
using System.Runtime.CompilerServices;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.FileServing.Utils;

namespace DynaServeLib;

enum ServRange
{
	File,
	Folder,
	FolderWithRecursion
}
public class FileServOpt
{
	public Assembly? Ass { get; set; }
	internal ServRange Range { get; set; } = ServRange.File;
	public string MountLocation { get; set; } = "";
	public MountJsFlags? JsFlags { get; set; }
	public (string, string)[] Substs { get; set; } = Array.Empty<(string, string)>();

	private FileServOpt() {}

	internal static FileServOpt Make(Action<FileServOpt>? optFun)
	{
		var opt = new FileServOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

public static class ServOptFileServExt
{
	public static void ServeFile(
		this ServOpt opt,
		string @short,
		Action<FileServOpt>? foptFun = null,
		[CallerFilePath] string? srcFile = null
	) =>
		opt.ServeFileInnerWithCat(
			@short,
			@short.ToCat(),
			foptFun,
			srcFile ?? throw new ArgumentException("Do not specify the srcFile argument")
		);

	public static void ServeFolder(
		this ServOpt opt,
		string @short,
		FCat cat,
		Action<FileServOpt>? foptFun = null,
		[CallerFilePath] string? srcFile = null
	) =>
		opt.ServeFileInnerWithCat(
			@short,
			cat,
			fopt =>
			{
				fopt.Range = ServRange.Folder;
				foptFun?.Invoke(fopt);
			},
			srcFile ?? throw new ArgumentException("Do not specify the srcFile argument")
		);

	private static void ServeFileInnerWithCat(
		this ServOpt opt,
		string @short,
		FCat cat,
		Action<FileServOpt>? foptFun,
		string srcFile
	)
	{
		var fopt = FileServOpt.Make(foptFun);
		var searchRoot = FileFinder.GetSearchRoot(srcFile, opt.SearchFolder);
		opt.Mounts.AddRange(
			FileFinder.ResolveShort(@short, fopt.Ass, cat, fopt.Range, searchRoot)
				.Select(file => new Mount(
					new FileMountSrc(file),
					fopt.MountLocation,
					fopt.Substs,
					fopt.JsFlags
				))
		);
	}


	
	public static void ServeString(
		this ServOpt opt,
		string @string,
		string name,
		Action<FileServOpt>? foptFun = null
	)
	{
		var fopt = FileServOpt.Make(foptFun);
		opt.Mounts.Add(new Mount(
			new StringMountSrc(@string, name),
			fopt.MountLocation,
			fopt.Substs,
			fopt.JsFlags
		));
	}


	/*public static void AddSlnFolder(
		this ServOpt opt,
		params string[] slnFolders
	) =>
		opt.SlnFolders.AddRange(slnFolders);

	internal static void AddEmbeddedHtml(
		this ServOpt opt,
		string name,
		params (string, string)[] substitutions
	) =>
		opt.ExtraHtmlNodes.Add(Embedded.Read(name, substitutions));

	public static void Serve(
		this ServOpt opt,
		FCat cat,
		params string[] fuzzyFolders
	) =>
		opt.ServNfos.AddRange(
			fuzzyFolders.Select(fuzzyFolder => new LocalFolderServNfo(cat, fuzzyFolder))
		);

	public static void ServeFile(
		this ServOpt opt,
		string fuzzyFolder,
		string file,
		params (string, string)[] substitutions
	) =>
		opt.ServNfos.Add(new LocalFileServNfo(fuzzyFolder, file, substitutions));

	public static void ServeEmbedded(
		this ServOpt opt,
		string embeddedName,
		params (string, string)[] substitutions
	) =>
		opt.ServNfos.Add(
			DirectFileServNfo.FromString(
				embeddedName.ToCat(),
				embeddedName,
				Embedded.Read(embeddedName, substitutions)
			)
		);

	public static void ServeHardcoded(
		this ServOpt opt,
		string name,
		string content
	) =>
		opt.ServNfos.Add(
			DirectFileServNfo.FromString(
				name.ToCat(),
				name,
				content
			)
		);*/
}