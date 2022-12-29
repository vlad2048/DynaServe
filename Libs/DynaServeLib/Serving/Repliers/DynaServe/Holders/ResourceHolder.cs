using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DynaServeLib.Serving.Structs;

namespace DynaServeLib.Serving.Repliers.DynaServe.Holders;

class ResourceHolder
{
    private readonly ConcurrentDictionary<string, Reply> resourceMap = new();

    public void AddContent(string link, Reply resource) => resourceMap[link] = resource;
    public bool TryGetContent(string link, [NotNullWhen(true)] out Reply? resource) => resourceMap.TryGetValue(link, out resource);
    public void RemoveLink(string link) => resourceMap.Remove(link, out _);

    public void AddFolders(IEnumerable<string> folders, string pattern, string mountedFolder, ReplyType replyType)
    {
	    foreach (var folder in folders)
	    {
		    var files = Directory.GetFiles(folder, pattern);
		    foreach (var file in files)
		    {
			    var reply = Reply.Mk(replyType, File.ReadAllBytes(file));
			    var name = Path.GetFileName(file);
			    var link = $"{mountedFolder}/{name}";
			    AddContent(link, reply);
		    }
	    }
    }
}