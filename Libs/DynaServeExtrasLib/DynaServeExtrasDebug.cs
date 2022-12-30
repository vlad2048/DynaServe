namespace DynaServeExtrasLib;

public static class DynaServeExtrasDebug
{
	// HACK
	// not null		-> picks the .css files from the solution folder for live editing
	// null			-> picks the .css files from the .dll embedded resources for prod
	public static string? HardcodedSolutionFolder { get; set; } = @"C:\Dev_Nuget\Libs\DynaServe";
}