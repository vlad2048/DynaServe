using DynaServeLib.Nodes;

namespace DynaServeExtrasLib.Utils;

public static class ClsExt
{
	public static string AddCls(this string cls, string clsExtra) => $"{cls} {clsExtra}";

	public static string AddClsIf(this string cls, string clsExtra, bool cond) => cond switch
	{
		false => cls,
		true => cls.AddCls(clsExtra)
	};
	
	public static HtmlNode ClsOnIf(this HtmlNode node, string cls, bool cond) => cond switch
	{
		false => node.Cls(cls),
		true => node.Cls($"{cls} {cls}-on")
	};
}