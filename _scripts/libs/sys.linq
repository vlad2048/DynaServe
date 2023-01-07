<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <NuGetReference>PowTrees.LINQPad</NuGetReference>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
  <Namespace>C = LINQPad.Controls.Control</Namespace>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Utils.Exts</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>PowBasics.Geom</Namespace>
  <Namespace>PowBasics.StringsExt</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>PowTrees.LINQPad</Namespace>
  <Namespace>PowTrees.LINQPad.DrawerLogic</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>PowTrees.Algorithms</Namespace>
</Query>

void Main()
{
	var html = """
		<!DOCTYPE html>
		<html>
			<head>
				<link rel="icon" type="image/png" href="image/creepy-icon.png" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<title>DynaServe</title>
				<link href='http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900' rel='stylesheet' type='text/css'>
			</head>
			<body>
				<div id="id-1"></div>
				<span class="cls"></span>
			</body>
		</html>
		""";
	
	//html.Prn().Dump();
	var dom = html.Parse();
	var body = dom.FindDescendant<IHtmlBodyElement>()!;
	var div = body.FindDescendant<IHtmlDivElement>()!;

	dom.Dump();
	
	body.Dump();
	
	div.Dump();
}


private static readonly SerialDisp<Disp> serD = new();
public static Disp D = null!;

void OnStart()
{
	serD.Value = null;
	serD.Value = D = new Disp();
	LinqpadDump.DmpFun = e => e.Dump();
	LinqpadDump.ClearFun = DrawExt.Clear;
	Util.HtmlHead.AddStyles(DrawUtils.Css);
}


public static object ToDump(object o) => o switch
{
	//INode e => e.Prn(),
	INode e => e.PrnTxt(),
	_ => o
};


public static class DrawExt
{
	public static void Clear()
	{
		Util.HtmlHead.AddScript("""
			function clearFun() {
				console.log('start')
				const root = document.getElementById('final');
				var arr = root.children;
				for (let i = arr.length - 1; i >= 0; i--) {
					const elt = arr[i];
					const name = elt.tagName.toLowerCase();
					elt.remove();
				}
			}
			"""
		);
		Util.InvokeScript(false, "clearFun");
	}

	public static C PrnTxt(this INode node)
	{
		var tree = node.ToTree();
		var txt = tree.LogToString(opt =>
		{
			opt.FormatFun = e => e.GetStr();
		});
		return new DumpContainer(Util.FixedFont(txt));
	}
	
	public static C Prn(this INode node)
	{
		var tree = node.ToTree();
		var ctrl = PanelGfx.Make(gfx =>
		{
			gfx.TreeCtrl<INode, Control>(
				new Pt(0, 0),
				tree,
				o => o.GetStr(),
				(node, str) => node.MkNodeCtrl(),
				opt =>
				{
				}
			);
		});
		ctrl.CssClass = "tree";
		return ctrl;
	}
}


static class DrawUtils
{
	// *******
	// * CSS *
	// *******
	public static string Css = """
		:root {
			--del: #888;
			--txt: #00f;
			--white: #fff;
			
			--key-id:  #ebd61e;
			--val-id:  #13eb77;
			--key-cls: #c4b52d;
			--val-cls: #2bc271;
		}
		.tree {
			margin-bottom: 15px;
		}		
		.node {
			font-family: consolas;
			white-space: nowrap;
		}
		""";
	
	// ********
	// * Text *
	// ********
	static string SName(this INode e) => e.NodeName.ToLowerInvariant();					// span
	static string SId(this IElement e) => e.Id.SOpt(e => $" id='{e}'");					// id="id-1"
	static string SCls(this IElement e) => e.ClassName.SOpt(e => $" class='{e}'");		// class="cls"
	
	// **********
	// * Styles *
	// **********
	static C TDel(this C c) => c.Col("del");				// < > = "
	static C TName(this C c) => c.Col("white");				// span
	static C TKeyId(this C c) => c.Col("key-id");			// id="id-1"
	static C TValId(this C c) => c.Col("val-id");
	static C TKeyCls(this C c) => c.Col("key-cls");			// class="cls"
	static C TValCls(this C c) => c.Col("val-cls");
	
	// **************
	// * Build Text *
	// **************
	public static string GetStr(this INode o) => o switch
	{
		IText e => e.TextContent.CleanTxt(),
		IElement e => $"<{e.SName()}{e.SId()}{e.SCls()}>",
		INode e => e.NodeName,
	};
	
	// *****************
	// * Build Control *
	// *****************
	public static C MkNodeCtrl(this INode o) { var ctrls = o.GetCtrls(); var div = new Div(ctrls); div.CssClass = "node"; return div; }
	private static C[] GetCtrls(this INode o)
	{
		var arr = o switch
		{
			IText e => new C[] { Sp(e.GetStr()) },
			IElement e => new C[][]
			{
				Sp("<").TDel().N(),
				Sp(e.SName()).TName().N(),
				e.Id.TOpt(e => new C[] {
					Sp(" id").TKeyId(),
					Sp("=\"").TDel(),
					Sp(e).TValId(),
					Sp("\"").TDel()
				}),
				e.ClassName.TOpt(e => new C[] {
					Sp(" class").TKeyCls(),
					Sp("=\"").TDel(),
					Sp(e).TValCls(),
					Sp("\"").TDel()
				}),
				Sp(">").TDel().N(),
			}.F(),
			INode e => new C[] { Sp(e.GetStr()) },
		};
		return arr.WhereToArray(e => e != null);
	}
	
	
	
	// *********
	// * Utils *
	// *********
	static Span Sp(string txt) => new(txt);
	static string SOpt(this string? s, Func<string, string> fun) => s switch
	{
		null => "",
		not null => fun(s)
	};
	static C[] TOpt(this string? s, Func<string, C[]> fun) => s switch
	{
		null => Array.Empty<C>(),
		not null => fun(s)
	};
	static C Col(this C c, string col) { c.Styles["color"] = $"var(--{col})"; return c; }
	
	static C[] N(this C c) => new C[] { c };
	static C[] F(this C[][] source) => source.SelectMany(e => e).ToArray();
	static string CleanTxt(this string str) => str.Replace("\n", "").Replace("\r", "").Trim().Truncate(16);
}
