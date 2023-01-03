namespace DynaServeLib.Logging;

public interface ILogr
{
	void Log(string msg);
	void LogTransition(string transition, string dom);
	void CssError(string msg);
}

public class NullLogr : ILogr
{
	public void Log(string msg) { }
	public void LogTransition(string transition, string dom) { }
	public void CssError(string msg) { }
}

public class ConsoleLogr : ILogr
{
	public void Log(string msg) => L(msg);

	public void LogTransition(string transition, string dom)
	{
		Console.WriteLine(transition);
		Console.WriteLine(dom);
	}

	public void CssError(string msg) => Console.Error.WriteLine(msg);

	private static void L(string s) => Console.WriteLine(s);
}