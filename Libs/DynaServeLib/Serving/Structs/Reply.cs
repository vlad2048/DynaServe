using System.Net;
using System.Reflection;
using System.Text;
using DynaServeLib.Serving.FileServing.StructsEnum;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Structs;

enum ReplyDataType
{
	String,
	Binary
}

class ReplyData
{
	public ReplyDataType Type { get; }
	public string? DataString { get; private init; }
	public byte[]? DataBinary { get; private init; }

	private ReplyData(ReplyDataType type) => Type = type;

	public static ReplyData MkString(string dataString, (string, string)[] substs) => new(ReplyDataType.String) { DataString = dataString.ApplySubsts(substs) };
	public static ReplyData MkBinary(byte[] dataBinary) => new(ReplyDataType.Binary) { DataBinary = dataBinary };
}


record Reply(
	FType Type,
	ReplyData Data
)
{
	public static async Task<Reply> LoadFromFile(
		FType type,
		string filename,
		(string, string)[] substs,
		Func<string, Task<string>>? compileFun
	) =>
		new(
			type,
			type.IsBinary() switch
			{
				false => ReplyData.MkString(await (await File.ReadAllTextAsync(filename)).Compile(compileFun), substs),
				true => ReplyData.MkBinary(await File.ReadAllBytesAsync(filename))
			}
		);

	public static async Task<Reply> LoadFromEmbedded(
		FType type,
		string embeddedName,
		Assembly ass,
		(string, string)[] substs,
		Func<string, Task<string>>? compileFun
	) =>
		new(
			type,
			type.IsBinary() switch
			{
				false => ReplyData.MkString(await EmbeddedUtils.LoadAsString(embeddedName, ass).Compile(compileFun), substs),
				true => ReplyData.MkBinary(EmbeddedUtils.LoadAsBinary(embeddedName, ass)),
			}
		);

	public static async Task<Reply> LoadFromString(
		FType type,
		string @string,
		(string, string)[] substs,
		Func<string, Task<string>>? compileFun
	) =>
		new(
			type,
			ReplyData.MkString(await @string.Compile(compileFun), substs)
		);
}


static class ReplyExt
{
	public static async Task WriteToResponse(this Reply reply, HttpListenerResponse response)
	{
		var bytes = reply.Data.Type switch
		{
			ReplyDataType.String => reply.Data.DataString!.ToBytes(),
			ReplyDataType.Binary => reply.Data.DataBinary!,
			_ => throw new ArgumentException($"Invalid Reply.Data.Type: {reply.Data.Type}")
		};
		response.ContentEncoding = reply.Data.Type switch
		{
			ReplyDataType.String => Encoding.UTF8,
			ReplyDataType.Binary => Encoding.Default,
			_ => throw new ArgumentException()
		};
		response.ContentType = reply.Type.ToMime();
		response.ContentLength64 = bytes.LongLength;
		await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
	}

	public static Reply MakeReplyFromImage(string name, byte[] data)
	{
		var ext = Path.GetExtension(name).ToLowerInvariant();
		return ext switch
		{
			".png" => new Reply(FType.ImagePng, ReplyData.MkBinary(data)),
			".jpeg" => new Reply(FType.ImageJpg, ReplyData.MkBinary(data)),
			".jpg" => new Reply(FType.ImageJpg, ReplyData.MkBinary(data)),
			".ico" => new Reply(FType.ImageIco, ReplyData.MkBinary(data)),
			".svg" => new Reply(FType.ImageSvg, ReplyData.MkBinary(data)),
			_ => throw new ArgumentException($"Unknown image extension: {ext}")
		};
	}

	public static async Task<string> Compile(this string str, Func<string, Task<string>>? compileFun) => compileFun switch
	{
		null => str,
		not null => await compileFun(str)
	};
}