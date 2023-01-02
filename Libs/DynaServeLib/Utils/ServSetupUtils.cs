using System.Reactive.Linq;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Syncing.Structs;

namespace DynaServeLib.Utils;

static class ServSetupUtils
{
	public static IDisposable HookClientUserMessages(Messenger messenger, ServOpt opt) =>
		messenger.WhenClientMsg
			.Where(msg => msg.Type == ClientMsgType.User)
			.Subscribe(msg => opt.OnClientUserMsg(new ClientUserMsg(msg.UserType!, msg.UserArg!)));
}