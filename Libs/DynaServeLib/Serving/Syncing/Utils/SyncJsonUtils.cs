using System.Text.Json.Serialization;
using System.Text.Json;

namespace DynaServeLib.Serving.Syncing.Utils;

static class SyncJsonUtils
{
	private static readonly JsonSerializerOptions jsonOpt = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
	static SyncJsonUtils() => jsonOpt.Converters.Add(new JsonStringEnumConverter());
	
	public static T Deser<T>(string str) => JsonSerializer.Deserialize<T>(str, jsonOpt)!;
	public static string Ser<T>(T obj) => JsonSerializer.Serialize(obj, jsonOpt);
}