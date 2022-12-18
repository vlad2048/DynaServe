using DynaServeLib.Security.Utils;

namespace DynaServeLib.Security.LogicUrlAcl;

static class UrlAclOps
{
	public static string MkUrl(int port) => $"http://*:{port}/";

	public static string GetCmdToOpenPort(int port) =>
		$"""netsh http add urlacl url={MkUrl(port)} user=Everyone""";

	public static string GetCmdToClosePort(int port) =>
		$"""netsh http delete urlacl url={MkUrl(port)}""";

	public static bool IsPortOpen(int port) =>
		Cmder.Run("netsh", "http", "show", "urlacl", $"url={MkUrl(port)}")
			.Any(e => e.Contains(MkUrl(port)));
}