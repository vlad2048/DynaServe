namespace DynaServeLib.Serving.FileServing.Utils;

static class ServFileUtils
{
	public static string TransposeFileToCompiledFolderIFN(this string file, string scssOutputFolder)
	{
		var ext = Path.GetExtension(file);
		if (ext != ".scss" && ext != ".css") return file;

		var dir = Path.GetFileName(Path.GetDirectoryName(file)!);
		var name = Path.GetFileNameWithoutExtension(file);
		return Path.Combine(scssOutputFolder, dir, $"{name}.css");
	}
}