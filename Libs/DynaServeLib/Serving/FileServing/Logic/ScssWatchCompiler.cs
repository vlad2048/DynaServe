using System.Diagnostics;
using CliWrap;
using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.Utils;
using DynaServeLib.Utils;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.Serving.FileServing.Logic;

static class ScssWatchCompiler
{
	public static IDisposable Start(string[] inputFolders, string rootOutputFolder, ILogr logr)
	{
		KillAllSassProcesses();		// HACK: this will prevent running multiple instances
		PrepareOutputFolder(inputFolders, rootOutputFolder);		// HACK: again this will prevent running multiple instances

		var d = new Disp();
		var cancelToken = CancelUtils.MakeFromDisp(d);

		foreach (var inputFolder in inputFolders)
		{
			var outputFolder = Path.Combine(rootOutputFolder, Path.GetFileName(inputFolder));
			Cli.Wrap("sass")
				.WithArguments(
					$@"""{inputFolder}"":""{outputFolder}"" --no-source-map --watch"
				)
				.WithStandardErrorPipe(PipeTarget.ToDelegate(logr.CssError))
				.ExecuteAsync(cancelToken);
		}

		return d;
	}


	private static void KillAllSassProcesses()
	{
		var procs = Process.GetProcessesByName("sass");
		foreach (var proc in procs)
			proc.Kill();
	}

	private static void PrepareOutputFolder(string[] inputFolders, string rootOutputFolder)
	{
		var expFiles = (
			from inputFolder in inputFolders
			from inputFile in Directory.GetFiles(inputFolder, "*.css").Concat(Directory.GetFiles(inputFolder, "*.scss"))
			select inputFile.TransposeFileToCompiledFolderIFN(rootOutputFolder)
		).ToArray();
		var expFolders = expFiles.SelectToArray(e => Path.GetDirectoryName(e)!).Distinct().ToArray();

		RemoveAllFilesInFolder(rootOutputFolder);

		foreach (var rootDir in Directory.GetDirectories(rootOutputFolder))
		{
			if (!expFolders.Contains(rootDir))
			{
				Directory.Delete(rootDir, true);
			}
			else
			{
				RemoveAllSubFoldersInFolder(rootDir);
				foreach (var outFile in Directory.GetFiles(rootDir))
				{
					if (!expFiles.Contains(outFile))
						File.Delete(outFile);
				}
			}
		}

		foreach (var expFolder in expFolders)
		{
			if (!Directory.Exists(expFolder))
				Directory.CreateDirectory(expFolder);
		}
	}

	private static void RemoveAllFilesInFolder(string folder)
	{
		foreach (var files in Directory.GetFiles(folder))
			File.Delete(files);
	}
	private static void RemoveAllSubFoldersInFolder(string folder)
	{
		foreach (var dir in Directory.GetDirectories(folder))
			Directory.Delete(dir, true);
	}
	
}
