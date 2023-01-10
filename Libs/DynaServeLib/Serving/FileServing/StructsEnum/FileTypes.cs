namespace DynaServeLib.Serving.FileServing.StructsEnum;

public enum FCat
{
	Html,
	Css,
	Js,
	Image,
	Font,
	Manifest,
	Video,

	Ts,
}

enum FType
{
	// Html
	Html,

	// Css
	Css,
	Scss,

	// Js
	Js,

	// Image
	ImageSvg,
	ImagePng,
	ImageJpg,
	ImageIco,

	// Font
	FontWoff2,

	// Manifest
	Json,

	// Video
	VideoMP4,

	// Ts
	Ts
}


static class FileTypeExtSugar
{
	public static string ToLink(this string filename)
	{
		var mountFolder = Path.GetExtension(filename).Ext2Type().ToCat().ToMountFolder();
		var name = Path.GetFileName(filename);
		return (mountFolder != "") switch
		{
			true => $"{mountFolder}/{name}",
			false => name,
		};
	}

	public static FType ToType(this string filename) =>
		Path.GetExtension(filename).Ext2Type();

	public static FCat ToCat(this string filename) =>
		Path.GetExtension(filename).Ext2Type().ToCat();
}


static class FileTypeExt
{
	public static FType[] ToFTypes(this FCat cat) => cat switch
	{
		FCat.Html => new[] { FType.Html },
		FCat.Css => new[] { FType.Css, FType.Scss },
		FCat.Js => new[] { FType.Js },
		FCat.Image => new[] { FType.ImageSvg, FType.ImagePng, FType.ImageJpg, FType.ImageIco },
		FCat.Font => new[] { FType.FontWoff2 },
		FCat.Manifest => new[] { FType.Json },
		FCat.Video => new[] { FType.VideoMP4 },
		FCat.Ts => new[] { FType.Ts },
		_ => throw new ArgumentException()
	};

	public static string[] ToExts(this FType detail) => detail switch
	{
		FType.Html => new[] { ".html" },

		FType.Css => new[] { ".css" },
		FType.Scss => new[] { ".scss" },

		FType.Js => new[] { ".js" },

		FType.ImageSvg => new[] { ".svg" },
		FType.ImagePng => new[] { ".png" },
		FType.ImageJpg => new[] { ".jpg", ".jpeg" },
		FType.ImageIco => new[] { ".ico" },

		FType.FontWoff2 => new [] { ".woff2" },

		FType.Json => new[] { ".json" },

		FType.VideoMP4 => new[] { ".mp4" },

		FType.Ts => new[] { ".ts" },

		_ => throw new ArgumentException()
	};

	public static string ToMountFolder(this FCat cat) => cat switch
	{
		FCat.Html => "html",
		FCat.Css => "css",
		FCat.Js => "",
		FCat.Image => "images",
		FCat.Font => "fonts",
		FCat.Manifest => "",
		FCat.Video => "videos",
		FCat.Ts => throw new ArgumentException("Cannot mount .ts files"),
		_ => throw new ArgumentException($"Invalid Cat: {cat}")
	};

	public static bool NeedsLinking(this FCat cat) => cat switch
	{
		FCat.Css => true,
		FCat.Js => true,
		FCat.Manifest => true,
		_ => false
	};

	public static FType Ext2Type(this string fileExt)
	{
		fileExt = fileExt.ToLowerInvariant();
		return fileExt switch
		{
			".html" => FType.Html,

			".css" => FType.Css,
			".scss" => FType.Scss,

			".js" => FType.Js,

			".svg" => FType.ImageSvg,
			".png" => FType.ImagePng,
			".jpeg" or ".jpg" => FType.ImageJpg,
			".ico" => FType.ImageIco,

			".woff2" => FType.FontWoff2,

			".json" => FType.Json,

			".mp4" => FType.VideoMP4,

			".ts" => FType.Ts,

			_ => throw new ArgumentException($"Invalid file ext: {fileExt}"),
		};
	}


	public static FCat ToCat(this FType t) => t switch
	{
		FType.Html => FCat.Html,

		FType.Css => FCat.Css,
		FType.Scss => FCat.Css,

		FType.Js => FCat.Js,

		FType.ImageSvg => FCat.Image,
		FType.ImagePng => FCat.Image,
		FType.ImageJpg => FCat.Image,
		FType.ImageIco => FCat.Image,

		FType.FontWoff2 => FCat.Font,

		FType.Json => FCat.Manifest,

		FType.VideoMP4 => FCat.Video,

		FType.Ts => FCat.Ts,

		_ => throw new ArgumentException($"Invalid FType: {t}"),
	};


	public static string ToMime(this FType t) => t switch
	{
		FType.Html => "text/html",

		FType.Css => "text/css",
		FType.Scss => "text/css", // as this gets compiled to .css

		FType.Js => "text/javascript",

		FType.ImageSvg => "image/svg+xml",
		FType.ImagePng => "image/png",
		FType.ImageJpg => "image/jpeg",
		FType.ImageIco => "image/x-icon",

		FType.FontWoff2 => "font/woff2",
		
		FType.Json => "application/json",
		
		FType.VideoMP4 => "video/mp4",
		
		FType.Ts => throw new ArgumentException(".ts has no mime type"),

		_ => throw new ArgumentException($"Cannot find mime type for FType:{t}")
	};


	public static bool IsBinary(this FType t) => t switch
	{
		FType.Html => false,

		FType.Css => false,
		FType.Scss => false,

		FType.Js => false,

		FType.ImageSvg => false,
		FType.ImagePng => true,
		FType.ImageJpg => true,
		FType.ImageIco => true,

		FType.FontWoff2 => true,
		
		FType.Json => false,
		
		FType.VideoMP4 => true,
		
		_ => throw new ArgumentException($"Invalid FType: {t}")
	};
}