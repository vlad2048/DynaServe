using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Logging;

public interface ILogr
{
	void OnSimpleMsg(string msg);
	void OnCssError(string msg);
	void OnLogEvt(LogEvt evt);
}

public class NullLogr : ILogr
{
	public void OnSimpleMsg(string msg) { }
	public void OnCssError(string msg) { }
	public void OnLogEvt(LogEvt evt) { }
}

public class ConsoleLogr : ILogr
{
	private const string Indent = "  ";

	public void OnSimpleMsg(string msg) => L(msg);

	public void OnCssError(string msg) => Console.Error.WriteLine(msg);

	public void OnLogEvt(LogEvt evt)
	{
		LD(evt.Message);
		LS("CssLinks");
		foreach (var link in evt.CssLinks)
			L(Indent + link);
		Print("DOM", evt.Dom);
		Print("FullLog", evt.FullLog);

		void Print(string header, string? val)
		{
			if (string.IsNullOrEmpty(val) || val == "_")
			{
				LS(Indent + $"{header}: _");
			}
			else
			{
				LS(Indent + header);
				L(val.AddPrefixToLines(Indent + Indent));
			}
		}
	}

	private static void L(string s) => Console.WriteLine(s);

	private static void LD(string s)
	{
		L(s);
		L(new string('=', s.Length));
	}

	private static void LS(string s)
	{
		L(Indent + s);
		L(Indent + new string('=', s.Length));
	}
}