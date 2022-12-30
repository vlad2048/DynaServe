using DynaServeLib.Utils;

namespace DynaServeExtrasLib.Utils;

static class EmbeddedExt
{
	internal static string Read(string name, params (string, string)[] substitutions) =>
		Embedded.Read(name, typeof(EmbeddedExt).Assembly, substitutions);

	internal static EmbeddedFile[] ReadFolder(string folderName, string ext) => Embedded.ReadFolder(folderName, typeof(EmbeddedExt).Assembly, ext);
}