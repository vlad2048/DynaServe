import { send } from "./websockets.js";
import { ServerMsg } from "./websockets-types.js";
import { fixAutofocus, handleReplyScriptsSync, handleScriptRefresh, showError } from "./websockets-utils";

var statusEltId = "{{StatusEltId}}";

export function handleServerMsg(evt: ServerMsg) {

	// ****************************
	// ****************************
	// ** Handle Server Messages **
	// ****************************
	// ****************************
	switch (evt.type) {
		case "FullUpdate": {
			// Empty the body (except for #syncserv-status)
			const body = document.getElementsByTagName("body")[0];
			for (let i = body.children.length - 1; i >= 0; i--) {
				const child = body.children[i];
				if (child.id !== statusEltId) child.remove();
			}
			// Add the dynamic nodes to body
			body.innerHTML += evt.html;
			break;
		}

    
		case "ReplyScriptsSync": {
			handleReplyScriptsSync(evt.cssLinksDel, evt.cssLinksAdd, evt.jsLinksDel, evt.jsLinksAdd);
			break;
		}

		case "ScriptRefresh": {
			handleScriptRefresh(evt.scriptType, evt.link);
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
							else node.setAttribute(chg.name!, chg.val);
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
					fixAutofocus();
					break;
				}

				case "InsertHtmlUnderParent":
				{
					const elt = document.getElementById(evt.parentId);
					elt.innerHTML += evt.html;
					fixAutofocus();
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
					fixAutofocus();
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
		  showError(err);
		}
    
	}
}