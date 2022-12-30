using DynaServeExtrasLib.Components.EditListLogic.Structs;
using DynaServeExtrasLib.Components.EditListLogic.StructsEnum;

namespace DynaServeExtrasLib.Components.EditListLogic.Utils;

public static class EditListCls
{
    public static string DefaultItemCls<T>(ItemNfo<T> Nfo) => Nfo.SelStatus switch
    {
        ItemSelStatus.None => "editlist-item",
        ItemSelStatus.Sel => "editlist-item editlist-item-selmulti",
        ItemSelStatus.SelLast => "editlist-item editlist-item-selmulti editlist-item-selsingle",
        _ => throw new ArgumentException()
    };
}