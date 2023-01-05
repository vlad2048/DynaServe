using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Refreshers;

namespace DynaServeLib.Nodes;


public class HtmlNode
{
	private readonly ISubject<Unit> whenDisposed = new AsyncSubject<Unit>();
	public IObservable<Unit> WhenDisposed => whenDisposed.AsObservable();

	internal readonly List<IRefresher> refreshers = new();
	internal IRefresher[] Refreshers => refreshers.ToArray();
	internal void AddRefresher(IRefresher refresher) => refreshers.Add(refresher);

	public bool IsTxt { get; }
	public string TagName { get; }
	public string? Id { get; set; }
	public string? Cls { get; set; }
	public string? Txt { get; set; }
	public Dictionary<string, string> Attrs { get; } = new();
	public HtmlNode[] Children { get; internal set; } = Array.Empty<HtmlNode>();

	public static implicit operator HtmlNode[](HtmlNode n) => new[] { n };

	public HtmlNode(string tagName) : this(false, null)
	{
		TagName = tagName;
	}

	public static HtmlNode MkTxt(string txt) => new(true, txt);

	private HtmlNode(bool isTxt, string? txt)
	{
		IsTxt = isTxt;
		Txt = txt;
		TagName = "dummy";
	}

	internal IElement MakeElt(IHtmlDocument doc)
	{
		var elt = doc.CreateElement(TagName);
		if (Id != null)
			elt.Id = Id;
		if (Cls != null)
			elt.ClassName = Cls;
		if (Txt != null)
			elt.TextContent = Txt;
		foreach (var (key, val) in Attrs)
			elt.SetAttribute(key, val);

		return elt;
	}
}