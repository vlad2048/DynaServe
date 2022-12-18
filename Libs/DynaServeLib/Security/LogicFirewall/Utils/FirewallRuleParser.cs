using DynaServeLib.Security.LogicFirewall.Structs;

namespace DynaServeLib.Security.LogicFirewall.Utils;

static class FirewallRuleParser
{
	public static FirewallRule[] ParseFirewallRules(this IEnumerable<string> linesSource)
	{
		var lines = linesSource.ToArray();
		return lines
			.GetLineIndices(e => e.StartsWith("-----"))
			.Select(i => lines.Skip(i - 1).TakeWhile(e => e.Trim() != string.Empty).ToArray())
			.Select(rs => new FirewallRule(
				rs.Read("Enabled").ToBool(),
				rs.Read("Rule Name"),
				rs.Read("Direction").ToDirection(),
				rs.Read("Profiles").ToProfiles(),
				rs.Read("Protocol"), //.ToProtocol(),
				rs.Read("LocalIP"),
				rs.Read("RemoteIP"),
				rs.ReadOpt("LocalPort"),
				rs.ReadOpt("RemotePort"),
				rs.Read("Action")
			))
			.ToArray();
	}

	private static int[] GetLineIndices(this string[] lines, Func<string, bool> predicate)
	{
		var list = new List<int>();
		for (var i = 0; i < lines.Length; i++)
			if (predicate(lines[i]))
				list.Add(i);
		return list.ToArray();
	}

	private static string Read(this string[] ruleLines, string header) =>
		ruleLines
			.Single(e => e.StartsWith($"{header}:"))
			.ExtractValue();

	private static string ReadOpt(this string[] ruleLines, string header)
	{
		var line = ruleLines.FirstOrDefault(e => e.StartsWith($"{header}:"));
		if (line == null) return string.Empty;
		return line.ExtractValue();
	}

	private static string ExtractValue(this string line)
	{
		var idx = line.IndexOf(':');
		var str = line[(idx + 1)..].Trim();
		return str;
	}

	private static bool ToBool(this string s) => s switch
	{
		"Yes" => true,
		"No" => false,
		_ => throw new ArgumentException()
	};

	private static FirewallDirection ToDirection(this string s) => s switch
	{
		"In" => FirewallDirection.In,
		"Out" => FirewallDirection.Out,
		_ => throw new ArgumentException()
	};

	private static FirewallProfile ToProfiles(this string s)
	{
		var parts = s.Split(',');
		FirewallProfile val = 0;
		foreach (var part in parts)
		{
			var partVal = part switch
			{
				"Private" => FirewallProfile.Private,
				"Domain" => FirewallProfile.Domain,
				"Public" => FirewallProfile.Public,
				_ => throw new ArgumentException()
			};
			val |= partVal;
		}
		return val;
	}
}