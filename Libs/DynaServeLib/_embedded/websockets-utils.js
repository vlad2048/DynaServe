var httpLink = "{{HttpLink}}";
//var httpLink = "http://box-pc:7000/";

function runCheck() {
    console.log("runCheck_0");
}

// *********
// * Utils *
// *********
// Input : file:///C:/folder/js/simple.js?c=3
// Output:                   js/simple.js?c=3
function delPrefix(lnk) {
    let idx = lnk.indexOf("css/");
    if (idx !== -1) lnk = lnk.substring(idx);
    idx = lnk.indexOf("js/");
    if (idx !== -1) lnk = lnk.substring(idx);
    return lnk;
}

// Input : file:///C:/folder/js/simple.js?c=3
// Output: file:///C:/folder/js/simple.js
function delSuffix(lnk) {
    let idx = lnk.indexOf("?");
    if (idx !== -1) lnk = lnk.substring(0, idx);
    return lnk;
}

// Input :                   js/simple.js?c=3
// Output:                   js/simple.js
// Input : file:///C:/folder/js/simple.js
// Output:                   js/simple.js
function getBaseLnk(lnk) {
    return delPrefix(delSuffix(lnk));
}
function isMatch(l1, l2) {
    return getBaseLnk(l1) === getBaseLnk(l2);
}

// Input :                   js/simple.js?c=3
// Output:                   js/simple.js?c=4
// Input : file:///C:/folder/js/simple.js
// Output:                   js/simple.js?c=1
function bumpLnk(lnk) {
    lnk = delPrefix(lnk);
    const baseLnk = getBaseLnk(lnk);
    const idx = lnk.indexOf("?c=");
    if (idx === -1) return `${baseLnk}?c=1`;
    const numStr = lnk.substring(idx + 3);
    const num = +numStr;
    return `${baseLnk}?c=${num + 1}`;
}

// Input : http://box-pc:7000/css/websockets.css
// Output: true
// Input : file:///C:/Dev_Nuget/Libs/DynaServe/Libs/DynaServeLib/_embedded/websockets.css
// Output: false
// Input : file:///C:/Dev_Nuget/Libs/DynaServe/Libs/DynaServeLib/_embedded/css/websockets.css
// Output: true
// Input : http://google.com/...
// Output: false
function relevantUrl(url) {
    return (
        !!url &&
        (url.includes("css/") || url.includes("js/")) &&
        (url.startsWith("file:///") || url.startsWith(httpLink))
    );
}

// Input : 'script'
// Output: Node[]
function getHeadTags(tagName) {
    const tagsSource = document
        .getElementsByTagName("head")[0]
        .getElementsByTagName(tagName);
    const tags = [];
    for (let i = 0; i < tagsSource.length; i++) tags.push(tagsSource[i]);
    return tags;
}

function getCssScripts() {
    return getHeadTags("link")
        .filter((e) => e.rel === "stylesheet")
        .filter((e) => relevantUrl(e.href));
}

function getJsScripts() {
    return getHeadTags("script").filter((e) => relevantUrl(e.src));
}

function mkCssScript(lnk) {
    const head = document.getElementsByTagName("head")[0];
    const tag = document.createElement("link");
    tag.rel = "stylesheet";
    tag.type = "text/css";
    tag.href = lnk;
    head.appendChild(tag);
}

function mkJsScript(lnk) {
    const head = document.getElementsByTagName("head")[0];
    const tag = document.createElement("script");
    tag.src = lnk;
    head.appendChild(tag);
}

function isSysJsLnk(lnk) {
    lnk.includes("js/websockets.js");
}

function play() {
    console.log(bumpLnk("js/simple.js?c=3"));
    console.log(bumpLnk("file:///C:/folder/js/simple.js"));
}

// ************************
// * getReqScriptsSyncMsg *
// ************************
/*
Output:
{
	cssLinks: string[];
	jsLinks: string[];
}
*/
function getReqScriptsSyncMsg() {
    return {
        cssLinks: getCssScripts().map((e) => getBaseLnk(e.href)),
        jsLinks: getJsScripts().map((e) => getBaseLnk(e.src)),
    };
}

// **************************
// * handleReplyScriptsSync *
// **************************
/*
Input:
{
	cssLinksDel: string[];
	cssLinksAdd: string[];
	jsLinksDel: string[];
	jsLinksAdd: string[];
}
*/
var isFirstTime = true;
function handleReplyScriptsSync(msg) {
    getCssScripts()
        .filter((e) => msg.cssLinksDel.some((f) => isMatch(f, e.href)))
        .forEach((e) => e.remove());
    //if (!isFirstTime)
    //getCssScripts().forEach((e) => (e.href = bumpLnk(e.href)));
    getCssScripts().forEach((e) => {
        mkCssScript(bumpLnk(e.href));
        setTimeout(() => {
            e.remove();
        }, 100); // <=50 causes the styles to disappear shortly on reload
    });
    msg.cssLinksAdd.forEach(mkCssScript);

    /*
	getJsScripts()
		.filter(e => msg.jsLinksDel.some(f => isMatch(f, e.src)))
		.forEach(e => e.remove());
	//if (!isFirstTime)
	//  getJsScripts().forEach((e) => (e.src = bumpLnk(e.src)));
	getJsScripts().forEach(e => {
		mkJsScript(bumpLnk(e.src));
		setTimeout(() => {
			e.remove();
		}, 1000); // <=50 causes the styles to disappear shortly on reload
	});
	msg.jsLinksAdd.forEach(mkJsScript);
  */
    if (isFirstTime) isFirstTime = false;
}

// ***********************
// * handleScriptRefresh *
// ***********************
/*
Input:
{
	type: 'Css' | 'Js';
	link: string;
}
*/
function handleScriptRefresh(msg) {
    switch (msg.type) {
        case "Css": {
            const nodes = getCssScripts().filter((e) =>
                isMatch(e.href, msg.link)
            );
            if (nodes.length === 0)
                throw new Error("Failed to find the CSS script to refresh");
            if (nodes.length > 1)
                throw new Error("Found too many CSS scripts to refresh");
            const node = nodes[0];
            const bl = bumpLnk(node.href);

            node.href = bl;

            break;
        }

        case "Js": {
            const nodes = getJsScripts().filter((e) =>
                isMatch(e.src, msg.link)
            );
            if (nodes.length === 0)
                throw new Error("Failed to find the JS script to refresh");
            if (nodes.length > 1)
                throw new Error("Found too many JS scripts to refresh");
            const node = nodes[0];
            const bl = bumpLnk(node.src);

            if (isSysJsLnk(node.src)) {
                node.src = bl;
            } else {
                //node.remove();
                //mkJsScript(bl);

                mkJsScript(bl);
                setTimeout(() => {
                    node.remove();
                }, 100);
            }

            break;
        }

        default:
            throw new Error(`Invalid ScriptType: ${msg.type}`);
    }
}



function fixAutofocus() {
	const autofocusElt = document.querySelector("[autofocus]");
	if (!!autofocusElt) {
		autofocusElt.focus();
	}
}