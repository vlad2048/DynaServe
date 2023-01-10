using System.Text;
using CliWrap;
using DynaServeLib.Logging;

namespace DynaServeLib.Serving.FileServing.Utils;

static class ScssCompiler
{
	public static async Task<string> Compile(string fileContent, ILogr logr)
	{
		var stdOutBuffer = new StringBuilder();
		var stdErrBuffer = new StringBuilder();
		var res = await Cli.Wrap("sass")
			.WithArguments(new [] {
				"--stdin",
				"--no-source-map",
			})
			.WithStandardInputPipe(PipeSource.FromString(fileContent))
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
			.WithValidation(CommandResultValidation.None)
			.ExecuteAsync();
		var stdOut = stdOutBuffer.ToString();
		var stdErr = stdErrBuffer.ToString();

		if (stdErr != "")
			logr.CssError(stdErr);

		return stdOut;
	}
}