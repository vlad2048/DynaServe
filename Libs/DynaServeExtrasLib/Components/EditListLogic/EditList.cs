using DynaServeLib.Nodes;
using PowMaybe;
using PowRxVar;
using System.Reactive.Linq;
using System.Reactive;
using DynaServeExtrasLib.Utils;
using DynaServeExtrasLib.Components.EditListLogic.Structs;
using DynaServeExtrasLib.Components.EditListLogic.StructsEnum;
using DynaServeExtrasLib.Components.EditListLogic.Utils;
using PowBasics.CollectionsExt;

namespace DynaServeExtrasLib.Components.EditListLogic;

public class EditList<T> : IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private static readonly Func<ItemNfo<T>, HtmlNode> defaultItemDispFun = nfo =>
        Div(EditListCls.DefaultItemCls(nfo)).Txt($"{nfo.Item}");

    private readonly IRwVar<T[]> list;
    private readonly EditListOpt<T> opt;
    private readonly IRwVar<Maybe<T>> selItem;
    private readonly IRwVar<T[]> selItems;
    private IRoVar<int> SelItemIndex { get; }

    public HtmlNode UI { get; }
    public IRoVar<Maybe<T>> SelItem => selItem.ToReadOnly();
    public IRoVar<T[]> SelItems => selItems.ToReadOnly();
    public IObservable<Unit> WhenChanged => Observable.Merge(
        list.ToUnit(),
        SelItem.ToUnit(),
        SelItems.ToUnit(),
        opt.ItemDispWhenChange ?? Observable.Never<Unit>()
    ).Throttle(TimeSpan.Zero).Prepend(Unit.Default);

    /*private static void L(string s)
    {
	    if (typeof(T).Name != "RecSlave") return;
	    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} - {Thread.CurrentThread.Name}] {s}");
    }*/

    public EditList(
        IRwVar<T[]> list,
        string title,
        Func<Maybe<T>, Task<Maybe<T>>> createFun,
        Action<EditListOpt<T>>? optFun = null
    )
    {
        this.list = list;
        opt = EditListOpt<T>.Build(optFun);
        var itemDispFun = opt.ItemDispFun ?? defaultItemDispFun;
        selItem = Var.Make(May.None<T>()).D(d);
        selItems = Var.Make(Array.Empty<T>()).D(d);
        // TODO: not sure why this line fails !
        //SelItemIndex = Var.Expr(() => ComputeSelItemIndex(SelItem.V));
        SelItemIndex = Var.Expr(() => ComputeSelItemIndex(selItem.V), list.ToUnit());

        Constraint_MakeSelNone_When_RemovedFromArr().D(d);

        //list.Where(e => e.Length == 0).Subscribe(_ => L("List <- []")).D(d);

        UI =
            Div("editlist").SetWidthOpt(opt.Width).Wrap(

                Div("editlist-titlelist").OnClick(OnNoItemClicked).Wrap(

                    TH3().Txt(title),

                    Div("editlist-list").Wrap(
                        WhenChanged,
                        () => list.V
                            .Select(ComputeItemDispNfo)
                            .Select(nfo =>
                                itemDispFun(nfo)
                                    .OnClick(() =>
                                    {
                                        OnItemClicked(nfo.Item);
                                    }, true)
                            )
                    )
                ),


                Div("editlist-btnrow").Wrap(

                        IconBtn("fa-solid fa-up-long", () =>
                        {
                            MoveSelItemUp();
                        }).EnableWhen(SelItemIndex.Select(idx => idx != -1 && idx > 0)),

                        IconBtn("fa-solid fa-down-long", () =>
                        {
                            MoveSelItemDown();
                        }).EnableWhen(SelItemIndex.Select(idx => idx != -1 && idx < list.V.Length - 1)),

                        IconBtn("fa-solid fa-pen-to-square", async () =>
                        {
                            if ((await createFun(May.Some(SelItem.V.Ensure()))).IsNone(out var item)) return;
                            ReplaceSelItem(item);
                        }).EnableWhen(SelItem.Select(e => e.IsSome())),

                        IconBtn("fa-solid fa-plus", async () =>
                        {
                            if ((await createFun(May.None<T>())).IsNone(out var item)) return;
                            AddItem(item);
                        }).EnableWhen(opt.WhenCanAdd),

                        IconBtn("fa-solid fa-trash", () =>
                        {
                            DelSelItem();
                        }).EnableWhen(SelItem.Select(e => e.IsSome()))
                )
            );
    }

	private IDisposable Constraint_MakeSelNone_When_RemovedFromArr() =>
		list
			.Subscribe(arr =>
			{
				if (SelItem.V.IsSomeAndVerifies(e => !arr.Contains(e)))
					selItem.V = May.None<T>();
				if (SelItems.V.Any(e => !arr.Contains(e)))
					selItems.V = selItems.V.WhereToArray(arr.Contains);
			});


    public void TransformSelItemEnsure(Func<T, T> transformFun) => ReplaceSelItem(transformFun(SelItem.V.Ensure()));



    private void OnNoItemClicked()
    {
        selItem.V = May.None<T>();
        selItems.V = Array.Empty<T>();
    }

    private void OnItemClicked(T item)
    {
        switch (opt.SelectMode)
        {
            case EditListSelectMode.Single:
                selItem.V = May.Some(item);
                selItems.V = new[] { item };
                break;
            case EditListSelectMode.Multiple:
                var isMultiSel = selItems.V.Contains(item);
                if (isMultiSel)
                {
                    selItem.V = May.None<T>();
                    selItems.V = selItems.V.ArrDel(item);
                }
                else
                {
                    selItem.V = May.Some(item);
                    selItems.V = selItems.V.ArrAdd(item);
                }
                break;
            default:
                throw new ArgumentException();
        }
    }

    private void MoveSelItemUp()
    {
        var idx = SelItemIndex.V;
        if (idx == -1 || idx <= 0) throw new ArgumentException($"Err in MoveSelItemUp idx={idx} (list.length={list.V.Length})");
        var elt = SelItem.V.Ensure();
        var l = list.V.ToList();

        l.RemoveAt(idx);
        l.Insert(idx - 1, elt);
		//L($"MoveUp Before ({list.V.Length} <- {l.Count})");
        list.V = l.ToArray();
        //L($"MoveUp After ({list.V.Length} <- {l.Count})");

        selItems.V = new[] { elt };
    }

    private void MoveSelItemDown()
    {
        var idx = SelItemIndex.V;
        if (idx == -1 || idx >= list.V.Length - 1) throw new ArgumentException($"Err in MoveSelItemDown idx={idx} list.length={list.V.Length}");
        var elt = SelItem.V.Ensure();
        var l = list.V.ToList();

        l.RemoveAt(idx);
        l.Insert(idx + 1, elt);
        list.V = l.ToArray();

        selItems.V = new[] { elt };
    }
    private void ReplaceSelItem(T itemNext)
    {
        var itemPrev = SelItem.V.Ensure();
		// TODO: updating the list BEFORE the selItem&selItems breaks master/slave linking (slaveList.SelItem becomes None for no reason when moving a Slave up). Not sure why. But it works like this
        selItem.V = May.Some(itemNext);
        selItems.V = new[] { itemNext };
        list.V = list.V.ArrRepl(itemPrev, itemNext);
    }

    private void AddItem(T item)
    {
        list.V = list.V.ArrAdd(item);
        selItem.V = May.Some(item);
        selItems.V = new[] { item };
    }

    private void DelSelItem()
    {
        var item = SelItem.V.Ensure();
        list.V = list.V.ArrDel(item);
        selItem.V = May.None<T>();
        var isMultiSel = selItems.V.Contains(item);
        if (isMultiSel)
            selItems.V = selItems.V.ArrDel(item);
    }

    private int ComputeSelItemIndex(Maybe<T> selItemMayVal) => selItemMayVal.IsSome(out var selItemVal) switch
    {
        true => list.V.ToList().IndexOf(selItemVal!),
        false => -1
    };

    private ItemNfo<T> ComputeItemDispNfo(T item)
    {
        var selStatus = SelItems.V.Contains(item) switch
        {
            false => ItemSelStatus.None,
            true => SelItem.V.IsSomeAndEqualTo(item) switch
            {
                false => ItemSelStatus.Sel,
                true => ItemSelStatus.SelLast,
            }
        };
        return new ItemNfo<T>(item, selStatus);
    }
}