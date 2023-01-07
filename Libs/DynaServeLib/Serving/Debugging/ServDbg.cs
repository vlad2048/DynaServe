/*
using System.Reactive.Linq;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic;
using DynaServeLib.Serving.Debugging.Structs;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils.Exts;
using PowMaybe;
using PowRxVar;

namespace DynaServeLib.Serving.Debugging;

public class ServDbg : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly DomOps domOps;
	private readonly Messenger messenger;
	private readonly SemaphoreSlim slim;
	private Maybe<ClientDomSnapshot> receivedSnap = May.None<ClientDomSnapshot>();

	internal ServDbg(DomOps domOps, Messenger messenger)
	{
		this.domOps = domOps;
		this.messenger = messenger;
		slim = new SemaphoreSlim(0).D(d);

		messenger.WhenClientMsg
			.OfType<AnswerDomSnapshotClientMsg>()
			.Subscribe(e =>
			{
				receivedSnap = May.Some(e.Msg);
				slim.Release();
			}).D(d);
	}

	public async Task<DbgSnap> GetSnap()
	{
		var domDbg = domOps.GetDbgNfo();
		receivedSnap = May.None<ClientDomSnapshot>();
		messenger.SendToClient(ServerMsg.MkReqDomSnapshot());
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
*/
