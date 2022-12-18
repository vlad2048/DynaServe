namespace DynaServeLib.Serving.Syncing.Structs;

enum ClientMsgType
{
	ReqCssSync,
	HookCalled,
	HookArgCalled,
	ReqFullLog
}

class ClientMsg
{
	public ClientMsgType Type { get; init; }
	public string? Id { get; init; }
	public string? EvtName { get; init; }
	public string? EvtArg { get; init; }
	public string? Html { get; init; }
	public string[]? CssLinks { get; init; }
}

public enum ServerMsgType
{
	FullUpdate,
	CssSync,
	AddChildToBody,
	ReplaceChildren,
	AddScriptCss,
	AddScriptJs,
	RefreshCss,
	SetAttr,
	ReqCallMethodOnNode
}

public class ServerMsg
{
	public ServerMsgType Type { get; private init; }
	public string? Html { get; private init; }
	public string[]? CssSyncRemove { get; private init; }
	public string[]? CssSyncAdd { get; private init; }
	public string? NodeId { get; private init; }
	public string? ScriptLink { get; private init; }
	public string? CssLinkRefresh { get; private init; }
	public string? AttrKey { get; private init; }
	public string? AttrVal { get; private init; }
	public string? MethodName { get; private init; }

	public static ServerMsg MkFullUpdate(string html) => new()
	{
		Type = ServerMsgType.FullUpdate,
		Html = html
	};

	public static ServerMsg MkCssSync(string[] remove, string[] add) => new()
	{
		Type = ServerMsgType.CssSync,
		CssSyncRemove = remove,
		CssSyncAdd = add
	};

	public static ServerMsg MkAddChildToBody(string html) => new()
	{
		Type = ServerMsgType.AddChildToBody,
		Html = html
	};

	public static ServerMsg MkReplaceChildren(string html, string nodeId) => new()
	{
		Type = ServerMsgType.ReplaceChildren,
		Html = html,
		NodeId = nodeId
	};

	public static ServerMsg MkAddScriptCss(string link) => new()
	{
		Type = ServerMsgType.AddScriptCss,
		ScriptLink = link
	};

	public static ServerMsg MkAddScriptJs(string link) => new()
	{
		Type = ServerMsgType.AddScriptJs,
		ScriptLink = link
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
		AttrVal = attrVal
	};

	public static ServerMsg MkReqCallMethodOnNode(string nodeId, string methodName) => new()
	{
		Type = ServerMsgType.ReqCallMethodOnNode,
		NodeId = nodeId,
		MethodName = methodName
	};
}
