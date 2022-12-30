using System.Reflection;
using PowBasics.CollectionsExt;

namespace DynaServeLib.Utils;

public record EmbeddedFile(string Name, byte[] Content);

public static class Embedded
{
	private static readonly Dictionary<Assembly, string[]> assMap = new();

	internal static string Read(string name, params (string, string)[] substitutions) =>
		Read(name, typeof(Embedded).Assembly, substitutions);


	public static EmbeddedFile[] ReadFolder(string folderName, Assembly assembly, string ext)
	{
		folderName = folderName.Replace("-", "_");

		bool IsInFolderAndHasExt(string fullname)
		{
			var searchStr = $".{folderName}.";
			var idx = fullname.IndexOf(searchStr, StringComparison.Ordinal);
			return idx != -1 && fullname.EndsWith(ext);
		}

		string GetName(string fullname)
		{
			var searchStr = $".{folderName}.";
			var idx = fullname.IndexOf(searchStr, StringComparison.Ordinal);
			return fullname[(idx + searchStr.Length)..];
		}

		return GetAssNames(assembly)
			.Where(IsInFolderAndHasExt)
			.SelectToArray(e => new EmbeddedFile(GetName(e), assembly.GetManifestResourceStream(e)!.ToBin()));
	}


	public static string Read(
		string name,
		Assembly assembly,
		params (string, string)[] substitutions
	)
	{
		var embedName = GetAssNames(assembly).Single(e => e.EndsWith(name));
		var text = assembly.GetManifestResourceStream(embedName)!.ToText();
		foreach (var (key, val) in substitutions)
			text = text.Replace(key, val);
		return text;
	}

	private static string ToText(this Stream stream)
	{
		using var ms = new MemoryStream();
		using var sr = new StreamReader(ms);
		stream.CopyTo(ms);
		ms.Flush();
		ms.Position = 0;
		return sr.ReadToEnd();
	}

	private static byte[] ToBin(this Stream stream)
	{
		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		ms.Flush();
		ms.Position = 0;
		return ms.ToArray();
	}

	private static string[] GetAssNames(Assembly assembly)
	{
		if (!assMap.TryGetValue(assembly, out var names))
			names = assMap[assembly] = assembly.GetManifestResourceNames();
		return names;
	}
}