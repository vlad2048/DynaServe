using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using DynaServeLib.Serving.Repliers.VideoForwarder.Events;
using DynaServeLib.Serving.Repliers.VideoForwarder.Structs;
using DynaServeLib.Serving.Repliers.VideoForwarder.Utils;
using DynaServeLib.Serving.Structs;
using DynaServeLib.Utils;
using DynaServeLib.Utils.Exts;
using PowRxVar;

namespace DynaServeLib.Serving.Repliers.VideoForwarder;

public class VideoForwarderOpt
{
	public string UrlPrefix { get; set; } = "videoforwarder/";
	public int BufferSize { get; set; } = 128 * 1024;
	public TimeSpan HeaderTimeout { get; set; } = TimeSpan.FromSeconds(2);
	public TimeSpan ChunkTimeout { get; set; } = TimeSpan.FromSeconds(2);

	private VideoForwarderOpt() {}

	internal static VideoForwarderOpt Build(Action<VideoForwarderOpt>? optFun)
	{
		var opt = new VideoForwarderOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

public class VideoForwarderReplier : IReplier, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly int port;
	private readonly VideoForwarderOpt opt;
	private readonly LinkMapper linkMapper = new();
	private readonly ISubject<IVidFwEvt> whenEvt;
	private readonly LinkSrcRequester linkSrcRequester;
	private readonly FileSrcRequester fileSrcRequester;
	private readonly Dictionary<string, CancellationTokenSource> cancelMap;
	private int reqCounter;

	private string Name2Link(string name) => $"{UrlUtils.GetLocalLink(port)}{opt.UrlPrefix}{name}";

	public string RegisterVid(string link, Headers? headers) => Name2Link(linkMapper.RegisterVid(new VidLinkFw(link, headers)));
	public string RegisterFile(string file) => Name2Link(linkMapper.RegisterFile(file));
	public IObservable<IVidFwEvt> WhenEvt => whenEvt.AsObservable();
	public IObservable<string> WhenEvtMsg => WhenEvt.Select(e => $"{e}");


	public VideoForwarderReplier(int port, Action<VideoForwarderOpt>? optFun = null)
	{
		opt = VideoForwarderOpt.Build(optFun);
		this.port = port;
		whenEvt = new Subject<IVidFwEvt>().D(d);
		cancelMap = new Dictionary<string, CancellationTokenSource>().D(d);
		linkSrcRequester = new LinkSrcRequester(whenEvt, opt.BufferSize, opt.HeaderTimeout, opt.ChunkTimeout);
		fileSrcRequester = new FileSrcRequester();
	}

	private CancellationToken GetNameCancelToken(string name)
	{
		if (cancelMap.TryGetValue(name, out var cancelSource))
		{
			cancelSource.Cancel();
			cancelSource.Dispose();
		}
		cancelSource = cancelMap[name] = new CancellationTokenSource();
		return cancelSource.Token;
	}

	private void RemoveNameToken(string name)
	{
		if (cancelMap.TryGetValue(name, out var cancelSource))
		{
			cancelMap.Remove(name);
			cancelSource.Dispose();
		}
	}

	public async Task<bool> Reply(ReqRes reqRes)
	{
		var (req, res) = reqRes;
		var url = req.GetUrl();
		if (!url.StartsWith(opt.UrlPrefix, StringComparison.InvariantCultureIgnoreCase)) return false;
		var name = url[opt.UrlPrefix.Length..];
		var nameCancelToken = GetNameCancelToken(name);
		var src = linkMapper.GetSrc(name);
		res.ContentType = "video/mp4";
		res.ContentEncoding = Encoding.Default;
		var rngReqStr = req.Headers.Get(RngReq.HeaderName);
		if (rngReqStr == null) throw new ArgumentException("src request has no 'range' header");
		var rngReq = RngReq.Parse(rngReqStr);

		var reqId = reqCounter++;
		whenEvt.OnNext(new StartVidFwEvt(reqId, name, src, rngReq));

		switch (src.Type)
		{
			case VidSrcType.File:
				await fileSrcRequester.Request(rngReq, src.File!, res);
				break;

			case VidSrcType.Link:
				await linkSrcRequester.Request(reqId, rngReq, src.Link!, res, nameCancelToken);
				break;

			default:
				throw new ArgumentException();
		}

		RemoveNameToken(name);

		return true;
	}
}



class LinkSrcRequester
{
	private readonly ISubject<IVidFwEvt> whenEvt;
	private readonly int bufferSize;
	private readonly TimeSpan headerTimeout;
	private readonly TimeSpan chunkTimeout;
	private readonly HttpClient client = new();

	public LinkSrcRequester(ISubject<IVidFwEvt> whenEvt, int bufferSize, TimeSpan headerTimeout, TimeSpan chunkTimeout)
	{
		this.whenEvt = whenEvt;
		this.bufferSize = bufferSize;
		this.headerTimeout = headerTimeout;
		this.chunkTimeout = chunkTimeout;
	}



	public async Task Request(int reqId, RngReq rngReq, VidLinkFw link, HttpListenerResponse localRes, CancellationToken nameCancelToken)
	{
		try
		{
			var req = MkReq(link, rngReq);
			using var d = new Disp();
			var cancelReqHeadersToken = nameCancelToken.WithTimeout(headerTimeout).D(d);
			var watch = Stopwatch.StartNew();
			var remoteRes = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancelReqHeadersToken);
			if (!AssignAndCheckStatusCode(localRes, remoteRes))
			{
				whenEvt.OnNext(new ErrHeadersVidFwEvt(reqId, remoteRes.StatusCode));
				return;
			}
			var rngRes = remoteRes.GetRngRes();
			rngRes.WriteToResponse(localRes);

			whenEvt.OnNext(new HeadersVidFwEvt(reqId, rngRes, remoteRes.Content.Headers.ContentLength ?? 0, watch.Elapsed));

			var cancelReadContentToken = nameCancelToken.WithTimeout(headerTimeout).D(d);
			await using var remoteResStream = await remoteRes.Content.ReadAsStreamAsync(cancelReadContentToken);

			await CopyStream(remoteResStream, localRes.OutputStream, reqId, nameCancelToken);
		}
		catch (OperationCanceledException)
		{
			whenEvt.OnNext(new ErrTimeoutVidFwEvt(reqId));
		}
		//	The specified network name is no longer available.
		//catch (HttpListenerException ex) when (ex.ErrorCode == 64)
		//{
		//	whenEvt.OnNext(new ErrNetworkUnavailableVidFwEvt(reqId));
		//}
		catch (Exception ex)
		{
			whenEvt.OnNext(new ErrUnexpectedException(reqId, ex));
		}
	}

	private async Task CopyStream(Stream src, Stream dst, int reqId, CancellationToken nameCancelToken)
	{
		var buffer = new byte[bufferSize];
		int read;
		
		async Task<int> ReadChunk()
		{
			using var d = new Disp();
			var cancelToken = nameCancelToken.WithTimeout(chunkTimeout).D(d);
			//var watch = Stopwatch.StartNew();
			var cnt = await src.ReadAsync(buffer, 0, buffer.Length, cancelToken);
			//whenEvt.OnNext(new ChunkVidFwEvt(reqId, cnt, watch.Elapsed));
			return cnt;
		}

		while ((read = await ReadChunk()) > 0)
		{
			//whenEvt.OnNext(new MsgVidFwEvt(reqId, "WriteAsync before"));
			try
			{
				using var d = new Disp();
				var cancelToken = nameCancelToken.WithTimeout(chunkTimeout).D(d);
				await dst.WriteAsync(buffer, 0, read, cancelToken);
			}
			/*catch (OperationCanceledException) // Handled above
			{
				return;
			}*/
			//	The specified network name is no longer available.
			catch (HttpListenerException ex) when (ex.ErrorCode == 64)
			{
				whenEvt.OnNext(new ErrClientClosedConnection(reqId));
				return;
			}
			//whenEvt.OnNext(new MsgVidFwEvt(reqId, "WriteAsync after"));
		}
	}


	private static HttpRequestMessage MkReq(VidLinkFw link, RngReq rngReq)
	{
		var req = new HttpRequestMessage(HttpMethod.Get, link.Link);
		if (link.Headers != null)
		{
			foreach (var (key, val) in link.Headers)
				req.Headers.Add(key, val);
		}
		rngReq.WriteToRequest(req);
		return req;
	}

	private static bool AssignAndCheckStatusCode(HttpListenerResponse res, HttpResponseMessage dstRes)
	{
		res.StatusCode = (int)dstRes.StatusCode;
		return dstRes.StatusCode == HttpStatusCode.PartialContent;
	}
}




class FileSrcRequester
{
	private const long LngCap = 100 * 1024;
	private readonly Dictionary<string, byte[]> fileCache = new();

	public async Task Request(RngReq rngReq, string file, HttpListenerResponse res)
	{
		if (!fileCache.TryGetValue(file, out var data))
			data = fileCache[file] = await File.ReadAllBytesAsync(file);
		if (!rngReq.CheckFit(data.Length))
			throw new ArgumentException($"{rngReq} doesn't fit (length:{data.Length})");

		var start = rngReq.Start;
		var totalLength = data.Length;
		var maxLng = totalLength - start;
		var lng = Math.Min(maxLng, LngCap);

		var rngRes = new RngRes(start, start + lng - 1, totalLength);

		rngRes.WriteToResponse(res);
		
		await res.OutputStream.WriteAsync(data, (int)rngReq.Start, (int)lng);
	}
}



file static class VideoForwarderExt
{
	public static RngRes GetRngRes(this HttpResponseMessage res)
	{
		var rngResVals = res.Content.Headers.GetValues(RngRes.HeaderName).ToArray();
		if (rngResVals.Length != 1)
			throw new ArgumentException($"wrong number of values in {RngRes.HeaderName}: {rngResVals.Length}");
		return RngRes.Parse(rngResVals[0]);
	}

	public static string GetUrl(this HttpListenerRequest req)
	{
		if (req.Url == null) throw new ArgumentException();
		return req.Url.AbsolutePath.RemoveLeadingSlash();
	}

	private static string RemoveLeadingSlash(this string s) => (s[0] == '/') switch
	{
		true => s[1..],
		false => s
	};
}
