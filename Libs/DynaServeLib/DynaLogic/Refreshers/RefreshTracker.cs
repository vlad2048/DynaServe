/*
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.DomLogic;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.DynaLogic.Refreshers;

class RefreshTracker : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly DomOps domOps;
	private IHtmlDocument dom => domOps.Dom;
	private readonly Dictionary<IElement, Disp> map;

	public RefreshTracker(DomOps domOps)
	{
		this.domOps = domOps;
		map = new Dictionary<IElement, Disp>().D(d);
	}

	public void Add(IElement parent, NodsRefs nodsRefs)
	{
		parent.AppendChildren(nodsRefs.Nods);
		AddRefs(nodsRefs.RefreshMap);
	}

	private void AddRefs(RefreshMap refMap)
	{
		foreach (var (node, refs) in refMap)
		{
			if (map.ContainsKey(node)) throw new ArgumentException();
			var refD = new Disp();
			foreach (var @ref in refs)
				@ref.Activate(domOps);
			map[node] = refD;
		}
	}
}
*/


/*
record RefreshTrackerDbgNfo(
	Dictionary<string, string[]> Map
);

class RefreshTracker : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Dictionary<string, Disp> refresherMap;
	private readonly DomOps domOps;
	private readonly Dictionary<string, IRefresher[]> dbgMap = new();

	public string DbgGetRefresherIds() => refresherMap.Keys.JoinText(", ");

	public RefreshTrackerDbgNfo GetDbgNfo() => new(dbgMap.ToDictionary(
		t => t.Key,
		t => t.Value.SelectToArray(e => e.GetType().Name)
	));

	public RefreshTracker(DomOps domOps)
	{
		refresherMap = new Dictionary<string, Disp>().D(d);
		this.domOps = domOps;
	}

	public void ReplaceRefreshers(IEnumerable<string> refreshersPrevKeys, IEnumerable<IRefresher> refreshersNext)
	{
		foreach (var refresherPrevKey in refreshersPrevKeys)
			RemoveRefresher(refresherPrevKey);
		AddRefreshers(refreshersNext);
	}

	
	public void AddRefreshers(IEnumerable<IRefresher> refreshers)
	{
		var grps = refreshers.GroupBy(e => e.NodeId);
		foreach (var grp in grps)
		{
			if (refresherMap.ContainsKey(grp.Key))
				throw new ArgumentException("refresherMap.ContainsKey(grp.Key)");
			var rd = new Disp();
			foreach (var refresher in grp)
				refresher.Activate(domOps).D(rd);

			try
			{
				dbgMap[grp.Key] = grp.ToArray();
			}
			catch (Exception ex)
			{
				throw new ArgumentException("dbgMap[grp.Key] = grp.ToArray()", ex);
			}
			try
			{
				refresherMap[grp.Key] = rd;
			}
			catch (Exception ex)
			{
				throw new ArgumentException("refresherMap[grp.Key] = rd;", ex);
			}
		}
	}


	public void RemoveChildrenRefreshers(IElement node, bool includeRoot)
	{
		var ids = node.GetAllChildrenIds(includeRoot);														
		ids.ForEach(RemoveRefresher);
	}




	private void RemoveRefresher(string id)
	{
		if (refresherMap.TryGetValue(id, out var rd))
		{
			rd.Dispose();
			refresherMap.Remove(id);
			dbgMap.Remove(id);
		}
	}
}
*/