using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;
using PowRxVar;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Comps;

class ValMap : IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly IReadOnlyList<IEdit> edits;
    private readonly Dictionary<string, object> map = new();
    private readonly ISubject<Unit> whenChanged;

    public IReadOnlyDictionary<string, object> Map => map.AsReadOnly();
    public IObservable<Unit> WhenChanged => whenChanged.AsObservable().Prepend(Unit.Default);

    public ValMap(IReadOnlyList<IEdit> edits)
    {
        this.edits = edits;
        whenChanged = new Subject<Unit>().D(d);
    }
    public void SetVal(string key, object val)
    {
        var actType = val.GetType();
        var expType = edits.Single(e => e.Key == key).ValType;
        if (actType != expType) throw new ArgumentException($"An IEdit value was set with the wrong type: {actType} (expected: {expType})");
        map[key] = val;
        whenChanged.OnNext(Unit.Default);
    }

    public bool IsFinished()
    {
        var actKeys = Map.Keys.OrderBy(e => e).ToArray();
        var expKeys = edits.Select(e => e.Key).OrderBy(e => e).ToArray();
        return
            actKeys.Length == expKeys.Length &&
            actKeys.Zip(expKeys).All(t => t.First == t.Second);
    }
}