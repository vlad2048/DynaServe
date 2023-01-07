using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynaServeLib.Serving.Syncing.Structs;


// *******************
// * Client Messages *
// *******************

enum ClientMsgType
{
	ReqScriptsSync,
	HookCalled,
	HookArgCalled,
	AnswerDomSnapshot,
	User
}

// @formatter:off
interface IClientMsg { ClientMsgType Type { get; } }

record ReqScriptsSyncClientMsg		(string[] CssLinks, string[] JsLinks)		: IClientMsg { public ClientMsgType Type => ClientMsgType.ReqScriptsSync;		}
record HookCalledClientMsg			(string Id, string EvtName)					: IClientMsg { public ClientMsgType Type => ClientMsgType.HookCalled;			}
record HookArgCalledClientMsg		(string Id, string EvtName, string EvtArg)	: IClientMsg { public ClientMsgType Type => ClientMsgType.HookArgCalled;		}
record AnswerDomSnapshotClientMsg	(string Head, string Body)					: IClientMsg { public ClientMsgType Type => ClientMsgType.AnswerDomSnapshot;	}
record UserClientMsg				(string UserType, string Arg)				: IClientMsg { public ClientMsgType Type => ClientMsgType.User;					}
// @formatter:on

static class SyncJsonUtils
{
	public static IClientMsg DeserClientMsg(string str)
	{
		return Get<ClientMsgShell>().Type switch
		{
			// @formatter:off
			ClientMsgType.ReqScriptsSync	=> Get<ReqScriptsSyncClientMsg>(),
			ClientMsgType.HookCalled		=> Get<HookCalledClientMsg>(),
			ClientMsgType.HookArgCalled		=> Get<HookArgCalledClientMsg>(),
			ClientMsgType.AnswerDomSnapshot	=> Get<AnswerDomSnapshotClientMsg>(),
			ClientMsgType.User				=> Get<UserClientMsg>(),
			// @formatter:on
			_ => throw new ArgumentException()
		};

		T Get<T>() where T : IClientMsg => JsonSerializer.Deserialize<T>(str, jsonOpt) ?? throw new ArgumentException();
	}

	public static string SerServerMsg(IServerMsg msg) => JsonSerializer.Serialize(msg, msg.GetType(), jsonOpt);

	private static readonly JsonSerializerOptions jsonOpt = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };
	static SyncJsonUtils() => jsonOpt.Converters.Add(new JsonStringEnumConverter());
	// ReSharper disable once ClassNeverInstantiated.Local
	private record ClientMsgShell(ClientMsgType Type) : IClientMsg;
}


// *******************
// * Server Messages *
// *******************

public enum DomOpType
{
	InsertHtmlUnderBody,
	InsertHtmlUnderParent,
	DeleteHtmlUnderParent,
	ReplaceHtmlUnderParent,
	DeleteParent,
}

public enum ScriptType
{
	Css,
	Js
}


public enum ServerMsgType
{
	FullUpdate,
	ReplyScriptsSync,
	ScriptRefresh,
	ChgsDomUpdate,
	DomOp,
	ReqDomSnapshot,
	ReqCallMethodOnNode
}

// @formatter:off
public interface IServerMsg { ServerMsgType Type { get; } }

public record FullUpdateServerMsg			(string Html)																			: IServerMsg { public ServerMsgType Type => ServerMsgType.FullUpdate;			}
public record ReplyScriptsSyncServerMsg		(string[] CssLinksDel, string[] CssLinksAdd, string[] JsLinksDel, string[] JsLinksAdd)	: IServerMsg { public ServerMsgType Type => ServerMsgType.ReplyScriptsSync;		}
// Type:Css  Link:css/edit-list?c=6
public record ScriptRefreshServerMsg		(ScriptType ScriptType, string Link)													: IServerMsg { public ServerMsgType Type => ServerMsgType.ScriptRefresh;		}
public record ChgsDomUpdateServerMsg		(params Chg[] Chgs)																		: IServerMsg { public ServerMsgType Type => ServerMsgType.ChgsDomUpdate;		}
public record DomOpServerMsg				(DomOpType OpType, string? Html, string? ParentId)										: IServerMsg { public ServerMsgType Type => ServerMsgType.DomOp;				}
public record ReqDomSnapshotServerMsg																								: IServerMsg { public ServerMsgType Type => ServerMsgType.ReqDomSnapshot;		}
public record ReqCallMethodOnNodeServerMsg	(string NodeId, string MethodName)														: IServerMsg { public ServerMsgType Type => ServerMsgType.ReqCallMethodOnNode;	}
// @formatter:on

