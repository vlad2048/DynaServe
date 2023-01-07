export class LinkUtils {
    static httpLink = "{{HttpLink}}"; // http://box-pc:7000/
    // Input : file:///C:/folder/js/simple.js?c=3
    // Output:                   js/simple.js?c=3
    static delPrefix(lnk) {
        let idx = lnk.indexOf("css/");
        if (idx !== -1)
            lnk = lnk.substring(idx);
        //idx = lnk.indexOf("js/");
        //if (idx !== -1) lnk = lnk.substring(idx);
        if (lnk.startsWith(this.httpLink))
            lnk = lnk.substring(this.httpLink.length);
        return lnk;
    }
    // Input : file:///C:/folder/js/simple.js?c=3
    // Output: file:///C:/folder/js/simple.js
    static delSuffix(lnk) {
        let idx = lnk.indexOf("?");
        if (idx !== -1)
            lnk = lnk.substring(0, idx);
        return lnk;
    }
    // Input :                   js/simple.js?c=3
    // Output:                   js/simple.js
    // Input : file:///C:/folder/js/simple.js
    // Output:                   js/simple.js
    static getBaseLnk(lnk) {
        return this.delPrefix(this.delSuffix(lnk));
    }
    static isMatch(l1, l2) {
        return this.getBaseLnk(l1) === this.getBaseLnk(l2);
    }
    // Input :                   js/simple.js?c=3
    // Output:                   js/simple.js?c=4
    // Input : file:///C:/folder/js/simple.js
    // Output:                   js/simple.js?c=1
    static bumpLnk(lnk) {
        lnk = this.delPrefix(lnk);
        const baseLnk = this.getBaseLnk(lnk);
        const idx = lnk.indexOf("?c=");
        if (idx === -1)
            return `${baseLnk}?c=1`;
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
    static relevantUrlCss(url) {
        return (!!url &&
            (url.includes("css/")) &&
            (url.startsWith("file:///") || url.startsWith(this.httpLink)));
    }
    static relevantUrlJs(url) {
        return (!!url &&
            (url.startsWith("file:///") || url.startsWith(this.httpLink)));
    }
    // Input : 'script'
    // Output: Node[]
    static getHeadTags(tagName) {
        const tagsSource = document
            .getElementsByTagName("head")[0]
            .getElementsByTagName(tagName);
        const tags = [];
        for (let i = 0; i < tagsSource.length; i++) {
            const elt = tagsSource[i];
            tags.push(elt);
        }
        return tags;
    }
    static getCssScripts() {
        return this.getHeadTags("link")
            .filter((e) => e.rel === "stylesheet")
            .filter((e) => this.relevantUrlCss(e.href));
    }
    static getJsScripts() {
        return this.getHeadTags("script")
            .filter((e) => this.relevantUrlJs(e.src));
    }
    static mkCssScript(lnk) {
        const head = document.getElementsByTagName("head")[0];
        const tag = document.createElement("link");
        tag.rel = "stylesheet";
        tag.type = "text/css";
        tag.href = lnk;
        head.appendChild(tag);
    }
    static mkJsScript(lnk) {
        const head = document.getElementsByTagName("head")[0];
        const tag = document.createElement("script");
        tag.src = lnk;
        head.appendChild(tag);
    }
    static isSysJsLnk(lnk) {
        return lnk.includes("js/websockets.js");
    }
}
