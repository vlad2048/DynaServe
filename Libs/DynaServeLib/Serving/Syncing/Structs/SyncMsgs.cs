namespace DynaServeLib.Serving.Syncing.Structs;

enum ClientMsgType
{
	ReqCssSync,
	HookCalled,
	HookArgCalled,
	ReqFullLog,
	User
}

class ClientMsg
{
	public ClientMsgType Type { get; init; }
	public string? Id { get; init; }
	public string? EvtName { get; init; }
	public string? EvtArg { get; init; }
	public string? Html { get; init; }
	public string[]? CssLinks { get; init; }
	public string? UserType { get; init; }
	public string? UserArg { get; init; }
}

public enum ServerMsgType
{
	FullUpdate,

	PropChangesDomUpdate,
	ReplaceChildrenDomUpdate,

	CssSync,
	AddChildToBody,
	RemoveChildFromBody,
	RefreshCss,
	SetAttr,
	SetCls,
	ReqCallMethodOnNode
}

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
	public ServerMsgType Type { get; private init; }
	public string? Html { get; private init; }
	public PropChange[]? PropChanges { get; private init; }
	public string[]? CssSyncRemove { get; private init; }
	public string[]? CssSyncAdd { get; private init; }
	public string? NodeId { get; private init; }
	public string? CssLinkRefresh { get; private init; }
	public string? AttrKey { get; private init; }
	public string? AttrVal { get; private init; }
	public string? Cls { get; private init; }
	public string? MethodName { get; private init; }

	public static ServerMsg MkFullUpdate(string html) => new()
	{
		Type = ServerMsgType.FullUpdate,
		Html = html,
	};

	public static ServerMsg MkPropChangesDomUpdate(PropChange[] propChanges) => new()
	{
		Type = ServerMsgType.PropChangesDomUpdate,
		PropChanges = propChanges,
	};

	public static ServerMsg MkReplaceChildrenDomUpdate(string html, string nodeId) => new()
	{
		Type = ServerMsgType.ReplaceChildrenDomUpdate,
		Html = html,
		NodeId = nodeId,
	};


	public static ServerMsg MkCssSync(string[] remove, string[] add) => new()
	{
		Type = ServerMsgType.CssSync,
		CssSyncRemove = remove,
		CssSyncAdd = add,
	};

	public static ServerMsg MkAddChildToBody(string html) => new()
	{
		Type = ServerMsgType.AddChildToBody,
		Html = html,
	};

	public static ServerMsg MkRemoveChildFromBody(string nodeId) => new()
	{
		Type = ServerMsgType.RemoveChildFromBody,
		NodeId = nodeId,
	};

	public static ServerMsg MkRefreshCss(string cssLinkRefresh) => new()
	{
		Type = ServerMsgType.RefreshCss,
		CssLinkRefresh = cssLinkRefresh,
	};

	public static ServerMsg MkSetAttr(string nodeId, string attrKey, string? attrVal) => new()
	{
		Type = ServerMsgType.SetAttr,
		NodeId = nodeId,
		AttrKey = attrKey,
		AttrVal = attrVal,
	};

	public static ServerMsg MkSetCls(string nodeId, string? cls) => new()
	{
		Type = ServerMsgType.SetCls,
		NodeId = nodeId,
		Cls = cls,
	};

	public static ServerMsg MkReqCallMethodOnNode(string nodeId, string methodName) => new()
	{
		Type = ServerMsgType.ReqCallMethodOnNode,
		NodeId = nodeId,
		MethodName = methodName,
	};
}
