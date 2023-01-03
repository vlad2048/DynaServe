using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Serving.Repliers;
using DynaServeLib.Utils;

namespace DynaServeLib;

public class ServOpt
{
	private ServOpt() {}

	internal List<IReplier> Repliers { get; } = new();
	internal List<string> SlnFolders { get; } = new()
	{
		@"C:\Dev_Nuget\Libs\DynaServe",
		@"C:\Dev_Nuget\Libs\ImdbLib",
		@"C:\Dev\Creepy",
	};
	internal List<IServNfo> ServNfos { get; } = new();
	internal List<string> ExtraHtmlNodes { get; } = new();

	public void AddRepliers(params IReplier[] repliers) => Repliers.AddRange(repliers);
	public bool CheckSecurity { get; set; } = false;
	public int Port { get; set; } = 7000;
	public ILogr Logr { get; set; } = new NullLogr();
	public bool PlaceWebSocketsHtmlManually { get; set; } = true;
	public bool ShowDynaServLibVersion { get; set; } = true;
	public Action<ClientUserMsg> OnClientUserMsg { get; set; } = _ => { };
	public string ScssOutputFolder { get; set; } = @"C:\tmp\css-compiled";

	internal static ServOpt Build(Action<ServOpt>? optFun)
	{
		var opt = new ServOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}


public static class ServOptFileServExt
{
	public static void AddSlnFolder(
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
		);
}