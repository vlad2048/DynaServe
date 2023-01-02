namespace DynaServeLib.Logging;

public interface ILogr
{
	void OnSimpleMsg(string msg);
	void OnCssError(string msg);
}

public class NullLogr : ILogr
{
	public void OnSimpleMsg(string msg) { }
	public void OnCssError(string msg) { }
}

public class ConsoleLogr : ILogr
{
	public void OnSimpleMsg(string msg) => L(msg);

	public void OnCssError(string msg) => Console.Error.WriteLine(msg);

	private static void L(string s) => Console.WriteLine(s);
}