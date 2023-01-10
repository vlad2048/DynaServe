using DynaServeLib.Nodes;
using DynaServeLib.Serving.Syncing.Structs;

namespace DynaServeLib.DynaLogic.Events;

interface IDomEvt {}
record UpdateChildrenDomEvt(string NodeId, HtmlNode[] Children) : IDomEvt;
record AddBodyNodeDomEvt(HtmlNode Node) : IDomEvt;
record RemoveBodyNodeDomEvt(string NodeId) : IDomEvt;
record ChgDomEvt(Chg Chg) : IDomEvt;
