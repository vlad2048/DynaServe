using DynaServeLib;

namespace DynaServeExtrasLib.Utils;

static class CssFolderAdder
{
	public static void AddDynaServeExtraCssFolder(this ServOpt opt, string cssFolderName)
	{
		var slnFolder = DynaServeExtrasDebug.HardcodedSolutionFolder;
		if (slnFolder != null)
		{
			ServeCssFolderInSln(opt, slnFolder, cssFolderName);
			return;
		}

		throw new NotImplementedException("TODO: need to compile the embedded .sass files into .css somehow");

		var embeddedFiles = EmbeddedExt.ReadFolder(cssFolderName, ".css").Concat(EmbeddedExt.ReadFolder(cssFolderName, ".scss")).ToArray();
		foreach (var embeddedFile in embeddedFiles)
			opt.AddScriptCss(embeddedFile.Name, embeddedFile.Content.FromBytes());
	}

	public static void AddDynaServeEmbeddedFontFolder(this ServOpt opt, string fontFolderName)
	{
		var embeddedFiles = EmbeddedExt.ReadFolder(fontFolderName, ".woff2");
		opt.ServEmbeddedFontResources(embeddedFiles);
	}



	private static void ServeCssFolderInSln(ServOpt opt, string slnFolder, string cssFolderName)
	{
		var cssFolder = Directory.GetDirectories(slnFolder, cssFolderName, SearchOption.AllDirectories).Single();
		opt.AddCss(cssFolder);
	}
}