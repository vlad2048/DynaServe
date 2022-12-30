using System.Reactive.Linq;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.Debugging.Structs;
using DynaServeLib.Serving.Syncing;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils.Exts;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Serving.Debugging;

public class ServDbg : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Dom dom;
	private readonly Syncer syncer;
	private readonly SemaphoreSlim slim;
	private Maybe<ClientDomSnapshot> receivedSnap = May.None<ClientDomSnapshot>();

	internal ServDbg(Dom dom, Syncer syncer)
	{
		this.dom = dom;
		this.syncer = syncer;
		slim = new SemaphoreSlim(0).D(d);

		syncer.WhenClientMsg
			.Where(e => e.Type == ClientMsgType.AnswerDomSnapshot)
			.Subscribe(e =>
			{
				receivedSnap = May.Some(e.ClientDomSnapshot!);
				slim.Release();
			}).D(d);
	}

	public async Task<DbgSnap> GetSnap()
	{
		var domDbg = dom.GetDbgNfo();
		receivedSnap = May.None<ClientDomSnapshot>();
		syncer.SendToClient(ServerMsg.MkReqDomSnapshot());
		await slim.WaitAsync();
		var receivedSnapVal = receivedSnap.Ensure();

		var dbgSnap = new DbgSnap(
			domDbg.ServerDom,
			ReconstructClientDom(receivedSnapVal),
			domDbg.RefreshTrackerDbgNfo.Map
		);
		return dbgSnap;
	}

	private static IHtmlDocument ReconstructClientDom(ClientDomSnapshot snap) =>
		$"""
		<!DOCTYPE html>
		<html>
			{snap.Head}
			{snap.Body}
		</html
		"""
			.Parse();
}