using System.Text.Json;
using PowMaybe;

namespace DynaServeLib.Gizmos;

static class DynaServVerDisplayer
{
	private record VerTime(
		int Ver,
		DateTime Time
	);

	public static int GetVer()
	{
		var nfoPrev = JsonUtils.Load(VerFile, MkNewVerTime);
		var nfoNext = MkNewVerTime();
		if (nfoNext.Time > nfoPrev.Time)
		{
			var nfoSave = nfoNext with { Ver = nfoPrev.Ver + 1 };
			JsonUtils.Save(VerFile, nfoSave);
			return nfoSave.Ver;
		}
		else
		{
			return nfoPrev.Ver;
		}
	}

	private static VerTime MkNewVerTime() => new(1, GetDllTime());
	private static DateTime GetDllTime() =>
		new FileInfo(typeof(DynaServVerDisplayer).Assembly.Location).LastWriteTime;

	private static string VerFile => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
		"DynaServ",
		"VerTime.json"
	).MakeFileFolderIFN();

	private static string MakeFileFolderIFN(this string file)
	{
		var folder = Path.GetDirectoryName(file)!;
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		return file;
	}



	private static class JsonUtils
	{
		private static readonly JsonSerializerOptions jsonOpt = new()
		{
			WriteIndented = true
		};
	
		public static T Load<T>(string file, Func<T> makeEmptyFun, Func<T, bool>? integrityCheck = null)
		{
			Maybe<T> LoadInner()
			{
				if (!File.Exists(file))
				{
					//$"File missing '{file}'";
					return May.None<T>();
				}
				try
				{
					var str = File.ReadAllText(file);
					var obj = JsonSerializer.Deserialize<T>(str, jsonOpt);
					if (obj == null)
					{
						//"Deserializing to null";
						return May.None<T>();
					}
					if (integrityCheck != null && !integrityCheck(obj))
					{
						//"Data deserialized OK but failed the integrity test";
						return May.None<T>();
					}
					return May.Some(obj);
				}
				catch (Exception ex)
				{
					//$"Exception deserializing '{file}': {ex.Message}";
					return May.None<T>();
				}
			}
		
			if (LoadInner().IsNone(out var obj))
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
	}
}