namespace DynaServeLib.Security.LogicFirewall.Structs;

enum FirewallDirection
{
	In,
	Out
}

[Flags] enum FirewallProfile
{
	Private = 1,
	Domain = 2,
	Public = 4
}

[Flags] enum FirewallProtocol
{
	Tcp = 1,
	Udp = 2
}

record FirewallRule(
	bool Enabled,
	string Name,
	FirewallDirection Direction,
	FirewallProfile Profile,
	string Protocol,
	string LocalIP,
	string RemoteIP,
	string LocalPort,
	string RemotePort,
	string Action
)
{
	public bool IsForPort(int port) =>
		Enabled &&
		IsTcp() &&
		IsPublic() &&
		DoesContainPort(port);

	private bool DoesContainPort(int port)
	{
		if (LocalPort == string.Empty || LocalPort.Any(char.IsAsciiLetter)) return false;
		var parts = LocalPort.Split(',');
		return parts.Any(part => DoesPortRangeContain(part, port));
	}

	private bool IsTcp() => Protocol == "TCP" || Protocol == "Any";

	private bool IsPublic() => Profile.HasFlag(FirewallProfile.Public);

	private static bool DoesPortRangeContain(string s, int port)
	{
		if (s.Contains('-'))
		{
			var parts = s.Split('-');
			var portMin = int.Parse(parts[0]);
			var portMax = int.Parse(parts[1]);
			return port >= portMin && port <= portMax;
		}
		else
		{
			var portVal = int.Parse(s);
			return port == portVal;
		}
	}
}