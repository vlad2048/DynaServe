using System.Reactive.Linq;
using DynaServeLib.Serving;
using DynaServeLib.Serving.Syncing.Structs;
using DynaServeLib.Utils.Exts;

namespace DynaServeLib.Utils;

static class ServSetupUtils
{
	public static IDisposable HookClientUserMessages(Messenger messenger, ServOpt opt) =>
		messenger.WhenClientMsg
			.OfType<UserClientMsg>()
			.SubscribeSafe(e => opt.OnClientUserMsg(new ClientUserMsg(e.UserType, e.Arg)));
}