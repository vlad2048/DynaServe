using DynaServeExtrasLib.Components.EditListLogic.Structs;
using DynaServeExtrasLib.Components.EditListLogic.StructsEnum;
using DynaServeLib.Nodes;
using System.Reactive;
using System.Reactive.Linq;

namespace DynaServeExtrasLib.Components.EditListLogic;

public class EditListOpt<T>
{
    public EditListSelectMode SelectMode { get; set; } = EditListSelectMode.Single;
    public int? Width { get; set; }
    public Func<ItemNfo<T>, HtmlNode>? ItemDispFun { get; set; }
    public IObservable<Unit>? ItemDispWhenChange { get; set; }
    public IObservable<bool> WhenCanAdd { get; set; } = Observable.Never<bool>().StartWith(true);

    private EditListOpt() { }
    internal static EditListOpt<T> Build(Action<EditListOpt<T>>? optFun)
    {
        var opt = new EditListOpt<T>();
        optFun?.Invoke(opt);
        return opt;
    }
}