using DynaServeLib.Logging;
using DynaServeLib.Serving.FileServing.Structs;
using DynaServeLib.Serving.Repliers;

namespace DynaServeLib;

public record ClientUserMsg(string UserType, string Arg);

public class ServOpt
{
	private ServOpt() {}

	internal List<IReplier> Repliers { get; } = new();
	internal List<Mount> Mounts { get; } = new();
	internal List<string> ExtraHtmlNodes { get; } = new();
	internal List<string> HtmlToAddToBody { get; } = new();

	public string? SearchFolder { get; set; }
	public void AddRepliers(params IReplier[] repliers) => Repliers.AddRange(repliers);
	public bool CheckSecurity { get; set; } = false;
	public int Port { get; set; } = 7000;
	public ILogr Logr { get; set; } = new NullLogr();
	public bool PlaceWebSocketsHtmlManually { get; set; } = true;
	public bool ShowDynaServLibVersion { get; set; } = true;
	public Action<ClientUserMsg> OnClientUserMsg { get; set; } = _ => { };
	public void AddHtmlToBody(string html) => HtmlToAddToBody.Add(html);

	internal static ServOpt Build(Action<ServOpt>? optFun)
	{
		var opt = new ServOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}