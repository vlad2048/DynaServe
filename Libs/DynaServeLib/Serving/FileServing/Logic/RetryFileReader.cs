using DynaServeLib.Logging;
using PowMaybe;

namespace DynaServeLib.Serving.FileServing.Logic;

static class RetryFileReader
{
	private static readonly TimeSpan[] retryIntervals =
	{
		TimeSpan.FromMilliseconds(50),
		TimeSpan.FromMilliseconds(150),
		TimeSpan.FromMilliseconds(350),
	};

	public static async Task<Maybe<byte[]>> ReadFileBytes(string file)
	{
		if (!File.Exists(file)) return May.None<byte[]>();

		async Task<Maybe<byte[]>> Read()
		{
			try
			{
				var bytes = await File.ReadAllBytesAsync(file);
				return May.Some(bytes);
			}
			// The process cannot access the file 'C:\tmp\css-compiled\idx-2\log-dlg.css' because it is being used by another process.
			catch (IOException ex) when (ex.HResult == -2147024864)
			{
				return May.None<byte[]>();
			}
		}

		var idx = 0;
		while (idx < retryIntervals.Length)
		{
			var mayBytes = await Read();
			if (mayBytes.IsSome()) return mayBytes;
			Thread.Sleep(retryIntervals[idx]);
			idx++;
		}
		throw new ArgumentException($"Failed to read file: '{file}' (because it is being used by another process)");
	}


	public static string ReadFile(string file, ILogr logr)
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
				logr.Log(ex.Message);
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
		logr.Log($"FATAL failed to read this file multiple times: '{file}'");

		throw new ArgumentException($"Failed to read file: '{file}' (because it is being used by another process)");
	}
}