namespace DynaServeLib.Utils;

static class Embedded
{
	private static readonly Lazy<string[]> resNames = new(() => typeof(Embedded).Assembly.GetManifestResourceNames());

	public static string Read(
		string name,
		params (string, string)[] substitutions
	)
	{
		var text = typeof(Embedded).Assembly
			.GetManifestResourceStream(
				resNames.Value.Single(e => e.EndsWith(name))
			)!
			.ToText();
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
}