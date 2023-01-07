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
	ChgType Type,
	ChgPropType? PropType,
	string? Name
)
{
	public override string ToString() => $"{Type}";
	public Chg Make(string nodePath, string? val) => new(nodePath, Type, PropType, Name, val);
}

public record Chg(
	string NodePath,
	ChgType Type,
	ChgPropType? PropType,
	string? Name,
	string? Val
)
{
	public override string ToString() => $"{Type} @ {NodePath}";
}


public static class ChgKeyMk
{
	public static ChgKey Text() => new(ChgType.Text, null, null);
	public static ChgKey Attr(string name) => new(ChgType.Attr, null, name);
	public static ChgKey PropStr(string name) => new(ChgType.Prop, ChgPropType.Str, name);
	public static ChgKey PropBool(string name) => new(ChgType.Prop, ChgPropType.Bool, name);
}
