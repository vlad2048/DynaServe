using DynaServeExtrasLib.Components.DlgInputLogic.Edits;
using DynaServeExtrasLib.Components.DlgInputLogic.Edits.Base;
using PowMaybe;
using PowRxVar;

namespace DynaServeExtrasLib.Components.DlgInputLogic.Comps;

public interface IDlgSetup
{
    Func<IDlgReader, bool>? ValidFun { get; set; }
    void EditString(string key, string label, string prevVal);
    void EditSingleChoice(string key, string label, Maybe<string> prevVal, string[] choices);
    void EditMultipleChoices(string key, string label, string[] prevVal, string[] choices);
}

class DlgSetup : IDlgSetup, IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly List<IEdit> edits = new();

    public IReadOnlyList<IEdit> Edits => edits.AsReadOnly();

    public Func<IDlgReader, bool>? ValidFun { get; set; }
    public void EditString(string key, string label, string prevVal) => AddEdit(new StringEdit(key, label, prevVal));
    public void EditSingleChoice(string key, string label, Maybe<string> prevVal, string[] choices) => AddEdit(new SingleChoiceEdit(key, label, prevVal, choices));
    public void EditMultipleChoices(string key, string label, string[] prevVal, string[] choices) => AddEdit(new MultipleChoicesEdit(key, label, prevVal, choices));

    private void AddEdit(IEdit edit)
    {
        if (Edits.Any(e => e.Key == edit.Key)) throw new ArgumentException($"Same key added multiple times: '{edit.Key}'");
        edits.Add(edit.D(d));
    }
}