using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.DynaLogic.Refreshers;
using PowRxVar;

namespace DynaServeLib.Nodes;


public class HtmlNode : IRoDispBase
{
	internal Disp D { get; } = new();
	public bool IsDisposed { get; private set; }
	public void Dispose() { if (IsDisposed) return; IsDisposed = true; D.Dispose(); }
	private readonly ISubject<Unit> whenDisposed = new AsyncSubject<Unit>();
	public IObservable<Unit> WhenDisposed => whenDisposed.AsObservable();

	private static int idCnt;

	internal readonly List<IRefresher> refreshers = new();

	public string TagName { get; }
	public string Id { get; set; } = $"id-{idCnt++}";
	public string? Cls { get; set; }
	public string? Txt { get; set; }
	public Dictionary<string, string> Attrs { get; } = new();
	public HtmlNode[] Children { get; private set; } = Array.Empty<HtmlNode>();
	internal IReadOnlyList<IRefresher> Refreshers => refreshers.AsReadOnly();

	public HtmlNode(string tagName)
	{
		TagName = tagName;
		refreshers.Add(new NodeRefresher(Id, D));
	}

	internal void SetChildren(HtmlNode[] children)
	{
		if (Children.Any()) throw new ArgumentException();
		Children = children;
	}

	internal void SetChildrenUpdater(IObservable<Unit> when, Func<IEnumerable<HtmlNode>> fun)
	{
		if (Children.Any()) throw new ArgumentException();
		refreshers.Add(new ChildRefresher(Id, when, () => fun().ToArray()));
	}

	internal void AddEvtHook(string evtName, Action action)
	{
		Attrs[$"on{evtName}"] = $"sockEvt('{Id}', '{evtName}')";
		refreshers.Add(new EvtRefresher(Id, evtName, action));
	}

	internal void AddEvtHookArg(string evtName, Action<string> action, string argExpr)
	{
		Attrs[$"on{evtName}"] = $"sockEvtArg('{Id}', '{evtName}', {argExpr})";
		refreshers.Add(new EvtArgRefresher(Id, evtName, action, argExpr));
	}

	internal void UpdateAttrWhen(string attrName, IObservable<string?> valObs) =>
		refreshers.Add(new AttrRefresher(Id, attrName, valObs));


	internal IElement MakeElt(IHtmlDocument doc, IDomTweaker[] domTweakers)
	{
		var elt = doc.CreateElement(TagName);
		elt.Id = Id;
		if (Cls != null)
			elt.ClassName = Cls;
		if (Txt != null)
			elt.TextContent = Txt;
		foreach (var (key, val) in Attrs)
			elt.SetAttribute(key, val);

		foreach (var domTweaker in domTweakers)
			domTweaker.TweakNode(elt);

		return elt;
	}
}