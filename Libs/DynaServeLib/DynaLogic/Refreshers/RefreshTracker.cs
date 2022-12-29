using AngleSharp.Dom;
using DynaServeLib.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace DynaServeLib.DynaLogic.Refreshers;

class RefreshTracker : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly Dictionary<string, Disp> refresherMap;
	private RefreshCtx? ctx;
	private RefreshCtx Ctx => ctx ?? throw new ArgumentException("Start not called");

	public RefreshTracker()
	{
		refresherMap = new Dictionary<string, Disp>().D(d);
	}

	public string DbgGetRefresherIds() => refresherMap.Keys.JoinText(", ");

	public void Start(RefreshCtx refreshCtx)
	{
		if (ctx != null) throw new ArgumentException();
		ctx = refreshCtx;
	}

	public void AddRefreshers(IEnumerable<IRefresher> refreshers)
	{
		if (ctx == null) throw new ArgumentException();

		var grps = refreshers.GroupBy(e => e.NodeId);
		foreach (var grp in grps)
		{
			if (refresherMap.ContainsKey(grp.Key))
				throw new ArgumentException();
			var rd = new Disp();
			foreach (var refresher in grp)
				refresher.Activate(Ctx).D(rd);
			refresherMap[grp.Key] = rd;
		}
	}

	public void RemoveChildrenRefreshers(IElement node, bool includeRoot)
	{
		var ids = node.GetAllChildrenIds(includeRoot);
		ids.ForEach(RemoveRefresher);
	}

	private void RemoveRefresher(string id)
	{
		if (ctx == null) throw new ArgumentException();

		//L($"RemoveRefresher({id}) inMap:{refresherMap.ContainsKey(id)}");

		if (refresherMap.TryGetValue(id, out var rd))
		{
			rd.Dispose();
			refresherMap.Remove(id);
		}
	}

	private static void L(string s) => Console.WriteLine(s);
}


file static class RefreshTrackerExt
{
	public static string[] GetAllChildrenIds(this IElement elt, bool includeRoot)
	{
		var list = new List<string>();

		if (includeRoot)
			list.Add(elt.Id);

		void Recurse(IElement e)
		{
			list.Add(e.Id);
			foreach (var ec in e.Children)
				Recurse(ec);
		}

		foreach (var eltChild in elt.Children)
			Recurse(eltChild);

		return list.ToArray();
	}
}