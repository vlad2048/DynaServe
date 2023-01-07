using System.Reactive.Linq;
using DynaServeLib.Serving.Syncing.Structs;
using PowMaybe;

namespace DynaServeLib.Nodes;

public class Ref
{
	public Maybe<string> Id { get; set; } = May.None<string>();

	public void Hook(HtmlNode node)
	{
		Id = May.Some(node.Id);
		node.WhenDisposed.Take(1).Subscribe(_ => Id = May.None<string>());
	}

	public void CallMethod(string methodName)
	{
		if (Id.IsSome(out var id))
			Serv.I?.Messenger.SendToClient(new ReqCallMethodOnNodeServerMsg(id, methodName));
	}
}