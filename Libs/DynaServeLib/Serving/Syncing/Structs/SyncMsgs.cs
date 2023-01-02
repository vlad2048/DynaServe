namespace DynaServeLib.Serving.Syncing.Structs;

enum ClientMsgType
{
	ReqScriptsSync,
	HookCalled,
	HookArgCalled,
	AnswerDomSnapshot,
	User
}

record ReqScriptsSyncMsg(
	string[] CssLinks,
	string[] JsLinks
);

record ClientDomSnapshot(
	string Head,
	string Body
);

class ClientMsg
{
	public ClientMsgType Type { get; init; }
	public ReqScriptsSyncMsg? ReqScriptsSyncMsg { get; init; }
	public string? Id { get; init; }
	public string? EvtName { get; init; }
	public string? EvtArg { get; init; }
	public string? Html { get; init; }
	public ClientDomSnapshot? ClientDomSnapshot { get; init; }
	public string? UserType { get; init; }
	public string? UserArg { get; init; }
}

public enum ServerMsgType
{
	FullUpdate,
	ReplyScriptsSync,
	ScriptRefresh,

	PropChangesDomUpdate,
	ReplaceChildrenDomUpdate,

	ReqDomSnapshot,

	AddChildToBody,
	RemoveChildFromBody,
	ReqCallMethodOnNode
}

public record ReplyScriptsSyncMsg(
	string[] CssLinksDel,
	string[] CssLinksAdd,
	string[] JsLinksDel,
	string[] JsLinksAdd
);

public enum ScriptType
{
	Css,
	Js
}

// Type:Css  Link:css/edit-list?c=6
public record ScriptRefreshNfo(ScriptType Type, string Link);

public enum PropChangeType
{
	Attr,
	Text
}

public class PropChange
{
	public PropChangeType Type { get; }
	public string NodeId { get; }
	public string? AttrName { get; private init; }
	public string? AttrVal { get; private init; }
	public string? TextVal { get; private init; }

	private PropChange(PropChangeType type, string nodeId)
	{
		Type = type;
		NodeId = nodeId;
	}

	public static PropChange MkAttrChange(string nodeId, string attrName, string? attrVal) =>
		new(PropChangeType.Attr, nodeId)
		{
			AttrName = attrName,
			AttrVal = attrVal,
		};

	public static PropChange MkTextChange(string nodeId, string? textVal) =>
		new(PropChangeType.Text, nodeId)
		{
			TextVal = textVal,
		};
}


public class ServerMsg
{
	public ServerMsgType Type { get; }
	public string? Html { get; private init; }
	public ReplyScriptsSyncMsg? ReplyScriptsSyncMsg { get; private init; }
	public ScriptRefreshNfo? ScriptRefreshNfo { get; private init; }
	public PropChange[]? PropChanges { get; private init; }
	public string? NodeId { get; private init; }
	public string? MethodName { get; private init; }

	private ServerMsg(ServerMsgType type) => Type = type;

	public static ServerMsg MkFullUpdate(string html) => new(ServerMsgType.FullUpdate)
	{
		Html = html,
	};

	public static ServerMsg MkReplyScriptsSync(ReplyScriptsSyncMsg replyScriptsSyncMsg) => new(ServerMsgType.ReplyScriptsSync)
	{
		ReplyScriptsSyncMsg = replyScriptsSyncMsg
	};

	public static ServerMsg MkScriptRefresh(ScriptType scriptType, string link) => new(ServerMsgType.ScriptRefresh)
	{
		ScriptRefreshNfo = new ScriptRefreshNfo(scriptType, link)
	};

	public static ServerMsg MkPropChangesDomUpdate(PropChange[] propChanges) => new(ServerMsgType.PropChangesDomUpdate)
	{
		PropChanges = propChanges,
	};

	public static ServerMsg MkReplaceChildrenDomUpdate(string html, string nodeId) => new(ServerMsgType.ReplaceChildrenDomUpdate)
	{
		Html = html,
		NodeId = nodeId,
	};

	public static ServerMsg MkReqDomSnapshot() => new(ServerMsgType.ReqDomSnapshot);

	public static ServerMsg MkAddChildToBody(string html) => new(ServerMsgType.AddChildToBody)
	{
		Html = html,
	};

	public static ServerMsg MkRemoveChildFromBody(string nodeId) => new(ServerMsgType.RemoveChildFromBody)
	{
		NodeId = nodeId,
	};

	public static ServerMsg MkReqCallMethodOnNode(string nodeId, string methodName) => new(ServerMsgType.ReqCallMethodOnNode)
	{
		NodeId = nodeId,
		MethodName = methodName,
	};
}
