using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.Logging;
using DynaServeLib.Utils.Exts;
using PowTrees.Algorithms;

namespace DynaServeLib.DynaLogic.Utils;

static class DomLogUtils
{
	private record DomNode(
		string Name,
		string? Id
	)
	{
		public override string ToString() => $"<{Name}{IdStr}>";

		private string IdStr => Id switch
		{
			null => "",
			not null => $"#{Id}"
		};
	}

	/*public static void LogDom(string msg, IHtmlDocument doc, Action<string> logFun)
	{
		var serverTree = Conv(doc);

		void L(string s) => logFun(s);
		void LTitle(string s)
		{
			L(s);
			L(new string('=', s.Length));
		}

		void LTree(string title, TNod<DomNode> root)
		{
			LTitle(title);
			L(root.LogToString());
			L("");
		}

		L("");
		LTree($"DOM ({msg})", serverTree);
	}*/

	public static void LogFull(string clientHtml, IHtmlDocument doc, string refresherIds, ILogr logr)
	{
		var clientTree = Conv(clientHtml.Parse());
		var serverTree = Conv(doc);

		var sb = new StringBuilder();
		void L(string s) => sb.AppendLine(s);
		void LTitle(string s)
		{
			L(s);
			L(new string('=', s.Length));
		}
		void LTree(string title, TNod<DomNode> root)
		{
			LTitle(title);
			L(root.LogToString());
			L("");
		}

		L("");
		LTree("Server DOM", serverTree);
		LTree("Client DOM", clientTree);
		L($"Refreshers on: {refresherIds}");
		L("");
		logr.OnLogEvt(new LogEvt(
			"Full Log",
			Array.Empty<string>(),
			"_",
			sb.ToString()
		));
	}


	private static TNod<DomNode> Conv(IHtmlDocument doc)
	{
		var body = doc.FindDescendant<IHtmlBodyElement>()!;
		TNod<DomNode> Recurse(IElement n) => Nod.Make(
			new DomNode(n.NodeName, n.Id),
			n.Children.Select(Recurse)
		);
		return Recurse(body);
	}
}