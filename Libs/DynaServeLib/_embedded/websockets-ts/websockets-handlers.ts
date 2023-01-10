import { send } from "./websockets.js";
import { ServerMsg } from "./websockets-types.js";
import { ScriptUtils } from "./script-utils.js";
import { DomUtils } from "./dom-utils.js";


export function handleServerMsg(evt: ServerMsg) {

	// ****************************
	// ****************************
	// ** Handle Server Messages **
	// ****************************
	// ****************************
	switch (evt.type) {
		case "FullUpdate": {
			DomUtils.replaceBody(evt.html);
			ScriptUtils.syncScripts(evt.scripts);
			break;
		}

    
		/*case "ReplyScriptsSync": {
			handleReplyScriptsSync(evt.cssLinksDel, evt.cssLinksAdd, evt.jsLinksDel, evt.jsLinksAdd);
			break;
		}*/

		case "ScriptRefresh": {
			ScriptUtils.refreshScript(evt.scriptNfo);
			break;
		}

		case "ChgsDomUpdate": {
			for (let chg of evt.chgs) {
				console.log(chg);
        		const node = document.evaluate(chg.nodePath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue as HTMLElement;
        		if (!node) continue;

				switch (chg.type) {
					case "Text":
            			if (chg.val !== undefined)
							node.innerText = chg.val;
						break;

					case "Attr":
						if (chg.name === "class") {
              				if (chg.val !== undefined)
							  node.className = chg.val;
							} else {
								if (chg.val === undefined || chg.val === null)
									node.removeAttribute(chg.name!);
								else
									node.setAttribute(chg.name!, chg.val);
						}
						break;

					case "Prop":
						switch (chg.propType) {
							case "Str":
                				let a: HTMLDivElement = null!;
								node[chg.name!] = chg.val;
								break;

							case "Bool":
								node[chg.name!] = !!chg.val;
								break;
						}
						break;

					default:
						throw new Error(`Invalid PropChangeType: ${chg.type}`);
				}
			}
			break;
		}


		case "DomOp": {
			switch (evt.opType) {
				case "InsertHtmlUnderBody":
				{
					const body = document.getElementsByTagName("body")[0];
					body.innerHTML += evt.html;
					DomUtils.fixAutofocus();
					break;
				}

				case "InsertHtmlUnderParent":
				{
					const elt = document.getElementById(evt.parentId);
					elt.innerHTML += evt.html;
					DomUtils.fixAutofocus();
					break;
				}

				case "DeleteHtmlUnderParent":
				{
					const elt = document.getElementById(evt.parentId);
					elt.innerHTML = "";
					break;
				}

				case "ReplaceHtmlUnderParent":
				{
					const elt = document.getElementById(evt.parentId);
					elt.innerHTML = evt.html;
					DomUtils.fixAutofocus();
					break;
				}

				case "DeleteParent":
				{
					const elt = document.getElementById(evt.parentId);
					elt.remove();
					break;
				}

				default:
					throw new Error(`Invalid DomOpType: ${evt.type}`);
			}
			break;
		}



		case "ReqDomSnapshot": {
			send({
				type: "AnswerDomSnapshot",
			  head: document.head.outerHTML,
				body: document.body.outerHTML,
			});
			break;
		}

		case "ReqCallMethodOnNode": {
			const elt = document.getElementById(evt.nodeId);
			elt[evt.methodName]();
			break;
		}

		case "ShowError": {
		  const err = new Error('Test ErrorYES2');
		  DomUtils.showError(err);
		}
    
	}
}