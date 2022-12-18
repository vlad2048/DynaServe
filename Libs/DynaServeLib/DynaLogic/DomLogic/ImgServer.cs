using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DynaServeLib.DynaLogic.Utils;
using DynaServeLib.Logging;
using DynaServeLib.Serving.Repliers.DynaServe.Holders;
using DynaServeLib.Serving.Structs;

namespace DynaServeLib.DynaLogic.DomLogic;

interface IDomTweaker
{
	void TweakNode(IElement elt);
}



class ImgDomTweaker : IDomTweaker
{
	private readonly IReadOnlyList<string> imgFolders;
	private readonly ResourceHolder resourceHolder;
	private readonly ILogr logr;
	private readonly HashSet<string> namesWarnedAbout = new();
	private readonly Dictionary<string, string> imgMap = new();

	public ImgDomTweaker(IReadOnlyList<string> imgFolders, ResourceHolder resourceHolder, ILogr logr)
	{
		this.imgFolders = imgFolders;
		this.resourceHolder = resourceHolder;
		this.logr = logr;
	}

	public void TweakNode(IElement elt)
	{
		if (!elt.IsLocalImgNode(out var link)) return;
		var name = Path.GetFileName(link);
		if (!imgMap.TryGetValue(name, out var finalLink))
		{
			var data = ReadImage(name);
			if (data == null) return;
			finalLink = ImgLinkUtils.MkImgLink(name);
			resourceHolder.AddContent(finalLink, ReplyExt.MakeReplyFromImage(name, data));
			imgMap[name] = finalLink;
		}
		((IHtmlImageElement)elt).Source = finalLink;
	}

	private byte[]? ReadImage(string name)
	{
		foreach (var imgFolder in imgFolders)
		{
			var fullName = Path.Combine(imgFolder, name);
			if (File.Exists(fullName))
				return File.ReadAllBytes(fullName);
		}

		if (!namesWarnedAbout.Contains(name))
		{
			namesWarnedAbout.Add(name);
			logr.OnCssError($"Failed to find image named: '{name}'");
		}

		return null;
	}
}