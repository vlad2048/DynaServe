using CliWrap;
using CliWrap.Buffered;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Security.Utils;

static class Cmder
{
	public static string[] Run(string exe, params string[] args) =>
		Cli.Wrap(exe)
			.WithArguments(args)
			.ExecuteBufferedAsync()
			.GetAwaiter()
			.GetResult()
			.StandardOutput
			.ToLines();
}