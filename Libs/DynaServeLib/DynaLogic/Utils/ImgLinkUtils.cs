using System.Diagnostics.CodeAnalysis;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace DynaServeLib.DynaLogic.Utils;

static class ImgLinkUtils
{
	public static string MkImgLink(string name) => $"image/{name}";

	public static bool IsLocalImgNode(this IElement elt, [NotNullWhen(true)]out string? link)
	{
		link = null;
		var isLocal =
			elt is IHtmlImageElement eltImg &&
			IsLocalImgLink(eltImg.Source);
		if (isLocal)
			link = ((IHtmlImageElement)elt).Source!;
		return isLocal;
	}

	private static bool IsLocalImgLink(string? link)
	{
		bool IsRemote() =>
			link == null ||
			link.StartsWith("http://") ||
			link.StartsWith("https://") ||
			link.StartsWith("/");

		return !IsRemote();
	}
}