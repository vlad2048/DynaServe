// *******************
// * Client Messages *
// *******************

export type ClientMsg =
  /*{ type: 'Error',
    message: string } |*/
  { type: 'ReqScriptsSync',
    cssLinks: string[], jsLinks: string[] } |
  { type: 'HookCalled',
    id: string, evtName: string } |
  { type: 'HookArgCalled',
    id: string; evtName: string; evtArg: string } |
  { type: 'AnswerDomSnapshot',
    head: string; body: string } |
  { type: 'User',
    userType: string; arg: string };




// *******************
// * Server Messages *
// *******************

type DomOpType =
	'InsertHtmlUnderBody' |
	'InsertHtmlUnderParent' |
	'DeleteHtmlUnderParent' |
	'ReplaceHtmlUnderParent' |
	'DeleteParent';

export type ScriptType = 'Css' | 'Js';

type ChgType = 'Text' | 'Attr' | 'Prop';
type ChgPropType = 'Str' | 'Bool';
export interface Chg {
  nodePath: string;
  type: ChgType;
  propType: ChgPropType;
  name?: string;
  val?: string;
}

export type ServerMsg =
  { type: 'FullUpdate',
    html: string } |

  { type: 'ReplyScriptsSync',
    cssLinksDel: string[];
    cssLinksAdd: string[];
    jsLinksDel: string[];
    jsLinksAdd: string[]; } |

  { type: 'ScriptRefresh',
    scriptType: ScriptType;
    link: string; } |

  { type: 'ChgsDomUpdate',
    chgs: Chg[] } |

  { type: 'DomOp',
    opType: DomOpType;
    html?: string;
    parentId?: string; } |

  { type: 'ReqDomSnapshot',
  } |

  { type: 'ReqCallMethodOnNode',
    nodeId: string;
    methodName: string; } |
    
  { type: 'ShowError'
  };
