import { LinkUtils } from "./link-utils.js";
import { ScriptType } from "./websockets-types.js";


// ************************
// * getReqScriptsSyncMsg *
// ************************
type LinkSet = {
	cssLinks: string[];
	jsLinks: string[];
}
export function getReqScriptsSyncMsg(): LinkSet {
	return {
		cssLinks: LinkUtils.getCssScripts().map((e) => LinkUtils.getBaseLnk(e.href)),
		jsLinks: LinkUtils.getJsScripts().map((e) => LinkUtils.getBaseLnk(e.src)),
	};
}

// **************************
// * handleReplyScriptsSync *
// **************************
var isFirstTime = true;
export function handleReplyScriptsSync(
	cssLinksDel: string[],
	cssLinksAdd: string[],
	jsLinksDel: string[],
	jsLinksAdd: string[]
) {
	LinkUtils.getCssScripts().filter((e) => cssLinksDel.some((f) => LinkUtils.isMatch(f, e.href))).forEach((e) => e.remove());
	if (!isFirstTime) LinkUtils.getCssScripts().forEach((e) => (e.href = LinkUtils.bumpLnk(e.href)));
	LinkUtils.getCssScripts().forEach((e) => {
		LinkUtils.mkCssScript(LinkUtils.bumpLnk(e.href));
		setTimeout(() => {
			e.remove();
		}, 100); // <=50 causes the styles to disappear shortly on reload
	});
	cssLinksAdd.forEach(LinkUtils.mkCssScript);

  console.log('jsDel', jsLinksDel);
  console.log('jsAdd', jsLinksAdd);

	LinkUtils.getJsScripts()
		.filter(e => jsLinksDel.some(f => LinkUtils.isMatch(f, e.src)))
		.forEach(e => e.remove());
	if (!isFirstTime)
    LinkUtils.getJsScripts().forEach((e) => (e.src = LinkUtils.bumpLnk(e.src)));
  LinkUtils.getJsScripts().forEach(e => {
    LinkUtils.mkJsScript(LinkUtils.bumpLnk(e.src));
		setTimeout(() => {
			e.remove();
		}, 1000); // <=50 causes the styles to disappear shortly on reload
	});
	jsLinksAdd.forEach(LinkUtils.mkJsScript);
	if (isFirstTime) isFirstTime = false;
}

// ***********************
// * handleScriptRefresh *
// ***********************
export function handleScriptRefresh(type: ScriptType, link: string) {
	switch (type) {
		case "Css": {
			const nodes = LinkUtils.getCssScripts().filter((e) =>
      LinkUtils.isMatch(e.href, link)
			);
			if (nodes.length === 0)
				throw new Error("Failed to find the CSS script to refresh");
			if (nodes.length > 1)
				throw new Error("Found too many CSS scripts to refresh");
			const node = nodes[0];
			const bl = LinkUtils.bumpLnk(node.href);

			node.href = bl;

			break;
		}

		case "Js": {
			const nodes = LinkUtils.getJsScripts().filter((e) => LinkUtils.isMatch(e.src, link));
			if (nodes.length === 0)
				throw new Error("Failed to find the JS script to refresh");
			if (nodes.length > 1)
				throw new Error("Found too many JS scripts to refresh");
			const node = nodes[0];
			const bl = LinkUtils.bumpLnk(node.src);

			if (LinkUtils.isSysJsLnk(node.src)) {
				node.src = bl;
			} else {
				//node.remove();
				//mkJsScript(bl);

				LinkUtils.mkJsScript(bl);
				setTimeout(() => {
					node.remove();
				}, 100);
			}

			break;
		}
	}
}



export function fixAutofocus() {
	const autofocusElt = document.querySelector<HTMLElement>("[autofocus]");
	if (!!autofocusElt) {
		autofocusElt.focus();
	}
}



// *************
// * ShowError *
// *************
export function showError(err) {
  if (!err) return;
  const errorPanelId = 'error-panel';

  var div = document.getElementById(errorPanelId);
  if (!div) {
    const body = document.body;
    div = document.createElement('div');
    div.id = errorPanelId;
    div.style.position = 'fixed';
    div.style.left = '20px';
    div.style.top = '10px';
    div.style.width = '90%';
    div.style.height = '80%';
    div.style.padding = '5px';
    div.style.border = '2px solid black';
    div.style.backgroundColor = '#333C';
    div.style.color = '#FFF';
    div.style.zIndex = '20';
    body.append(div);
  }
  div.innerText += `${err.message}\n${JSON.stringify(err)}\n\n`;
  div.style.left = '20px';
}