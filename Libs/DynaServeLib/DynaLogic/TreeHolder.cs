/*
using DynaServeLib.DynaLogic.Events;
using DynaServeLib.Nodes;
using PowRxVar;

namespace DynaServeLib.DynaLogic;

class TreeHolder : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly List<HtmlNode> nodes = new();
	private readonly HashSet<HtmlNode> nodeSet = new();

	public TreeHolder(ITreeEvtObs treeEvtObs)
	{
		treeEvtObs.WhenEvt.Subscribe(evt =>
		{
			switch (evt)
			{
				case AddNodeToBodyTreeEvt e:
				{
					nodes.Add(e.Node);
					nodeSet.AddRange(e.Node.GetAllDescendents());
					break;
				}
				case ReplaceNodeChildrenTreeEvt e:
				{
					if (nodeSet.Contains(e.Parent))
						foreach (var child in e.Parent.Children)
							nodeSet.RemoveRange(child.GetAllDescendents());

					e.Parent.Children = e.Children;

					if (nodeSet.Contains(e.Parent))
						foreach (var child in e.Parent.Children)
							nodeSet.AddRange(child.GetAllDescendents());
					break;
				}
				case ChangeNodeProp e:
				{
					switch (e.Prop)
					{
						case NodeProp.Cls:
							e.Node.Cls = e.Val;
							break;
						case NodeProp.Text:
							e.Node.Text = e.Val;
							break;
						default:
							throw new ArgumentException();
					}

					break;
				}
				default:
					throw new ArgumentException();
			}
		}).D(d);
	}
}

file static class HashSetExt
{
	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> source)
	{
		foreach (var elt in source)
			set.Add(elt);
	}
	public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> source)
	{
		foreach (var elt in source)
			set.Remove(elt);
	}
}
*/