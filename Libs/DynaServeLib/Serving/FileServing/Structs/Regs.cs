/*
using System.Net;
using System.Text;
using DynaServeLib.Serving.FileServing.Logic;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Utils.Exts;
using PowMaybe;

namespace DynaServeLib.Serving.FileServing.Structs;


record RegData(
	FType Type,
	byte[] Content
)
{
	public async Task WriteToResponse(HttpListenerResponse response)
	{
		var isBinary = Type.IsBinary();
		response.ContentEncoding = isBinary switch
		{
			true => Encoding.Default,
			false => Encoding.UTF8,
		};
		response.ContentType = Type.ToMime();
		response.ContentLength64 = Content.LongLength;
		await response.OutputStream.WriteAsync(Content, 0, Content.Length);
	}
}


interface IReg
{
	string Name { get; }
	Task<Maybe<RegData>> GetContent();
}


class DirectReg : IReg
{
	private readonly RegData data;

	public string Name { get; }

	public DirectReg(string name, byte[] content)
	{
		Name = name;
		data = new RegData(Name.ToType(), content);
	}

	public Task<Maybe<RegData>> GetContent() => Task.FromResult(May.Some(data));
}


class FileReg : IReg
{
	private Maybe<RegData> mayData = May.None<RegData>();

	public string Name => Path.GetFileName(Filename);
	public string Filename { get; }
	public (string, string)[]? Substitutions { get; }

	public FileReg(string filename, (string, string)[]? substitutions)
	{
		Filename = filename;
		Substitutions = substitutions;
	}

	public async Task<Maybe<RegData>> GetContent()
	{
		if (mayData.IsSome()) return mayData;
		mayData =
			from bytes in (await RetryFileReader.ReadFileBytes(Filename))
			select new RegData(Filename.ToType(), bytes.ApplySubstitutions(Substitutions));
		return mayData;
	}

	public void Invalidate()
	{
		mayData = May.None<RegData>();
	}
}


file static class FileRegExt
{
	public static byte[] ApplySubstitutions(this byte[] data, (string, string)[]? substitutions)
	{
		if (substitutions == null) return data;
		var text = data.FromBytes();
		foreach (var substitution in substitutions)
			text = text.Replace(substitution.Item1, substitution.Item2);
		return text.ToBytes();
	}
}
*/