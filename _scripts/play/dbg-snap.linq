<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeExtrasLib\bin\Debug\net7.0\DynaServeExtrasLib.dll</Reference>
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <NuGetReference>AngleSharp.Diffing</NuGetReference>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Serving.Debugging.Structs</Namespace>
  <Namespace>DynaServeLib.Utils.Exts</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>static DynaServeExtrasLib.Utils.HtmlNodeExtraMakers</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>AngleSharp.Diffing</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
</Query>

const string FileDomServer = @"C:\tmp\finding-bugs\dom-server.html";
const string FileDomClient = @"C:\tmp\finding-bugs\dom-client.html";

private static readonly SerialDisp<Disp> serD = new();
public static Disp D = null!;

void OnStart()
{
	serD.Value = null;
	serD.Value = D = new Disp();
}


void Main()
{
	IDisposable? ctrlD = null;
	var ctrlRef = new Ref();
	Util.InvokeScript(false, "eval", "document.body.style = 'font-family: consolas'");
	
	Serv.Start(
		opt =>
		{
			opt.AddCss(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)!, "dbg-snap-css"));
		},
		Div("btnrow").Wrap(
			TBtn().Txt("Dbg").OnClick(async () =>
			{
				var nfo = await Serv.Dbg.GetSnap();
				ShowSnap(nfo);
			}),
		
			TBtn().Txt("AddCtrl").OnClick(() =>
			{
				ctrlD?.Dispose();
				ctrlD = 
					Serv.AddNodeToBody(
						new HtmlNode("input")
							.Attr("type", "range")
							.Attr("min", "10")
							.Attr("max", "50")
							.Attr("value", "28")
							.Ref(ctrlRef)
							.HookArg("change", valStr =>
							{
								var valNext = int.Parse(valStr);
								$"val <- {valNext}".Dump();
							}, "this.value")
					);
			}),
			
			TBtn().Txt("DelCtrl").OnClick(() =>
			{
				ctrlD?.Dispose();
			})
		)
	).D(D);
}

void ShowSnap(DbgSnap snap)
{
	var serverDom = snap.ServerDom;
	var clientDom = snap.ClientDom;
	var serverDomHtml = snap.ServerDom.Fmt();
	var clientDomHtml = snap.ClientDom.Fmt();
	File.WriteAllText(FileDomServer, serverDomHtml);
	File.WriteAllText(FileDomClient, clientDomHtml);
	
	var diffs = DiffBuilder
	    .Compare(serverDomHtml)
		.WithTest(clientDomHtml)
		.WithOptions(opt =>
		{
			opt.AddDefaultOptions();
		})
		.Build()
		.ToArray();
	
	var sb = new StringBuilder("Snap received");
	if (diffs.Length == 0)
		sb.Append(" -> no diffs found");
	else
		sb.Append($"  -> {diffs.Length} diffs found");
	sb.Dump();
	
	var (servIds, servNoIdCnt) = serverDom.Body!.GetAllChildrenIds();
	var (clientIds, clientNoIdCnt) = clientDom.Body!.GetAllChildrenIds();
	var servStats = NodeUtils.ComputeRefreshStats(servIds, snap.RefreshMap, servNoIdCnt);
	var clientStats = NodeUtils.ComputeRefreshStats(clientIds, snap.RefreshMap, clientNoIdCnt);
	
	
	$"              -> Refreshers vs Serv   => {servStats}".Dump();
	$"                 Refreshers vs Client => {clientStats}".Dump();
}

internal record RefreshStats(
	int TotalNodeCount,
	int TotalRefresherCount,
	int ForgottenRefreshers,
	int MissingRefreshers,
	int DupIds,
	int NoIdCnt
)
{
	public override string ToString() => (ForgottenRefreshers == 0 && MissingRefreshers == 0 && DupIds == 0 && NoIdCnt == 0) switch
	{
		true => $"ok    (nodes:{TotalNodeCount} refreshers:{TotalRefresherCount})",
		false => $"error (nodes:{TotalNodeCount} refreshers:{TotalRefresherCount})  forgotten:{ForgottenRefreshers}  missing:{MissingRefreshers}  dupIds:{DupIds}  noidCnt:{NoIdCnt}",
	};
}

static class NodeUtils
{
	public static RefreshStats ComputeRefreshStats(string[] ids, Dictionary<string, string[]> map, int noidCnt)
	{
		var totalNodeCount = ids.Length;
		var totalRefresherCount = map.Sum(kv => kv.Value.Length);
		var refreshIds = map.Keys.ToArray();
		var forgotten = refreshIds.WhereNotToArray(ids.Contains).Length;
		var missing = ids.WhereNotToArray(refreshIds.Contains).Length;
		var dupIds = ids.Length - ids.Distinct().ToArray().Length;
		return new RefreshStats(totalNodeCount, totalRefresherCount, forgotten, missing, dupIds, noidCnt);
	}
	
	public static (string[], int) GetAllChildrenIds(this IElement elt)
	{
		var list = new List<string>();
		var noidCnt = 0;
		void Recurse(IElement e)
		{
			if (e.Id != null)
				list.Add(e.Id);
			else
			{
				if (e.NodeName != "BODY" && e.ClassName != "DynaServVerCls")
				{
					$"NoId for {e.NodeName} cls:{e.ClassName}".Dump();
					noidCnt++;
				}
			}
			foreach (var ec in e.Children)
				Recurse(ec);
		}
		Recurse(elt);
		return (list.ToArray(), noidCnt);
	}
}

