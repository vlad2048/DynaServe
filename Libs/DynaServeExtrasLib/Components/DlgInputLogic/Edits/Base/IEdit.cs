using DynaServeLib.Nodes;
using PowRxVar;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;

interface IEdit : IDisposable
{
    string Key { get; }
    string Label { get; }
    Type ValType { get; }
    HtmlNode MakeUI(Action<object> setFun, bool isFirst);
}

abstract record EditBase(
    string Key,
    string Label,
    Type ValType
) : IEdit
{
    protected Disp D = new();
    public void Dispose() => D.Dispose();

    public abstract HtmlNode MakeUI(Action<object> setFun, bool isFirst);
}