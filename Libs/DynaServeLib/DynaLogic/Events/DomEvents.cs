using DynaServeLib.Nodes;

namespace DynaServeLib.DynaLogic.Events;

interface IDomEvt {}
record UpdateChildrenDomEvt(string NodeId, HtmlNode[] Children) : IDomEvt;
record AddBodyNode(HtmlNode Node) : IDomEvt;
record RemoveBodyNode(string NodeId) : IDomEvt;


/*interface ITreeEvtObs
{
	IObservable<ITreeEvt> WhenEvt { get; }
}

interface ITreeEvtSig
{
	void SignalEvt(ITreeEvt evt);
}

class TreeEvents : ITreeEvtSig, ITreeEvtObs, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<ITreeEvt> whenEvt;
	public IObservable<ITreeEvt> WhenEvt => whenEvt.AsObservable();
	public void SignalEvt(ITreeEvt evt) => whenEvt.OnNext(evt);

	private TreeEvents()
	{
		whenEvt = new Subject<ITreeEvt>().D(d);
	}

	public static (ITreeEvtSig, ITreeEvtObs, IDisposable) Make()
	{
		var evt = new TreeEvents();
		return (evt, evt, evt);
	}
}*/