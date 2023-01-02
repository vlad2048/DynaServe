using System.Net;
using System.Text;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Serving.Structs;

enum ReplyType
{
	Html,
	ScriptJs,
	ScriptCss,
	Json,
	ImageSvg,
	ImagePng,
	ImageJpg,
	ImageIco,
	Video,
	FontWoff2,
}

class Reply
{
	public ReplyType Type { get; }
	public byte[] Data { get; }

	private Reply(ReplyType type, byte[] data)
	{
		Type = type;
		Data = data;
	}

	public static Reply MkBin(ReplyType type, byte[] data) => new(type, data);
	public static Reply MkTxt(ReplyType type, string data) => new(type, data.ToBytes());

	public static Reply Mk(ReplyType type, byte[] data) => new(type, data);
}


static class ReplyExt
{
	public static async Task WriteToResponse(this Reply reply, HttpListenerResponse response)
	{
		var isBinary = reply.Type.IsBinary();
		response.ContentEncoding = isBinary switch
		{
			true => Encoding.Default,
			false => Encoding.UTF8,
		};
		response.ContentType = reply.Type.GetMimeType();
		response.ContentLength64 = reply.Data.LongLength;
		await response.OutputStream.WriteAsync(reply.Data, 0, reply.Data.Length);
	}

	public static Reply MakeReplyFromImage(string name, byte[] data) => Path.GetExtension(name).ToLowerInvariant() switch
	{
		".png" => Reply.MkBin(ReplyType.ImagePng, data),
		".jpeg" => Reply.MkBin(ReplyType.ImageJpg, data),
		".jpg" => Reply.MkBin(ReplyType.ImageJpg, data),
		".ico" => Reply.MkBin(ReplyType.ImageIco, data),
		".svg" => Reply.MkBin(ReplyType.ImageSvg, data),
	};

	private static bool IsBinary(this ReplyType type) => type switch
	{
		ReplyType.Html => false,
		ReplyType.ScriptJs => false,
		ReplyType.ScriptCss => false,
		ReplyType.Json => false,
		ReplyType.ImageSvg => false,
		ReplyType.ImagePng => true,
		ReplyType.ImageJpg => true,
		ReplyType.ImageIco => true,
		ReplyType.Video => true,
		ReplyType.FontWoff2 => true,
		_ => throw new ArgumentException()
	};

	private static string GetMimeType(this ReplyType type) => type switch
	{
		ReplyType.Html => "text/html",
		ReplyType.ScriptJs => "text/javascript",
		ReplyType.ScriptCss => "text/css",
		ReplyType.Json => "application/json",
		ReplyType.ImageSvg => "image/svg+xml",
		ReplyType.ImagePng => "image/png",
		ReplyType.ImageJpg => "image/jpeg",
		ReplyType.ImageIco => "image/x-icon",
		ReplyType.Video => "video/mp4",
		ReplyType.FontWoff2 => "font/woff2",
		_ => throw new ArgumentException()
	};
}