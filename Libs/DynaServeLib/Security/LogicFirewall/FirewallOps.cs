using DynaServeLib.Security.LogicFirewall.Utils;
using DynaServeLib.Security.Utils;

namespace DynaServeLib.Security.LogicFirewall;

static class FirewallOps
{
	public static string GetCmdToOpenPort(int port) =>
		$"""netsh advfirewall firewall add rule name="__SyncServLib_{port}" dir=in action=allow protocol=TCP localport={port} profile=Public""";

	public static string GetCmdToClosePort(int port) =>
		$"""netsh advfirewall firewall delete rule name="__SyncServLib_{port}" """;

	public static bool IsPortOpen(int port) =>
		Cmder.Run("netsh", "advfirewall", "firewall", "show", "rule", "name=all", "dir=in", "type=static")
			.ParseFirewallRules()
			.Any(rule => rule.IsForPort(port));
}