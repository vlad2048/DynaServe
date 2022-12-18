namespace DynaServeLib.Serving.Repliers.VideoForwarder.Utils;

public enum VidSrcType
{
	Link,
	File
}

public record VidLinkFw(
	string Link,
	Headers? Headers
);

public record VidSrc(
	VidSrcType Type,
	VidLinkFw? Link,
	string? File
);

class LinkMapper
{
	private readonly Dictionary<VidSrc, string> src2name = new();
	private readonly Dictionary<string, VidSrc> name2src = new();

	public string RegisterVid(VidLinkFw vidLink)
	{
		var src = new VidSrc(VidSrcType.Link, vidLink, null);
		if (src2name.TryGetValue(src, out var name)) return name;
		name = $"vid_{src2name.Count}.mp4";
		src2name[src] = name;
		name2src[name] = src;
		return name;
	}

	public string RegisterFile(string file)
	{
		var src = new VidSrc(VidSrcType.File, null, file);
		if (src2name.TryGetValue(src, out var name)) return name;
		name = $"file_{src2name.Count}.mp4";
		src2name[src] = name;
		name2src[name] = src;
		return name;
	}

	public VidSrc GetSrc(string name) => name2src[name];
}