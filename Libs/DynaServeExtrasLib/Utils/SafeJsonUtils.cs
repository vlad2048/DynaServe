using PowMaybe;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DynaServeExtrasLib.Utils;

public static class SafeJsonUtils
{
	private static readonly JsonSerializerOptions jsonOpt = new()
	{
		WriteIndented = true
	};
	static SafeJsonUtils()
	{
		jsonOpt.Converters.Add(new MaybeSerializer<string>());
	}
	
	public static T Load<T>(string file, Func<T> makeEmptyFun, Func<T, bool>? integrityCheck = null)
	{
		Maybe<T> LoadInner()
		{
			if (!File.Exists(file))
			{
				L($"File missing '{file}'");
				return May.None<T>();
			}
			try
			{
				var str = File.ReadAllText(file);
				var obj = JsonSerializer.Deserialize<T>(str, jsonOpt);
				if (obj == null)
				{
					L("Deserializing to null");
					return May.None<T>();
				}
				if (integrityCheck != null && !integrityCheck(obj))
				{
					L("Data deserialized OK but failed the integrity test");
					return May.None<T>();
				}
				return May.Some(obj);
			}
			catch (Exception ex)
			{
				L($"Exception deserializing '{file}': {ex.Message}");
				return May.None<T>();
			}
		}
		
		if (LoadInner().IsNone())
		{
			Save(file, makeEmptyFun());
		}
		
		return LoadInner().Ensure();
	}
	
	public static void Save<T>(string file, T obj)
	{
		var str = JsonSerializer.Serialize(obj, jsonOpt);
		File.WriteAllText(file, str);
	}

	// ReSharper disable once UnusedParameter.Local
	private static void L(string s)
	{
		//Console.WriteLine(s);
	}
	
	
	
	public class MaybeSerializer<T> : JsonConverter<Maybe<T>> where T : class
	{
		public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var str = doc.Deserialize<T?>()!;
			return str.ToMaybe();
		}

		public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
		{
			var str = value.ToNullable();
			JsonSerializer.Serialize(writer, str);
		}
	}
}