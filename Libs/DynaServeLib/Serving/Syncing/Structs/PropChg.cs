using System.Reactive.Linq;

namespace DynaServeLib.Serving.Syncing.Structs;

public enum ChgType
{
	Text,
	Attr,
	Prop
}
public enum ChgPropType
{
	Str,
	Bool
}

public record ChgKey(
	string NodeId,
	ChgType Type,
	ChgPropType? PropType,
	string? Name
);

public record Chg(
	string NodeId,
	ChgType Type,
	ChgPropType? PropType,
	string? Name,
	string? Val
);

public static class ChgKeyMk
{
	public static ChgKey Text(string nodeId) => new(nodeId, ChgType.Text, null, null);
	public static ChgKey Attr(string nodeId, string name) => new(nodeId, ChgType.Attr, null, name);
	public static ChgKey PropStr(string nodeId, string name) => new(nodeId, ChgType.Prop, ChgPropType.Str, name);
	public static ChgKey PropBool(string nodeId, string name) => new(nodeId, ChgType.Prop, ChgPropType.Bool, name);
}

public static class ChgMk
{
	public static Chg Text(string nodeId, string? text) => new(nodeId, ChgType.Text, null, null, text);
	public static Chg Attr(string nodeId, string name, string? val) => new(nodeId, ChgType.Attr, null, name, val);
	public static Chg PropStr(string nodeId, string name, string? val) => new(nodeId, ChgType.Prop, ChgPropType.Str, name, val);
	public static Chg PropBool(string nodeId, string name, bool val) => new(nodeId, ChgType.Prop, ChgPropType.Bool, name, $"{val}");
}
