using DynaServeLib.Security.LogicFirewall;
using DynaServeLib.Security.LogicUrlAcl;

namespace DynaServeLib.Security;

public static class SecurityChecker
{
	public static bool CheckPort(bool checkSecurity, int port, bool enableLog = false)
	{
		if (!checkSecurity) return true;

		var isValid = true;

		void L(string s)
		{
			if (!enableLog) return;
			Console.WriteLine(s);
		}

		var title = $"Check port {port}";
		L(title);
		L(new string('-', title.Length));
		if (FirewallOps.IsPortOpen(port))
		{
			L("  Firewall -> Open");
		}
		else
		{
			isValid = false;
			LErr("  Firewall -> Closed");
			LErr($"    {FirewallOps.GetCmdToOpenPort(port)}");
			LErr($"    {FirewallOps.GetCmdToClosePort(port)}");
		}
	
		if (UrlAclOps.IsPortOpen(port))
		{
			L("  UrlAcl   -> Open");
		}
		else
		{
			isValid = false;
			LErr("  UrlAcl   -> Closed");
			LErr($"    {UrlAclOps.GetCmdToOpenPort(port)}");
			LErr($"    {UrlAclOps.GetCmdToClosePort(port)}");
		}
		L("");

		return isValid;
	}
	
	private static void LErr(string s)
	{
		Console.WriteLine(s);
	}
}