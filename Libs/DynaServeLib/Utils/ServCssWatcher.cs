using System.Diagnostics;
using System.Reactive.Disposables;
using CliWrap;
using DynaServeLib.DynaLogic;
using DynaServeLib.Logging;
using PowRxVar;

namespace DynaServeLib.Utils;

static class ServCssWatcher
{
	private const string OutputFolder = @"C:\tmp\css-compiled";
	private static readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(200);
	private static readonly TimeSpan maxWaitForReady = TimeSpan.FromMilliseconds(2000);
	private static readonly TimeSpan readyCheckInterval = TimeSpan.FromMilliseconds(100);

	public static IDisposable Setup(
		IReadOnlyList<string> cssFoldersIn,
		Dom dom,
		ILogr logr
	)
	{
		KillAllSassProcesses();

		if (cssFoldersIn.Count == 0) return Disposable.Empty;
		var d = new Disp();
		var cancelSource = new CancellationTokenSource();
		var cancelToken = cancelSource.Token;
		Disposable.Create(() =>
		{
			cancelSource.Cancel();
			cancelSource.Dispose();
		}).D(d);

		PrepareOutputFolder(OutputFolder);
		for (var index = 0; index < cssFoldersIn.Count; index++)
		{
			var cssFolderIn = cssFoldersIn[index];
			CompileAndWatchCss(cssFolderIn, index, dom, cancelToken, logr).D(d);
		}

		return d;
	}

	private static IDisposable CompileAndWatchCss(
		string cssFolderIn,
		int index,
		Dom dom,
		CancellationToken cancelToken,
		ILogr logr
	)
	{
		var d = new Disp();
		var useSass = Directory.GetFiles(cssFolderIn, "*.scss").Any();
		var expectedStyleSheetCount = Directory.GetFiles(cssFolderIn, "*.scss").Length + Directory.GetFiles(cssFolderIn, "*.css").Length;

		var cssFolderOut = useSass switch
		{
			false => cssFolderIn,
			true => CompileSass(cssFolderIn, index, cancelToken, logr).D(d)
		};

		WaitUntilReady(useSass, cssFolderOut, expectedStyleSheetCount).D(d);

		WatchOutputFolder(cssFolderOut, dom, logr).D(d);

		return d;
	}

	private static (string, IDisposable) CompileSass(string folderIn, int index, CancellationToken cancelToken, ILogr logr)
	{
		var d = new Disp();
		var folderOut = Path.Combine(OutputFolder, $"idx-{index}");
		Directory.CreateDirectory(folderOut);

		CopyAll(folderIn, folderOut, "*.css");
		var cssWatcher = new FolderWatcher(folderIn, "*.css", debounceTime).D(d);
		cssWatcher.WhenChange.Subscribe(fileSrc => CopySingle(fileSrc, folderOut)).D(d);

		Cli.Wrap("sass")
			.WithArguments(
				$@"""{folderIn}"":""{folderOut}"" --no-source-map --watch"
			)
			.WithStandardErrorPipe(PipeTarget.ToDelegate(logr.OnCssError))
			.ExecuteAsync(cancelToken);

		return (folderOut, d);
	}


	private static IDisposable WatchOutputFolder(string cssFolderOut, Dom dom, ILogr logr)
	{
		void RefreshInitial()
		{
			var initFiles = Directory.GetFiles(cssFolderOut, "*.css");
			foreach (var file in initFiles)
			{
				var name = Path.GetFileNameWithoutExtension(file);
				var script = RetryReadFile(file, logr);
				dom.AddOrRefreshCss(name, script);
			}
		}

		RefreshInitial();

		var d = new Disp();
		var folderWatcher = new FolderWatcher(cssFolderOut, "*.css", debounceTime).D(d);

		folderWatcher.WhenChange.Subscribe(file =>
		{
			var name = Path.GetFileNameWithoutExtension(file);
			var script = RetryReadFile(file, logr);
			dom.AddOrRefreshCss(name, script);
		}).D(d);
		return d;
	}

	private static IDisposable WaitUntilReady(bool useSass, string cssFolderOut, int expectedStyleSheetCount)
	{
		if (!useSass) return Disposable.Empty;
		var d = new Disp();

		bool IsReady() => Directory.GetFiles(cssFolderOut, "*.css").Length >= expectedStyleSheetCount;

		var timeStart = DateTime.Now;
		while (!IsReady() && DateTime.Now - timeStart < maxWaitForReady)
		{
			Thread.Sleep(readyCheckInterval);
		}

		return d;
	}

	private static void PrepareOutputFolder(string outputFolder)
	{
		if (!Directory.Exists(outputFolder))
		{
			Directory.CreateDirectory(outputFolder);
			return;
		}
		foreach (var dir in Directory.GetDirectories(outputFolder))
			Directory.Delete(dir, true);
	}

	/*private static string GetFolderOut(string folderIn)
	{
		var baseFolder = Path.GetDirectoryName(folderIn)!;
		var name = Path.GetFileNameWithoutExtension(folderIn);
		var folderOut = Path.Combine(baseFolder, $"{name}-compiled");
		if (!Directory.Exists(folderOut))
			Directory.CreateDirectory(folderOut);
		foreach (var file in Directory.GetFiles(folderOut))
			File.Delete(file);
		return folderOut;
	}*/

	private static void CopySingle(string fileSrc, string folderDst)
	{
		var fileDst = Path.Combine(folderDst, Path.GetFileName(fileSrc));
		File.Copy(fileSrc, fileDst, true);
	}

	private static void CopyAll(string folderSrc, string folderDst, string pattern)
	{
		var files = Directory.GetFiles(folderSrc, pattern);
		foreach (var fileSrc in files)
		{
			var fileDst = Path.Combine(folderDst, Path.GetFileName(fileSrc));
			File.Copy(fileSrc, fileDst, true);
		}
	}


	private static readonly TimeSpan[] retryIntervals =
	{
		TimeSpan.FromMilliseconds(50),
		TimeSpan.FromMilliseconds(150),
		TimeSpan.FromMilliseconds(350),
	};

	private static string RetryReadFile(string file, ILogr logr)
	{
		string? Read()
		{
			try
			{
				return File.ReadAllText(file);
			}
			// The process cannot access the file 'C:\tmp\css-compiled\idx-2\log-dlg.css' because it is being used by another process.
			catch (IOException ex) when (ex.HResult == -2147024864)
			{
				logr.OnSimpleMsg(ex.Message);
				return null;
			}
		}

		var idx = 0;
		while (idx < retryIntervals.Length)
		{
			var str = Read();
			if (str != null)
				return str;
			Thread.Sleep(retryIntervals[idx]);
			idx++;
		}
		logr.OnSimpleMsg($"FATAL failed to read this file multiple times: '{file}'");

		throw new ArgumentException($"Failed to read file: '{file}' (because it is being used by another process)");
	}

	private static void KillAllSassProcesses()
	{
		var procs = Process.GetProcessesByName("sass");
		foreach (var proc in procs)
			proc.Kill();
	}
}