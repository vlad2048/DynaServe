using System.Reflection;

namespace DynaServeLib.Utils;

public static class EmbeddedUtils
{
	public static string File2EmbeddedIFN(this string file, Assembly? ass) => ass switch
	{
		null => file,
		not null => FindFileInAssNames(file, GetAssNames(ass))
	};

	public static string LoadAsString(string embeddedName, Assembly ass) =>
		ass.GetManifestResourceStream(embeddedName)!.ToText();

	public static byte[] LoadAsBinary(string embeddedName, Assembly ass) =>
		ass.GetManifestResourceStream(embeddedName)!.ToBin();



	private static readonly Dictionary<Assembly, string[]> assMap = new();
	private static string[] GetAssNames(Assembly assembly)
	{
		if (!assMap.TryGetValue(assembly, out var names))
			names = assMap[assembly] = assembly.GetManifestResourceNames();
		return names;
	}

	private static string FindFileInAssNames(string file, string[] assNames)
	{
		var parts = file.Split('\\');
		for (var i = parts.Length - 1; i > 0; i--)
		{
			var subEmbedName = parts.Skip(i)
				.Join('\\')
				.ToEmbedName();
			var matches = assNames.Where(e => e.EndsWith(subEmbedName)).ToArray();
			switch (matches.Length)
			{
				case 0:
					throw new FileNotFoundException($"Cannot find '{file}' in embedded resources");
				case 1:
					return matches[0];
			}
		}
		throw new FileNotFoundException($"Cannot find '{file}' in embedded resources (ambiguous matches)");
	}
	
	private static string Join(this IEnumerable<string> source, char ch) => string.Join(ch, source);
	
	private static string ToEmbedName(this string s)
	{
		var folder = Path.GetDirectoryName(s)!;
		var name = Path.GetFileName(s);
		var embedFolder = folder.Replace("-", "_").Replace('\\', '.');
		var embedName = name.Replace('\\', '.');
		return $"{embedFolder}.{embedName}";
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
}


/*
using System.Reflection;
using PowBasics.CollectionsExt;

namespace DynaServeLib.Utils;

public record EmbeddedFile(string Name, byte[] Content);

public static class Embedded
{
	private static readonly Dictionary<Assembly, string[]> assMap = new();

	internal static string Read(string name, params (string, string)[] substitutions) =>
		Read(name, typeof(Embedded).Assembly, substitutions);

	public static string GetNameFromFullName(string s)
	{
		var idx = s.LastIndexOf('.');
		if (idx == -1) throw new ArgumentException();
		var t = s[..idx];
		idx = t.LastIndexOf('.');
		if (idx == -1) throw new ArgumentException();
		return s[(idx + 1)..];
	}


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

	internal static string ReadExact(string exactName, Assembly assembly) =>
		assembly.GetManifestResourceStream(exactName)!.ToText();

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
*/
