using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynaServeLib.Serving.Syncing.Structs;


// *******************
// * Client Messages *
// *******************

public enum ClientMsgType
{
	//Error,
	//ReqScriptsSync,
	HookCalled,
	HookArgCalled,
	AnswerDomSnapshot,
	User
}

// @formatter:off
public interface IClientMsg { ClientMsgType Type { get; } }

//public record ErrorClientMsg				(string Message)							: IClientMsg { public ClientMsgType Type => ClientMsgType.Error;		}
//public record ReqScriptsSyncClientMsg		(string[] CssLinks, string[] JsLinks)		: IClientMsg { public ClientMsgType Type => ClientMsgType.ReqScriptsSync;		}
public record HookCalledClientMsg			(string Id, string EvtName)					: IClientMsg { public ClientMsgType Type => ClientMsgType.HookCalled;			}
public record HookArgCalledClientMsg		(string Id, string EvtName, string EvtArg)	: IClientMsg { public ClientMsgType Type => ClientMsgType.HookArgCalled;		}
public record AnswerDomSnapshotClientMsg	(string Head, string Body)					: IClientMsg { public ClientMsgType Type => ClientMsgType.AnswerDomSnapshot;	}
public record UserClientMsg					(string UserType, string Arg)				: IClientMsg { public ClientMsgType Type => ClientMsgType.User;					}
// @formatter:on

static class SyncJsonUtils
{
	public static IClientMsg DeserClientMsg(string str)
	{
		return Get<ClientMsgShell>().Type switch
		{
			// @formatter:off
			//ClientMsgType.ReqScriptsSync	=> Get<ReqScriptsSyncClientMsg>(),
			ClientMsgType.HookCalled		=> Get<HookCalledClientMsg>(),
			ClientMsgType.HookArgCalled		=> Get<HookArgCalledClientMsg>(),
			ClientMsgType.AnswerDomSnapshot	=> Get<AnswerDomSnapshotClientMsg>(),
			ClientMsgType.User				=> Get<UserClientMsg>(),
			// @formatter:on
			_ => throw new ArgumentException($"Invalid ClientMsg.Type: {Get<ClientMsgShell>().Type}")
		};

		T Get<T>() where T : IClientMsg => JsonSerializer.Deserialize<T>(str, jsonOpt) ?? throw new ArgumentException("Failed to deserialize ClientMsg");
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
	Js,
	JsModule,
	Manifest,
}

public record ScriptNfo(
	ScriptType Type,
	string Link
);


public enum ServerMsgType
{
	FullUpdate,
	//ReplyScriptsSync,
	ScriptRefresh,
	ChgsDomUpdate,
	DomOp,
	ReqDomSnapshot,
	ReqCallMethodOnNode,
	ShowError,
}

// @formatter:off
public interface IServerMsg { ServerMsgType Type { get; } }

public record FullUpdateServerMsg			(string Html, ScriptNfo[] Scripts)														: IServerMsg { public ServerMsgType Type => ServerMsgType.FullUpdate;			}
//public record ReplyScriptsSyncServerMsg		(string[] CssLinksDel, string[] CssLinksAdd, string[] JsLinksDel, string[] JsLinksAdd)	: IServerMsg { public ServerMsgType Type => ServerMsgType.ReplyScriptsSync;		}
// Type:Css  Link:css/edit-list?c=6
public record ScriptRefreshServerMsg		(ScriptNfo ScriptNfo)																	: IServerMsg { public ServerMsgType Type => ServerMsgType.ScriptRefresh;		}
public record ChgsDomUpdateServerMsg		(params Chg[] Chgs)																		: IServerMsg { public ServerMsgType Type => ServerMsgType.ChgsDomUpdate;		}
public record DomOpServerMsg				(DomOpType OpType, string? Html, string? ParentId)										: IServerMsg { public ServerMsgType Type => ServerMsgType.DomOp;				}
public record ReqDomSnapshotServerMsg																								: IServerMsg { public ServerMsgType Type => ServerMsgType.ReqDomSnapshot;		}
public record ReqCallMethodOnNodeServerMsg	(string NodeId, string MethodName)														: IServerMsg { public ServerMsgType Type => ServerMsgType.ReqCallMethodOnNode;	}
public record ShowErrorServerMsg																									: IServerMsg { public ServerMsgType Type => ServerMsgType.ShowError;			}
// @formatter:on

