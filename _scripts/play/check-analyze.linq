<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\DynaServe\Libs\DynaServeLib\bin\Debug\net7.0\DynaServeLib.dll</Reference>
  <NuGetReference>Mono.Cecil</NuGetReference>
  <Namespace>DynaServeLib</Namespace>
  <Namespace>DynaServeLib.Logging</Namespace>
  <Namespace>DynaServeLib.Nodes</Namespace>
  <Namespace>DynaServeLib.Serving.FileServing.StructsEnum</Namespace>
  <Namespace>Mono.Cecil</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>static DynaServeLib.Nodes.Ctrls</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowBasics.StringsExt</Namespace>
</Query>

public const string DllBaseFile = "System.Reactive.dll";
public const string ClassName = "System.Reactive.Linq.Observable";
public static string DllFile => Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)!, DllBaseFile);

void Main()
{
	var meths = DllLoader.Load();
	var fvars = new FilterVars().D(D);

	Serv.Start(
		opt =>
		{
			//opt.Logr = new LPLogger();
			opt.AddSlnFolder(Path.GetDirectoryName(Util.CurrentQueryPath)!);
			opt.Serve(FCat.Css, "check-analyze-css");
		},
		Div("main").Wrap(
			TTable().Wrap(
				THead().Wrap(Tr().Wrap(
						Th().Wrap(TextBox(fvars.TxtRet)),
						Th().Wrap(TextBox(fvars.TxtName)),
						Th().Wrap(TextBox(fvars.TxtParams))
					)
				),
				
				TBody().Wrap(
					fvars.Filt.ToUnit(),
					() => meths
						.Where(e => fvars.Filt.V.IsMatch(e))
						.Select(e =>
							Tr().Wrap(
								Td("col-ret").Txt(e.Ret),
								Td("col-name").Txt(e.Name),
								Td("col-params").Wrap(e.MkParamsNodes())
							)
					)
				)
			)
		)
	).D(D);
}

				/*TBody().Wrap(
					meths
						.Select(e =>
							Tr().Attr("style", fvars.Filt.Select(filt => filt.IsMatch(e) ? null : "display:none")).Wrap(
								Td("col-ret").Txt(e.Ret),
								Td("col-name").Txt(e.Name),
								Td("col-params").Wrap(
									e.MkParamsNodes()
								)
							)
						)
				)*/



record Filt(string Ret, string Name, string Params)
{
	public bool IsMatch(MethNfo e) =>
		IsStrMatch(Ret, e.Ret) &&
		IsStrMatch(Name, e.Name) &&
		IsStrMatch(Params, e.ParamsStr);
	
	private static bool IsStrMatch(string searchStr, string methodStr) =>
		methodStr
			.Trim()
			.Contains(searchStr.Trim(), StringComparison.InvariantCultureIgnoreCase);
}

class FilterVars : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();
	
	public IRwVar<string> TxtRet { get; }
	public IRwVar<string> TxtName { get; }
	public IRwVar<string> TxtParams { get; }
	
	public IRoVar<Filt> Filt { get; }
	
	public FilterVars()
	{
		TxtRet = Var.Make("").D(d);
		TxtName = Var.Make("").D(d);
		TxtParams = Var.Make("").D(d);
		Filt = Var.Expr(() => new Filt(TxtRet.V, TxtName.V, TxtParams.V));
	}
}



public static class MethDispUtils
{
	private const string ColOff = "seg-off";

	public static HtmlNode[] MkParamsNodes(this MethNfo e)
	{
		var spans = new List<HtmlNode>();
		void Add(string content, string cls) => spans.Add(TSpan(cls).Txt(content));
		void AddSp() => spans.Add(TSpan("seg-sp").Txt(" "));
		Add("(", ColOff);
		for (var i = 0; i < e.Params.Length; i++)
		{
			if (i > 0)
			{
				Add(",", "seg-off");
				AddSp();
			}
			var p = e.Params[i];
			Add(p.Type, "seg-type");
			AddSp();
			Add(p.Name, "seg-name");
		}
		Add(")", ColOff);
		return spans.ToArray();
	}
	private static string FixSpaces(this string s) => s.Replace(" ", "&nbsp;");
}



private static readonly SerialDisp<Disp> serD = new();
public static Disp D = null!;
void OnStart() { serD.Value = null; serD.Value = D = new Disp(); }



public static HtmlNode TTable(string? cls = null) => new HtmlNode("table").Cls(cls);
public static HtmlNode THead(string? cls = null) => new HtmlNode("thead").Cls(cls);
public static HtmlNode TBody(string? cls = null) => new HtmlNode("tbody").Cls(cls);
public static HtmlNode Tr(string? cls = null) => new HtmlNode("tr").Cls(cls);
public static HtmlNode Th(string? cls = null) => new HtmlNode("th").Cls(cls);
public static HtmlNode Td(string? cls = null) => new HtmlNode("td").Cls(cls);
public static HtmlNode TSpan(string? cls = null) => new HtmlNode("span").Cls(cls);



public static class DllLoader
{
	public static MethNfo[] Load()
	{
		using var ass = AssemblyDefinition.ReadAssembly(DllFile);
		var module = ass.Modules.Single();
		var methods =
		(
			from type in module.Types
			where type.FullName == ClassName
			from method in type.Methods
			where method.IsPublic
			select new MethNfo(method)
		).ToArray();
		return methods;
	}
}




public record MethParam(string Name, string Type);

public class MethNfo
{
	public MethodDefinition M { get; }
	public string Name { get; }
	public string Ret { get; }
	public MethParam[] Params { get; }
	public string ParamsStr { get; }
	public string FullStr { get; }

	public MethNfo(MethodDefinition m)
	{
		this.M = m;
		Name = m.Name.Simplify();
		Ret = m.ReturnType.FullName.Simplify();
		Params = m.Parameters.Select(e => new MethParam(
			e.Name,
			e.ParameterType.FullName.Simplify()
		)).ToArray();
		ParamsStr = string.Join(", ", Params.Select(e => $"{e.Type} {e.Name}"));
		FullStr = $"{Ret} {Name} {ParamsStr}";
	}
}





public static class AssUtils
{
	public static string Simplify(this string s) => s
		/*.Replace("System.", "")
		.Replace("DynamicData.", "")
		.Replace("Collections.Generic.", "")
		.Replace("Linq.Expressions.", "")
		.Replace("Reactive.Concurrency.", "")
		.Replace("Binding.", "")*/
		.KeepLastPartOnly("<>")
		.Replace("`1", "")
		.Replace("`2", "")
		.Replace("`3", "")
		.Replace("`4", "")
		.Replace("Nullable<TimeSpan>", "TimeSpan?")
		.Replace("Void", "void")
		.Replace("Boolean", "bool")
		.Replace("Double", "double")
		.Replace("Single", "float")
		.Replace("Decimal", "decimal")
		.Replace("Int64", "long")
		.Replace("UInt64", "ulong")
		.Replace("Int32", "int")
		.Replace("UInt32", "uint")
		.Replace("Int16", "short")
		.Replace("UInt16", "ushort")
		.Replace("Byte", "byte")
		.Replace("Char", "char")
		.Replace("ObservableCacheEx::", "")
		;
	
	private static string KeepLastPartOnly(this string s, string seps)
	{
		foreach (var ch in seps)
			s = s.KeepLastPartOnly(ch);
		return s;
	}
	private static string KeepLastPartOnly(this string s, char sep) => string.Join(sep, s.Split(sep).Select(e => e.KeepLastPartOnly()));
	private static string KeepLastPartOnly(this string s) => s.Split('.')[^1];
}










class LPLogger : ILogr
{
	private const string colTransition = "#c9d450";
	private const string colDom = "#0FFC7E";
	
	private readonly DumpContainer dc;
	
	public LPLogger()
	{
		dc = new DumpContainer();
		var div = new Div(dc);
		div.Styles["font-family"] = "Consolas";
		div.Styles["font-weight"] = "bold";
		div.Styles["font-size"] = "12px";
		div.Styles["background-color"] = "#000935";
		div.Styles["color"] = "#0FFC7E";
		div.Styles["padding"] = "5px";
		div.Dump();
	}
	
	public void Log(string msg) => dc.AppendContent(msg);
	
	public void LogTransition(string transition, string dom)
	{
		dc.AppendContent(new Div(new Span(" ")));
		WriteWithCol(transition, colTransition);
		WriteWithCol(dom.Truncate(64), colDom);
		ScrollToBottom();
	}
	
	private void WriteWithCol(string str, string col)
	{
		var div = new Div(new Span(str));
		div.Styles["color"] = col;
		dc.AppendContent(div);
	}
	
	public void CssError(string msg) => dc.AppendContent($"[CssError]: {msg}");
	
	private static void ScrollToBottom() => Util.InvokeScript(false, "eval", "window.scrollTo(0, 1000000)");
}





