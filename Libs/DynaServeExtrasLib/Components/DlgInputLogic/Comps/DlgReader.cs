namespace DynaServeExtrasLib.Components.DlgInputLogic.Comps;

public interface IDlgReader
{
    IReadOnlyDictionary<string, object> GetMap();
    string GetString(string key);
    string[] GetMultipleChoices(string key);
}

public static class DlgReaderExt
{
    public static bool HasNonEmptyString(this IDlgReader reader, string key) => reader.GetMap().TryGetValue(key, out var val) switch
    {
        false => false,
        true => val switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            _ => false
        }
    };
}

class DlgReader : IDlgReader
{
    private readonly ValMap valMap;

    public DlgReader(ValMap valMap) => this.valMap = valMap;

    public IReadOnlyDictionary<string, object> GetMap() => valMap.Map;
    public string GetString(string key) => (string)valMap.Map[key];
    public string[] GetMultipleChoices(string key) => (string[])valMap.Map[key];
}
