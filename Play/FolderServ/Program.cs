using System.Net;
using System.Text;

namespace FolderServ;

static class Program
{
	private const string Folder = @"C:\tmp\tsc-play\code\outFolder";
	//private const string Folder = @"C:\tmp\tsc-play\demo-modules\basic";

	private static HttpListener listener = null!;

	static void Main()
	{
		listener = new HttpListener();
		listener.Prefixes.Add("http://*:7000/");
		listener.Start();
		listener.BeginGetContext(OnContext, null);
		L("Running");
		Console.ReadKey();
		L("Exiting");
	}

	private static void OnContext(IAsyncResult ar)
	{
		try
		{
			var ctx = listener.EndGetContext(ar);
			listener.BeginGetContext(OnContext, null);

			//Task.Run(async () =>
			{
				try
				{
					var req = ctx.Request;
					var res = ctx.Response;
					var url = req.GetUrl();
					if (url == string.Empty)
						url = "index.html";
					LW($"url:'{url}'");
					var file = GetFile(url);
					LW($"  file:'{file}'");
					if (file == null)
					{
						L("  -> Cannot find file");
						return;
					}
					L("  -> OK");

					var content = File.ReadAllText(file);
					var replyType = file.GetReplyType();
					var reply = new Reply(replyType, content);
					reply.WriteToResponse(res);
					res.Close();
				}
				catch (Exception ex)
				{
					L($"InnerEx: {ex}");
				}
			}
			//);
		}
		catch (Exception ex)
		{
			L($"OuterEx: {ex}");
		}
	}

	private static string? GetFile(string url)
	{
		var file = Path.Combine(Folder, url);
		if (File.Exists(file)) return file;
		file = $"{file}.js";
		if (File.Exists(file)) return file;
		return null;
	}

	private static void LW(string s) => Console.Write(s);
	private static void L(string s) => Console.WriteLine(s);
}

enum ReplyType
{
	Html,
	Js
}

record Reply(ReplyType Type, string Content)
{
	public void WriteToResponse(HttpListenerResponse response)
	{
		var data = Content.ToBytes();
		var isBinary = false;
		response.ContentEncoding = isBinary switch
		{
			true => Encoding.Default,
			false => Encoding.UTF8,
		};
		response.ContentType = Type.GetMimeType();
		response.ContentLength64 = data.LongLength;
		response.OutputStream.Write(data, 0, data.Length);
	}
}

static class ReplyUtils
{
	public static ReplyType GetReplyType(this string file) => Path.GetExtension(file).ToLowerInvariant() switch
	{
		".html" => ReplyType.Html,
		".js" => ReplyType.Js,
		_ => throw new ArgumentException()
	};

	public static string GetMimeType(this ReplyType type) => type switch
	{
		ReplyType.Html => "text/html",
		ReplyType.Js => "text/javascript",
		_ => throw new ArgumentException()
	};
}


static class UrlUtils
{
	public static string FromBytes(this byte[] data) => Encoding.UTF8.GetString(data);
	public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);

	public static string GetUrl(this HttpListenerRequest req)
	{
		if (req.Url == null) throw new ArgumentException();
		return req.Url.PathAndQuery.RemoveLeadingSlash();
	}

	private static string RemoveLeadingSlash(this string s) => (s[0] == '/') switch
	{
		true => s[1..],
		false => s
	};
}