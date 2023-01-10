export class ScriptUtils {
    static syncScripts(scriptsNext) {
        function same(s0, s1) { return s0.type === s1.type && ScriptUtils.base(s0.link) === ScriptUtils.base(s1.link); }
        function diff(s0, s1) { return !same(s0, s1); }
        const scriptsPrev = ScriptUtils.getDomScripts();
        const scriptsAdd = scriptsNext.filter(s0 => scriptsPrev.every(s1 => diff(s0, s1)));
        const scriptsDel = scriptsPrev.filter(s0 => scriptsNext.every(s1 => diff(s0, s1)));
        const scriptsBmp = scriptsNext.filter(s0 => scriptsPrev.some(s1 => same(s0, s1)))
            .filter(s => !ScriptUtils.base(s.link).includes('websockets.js'));
        console.log('scriptsPrev', scriptsPrev);
        console.log('scriptsNext', scriptsNext);
        console.log('scriptsAdd', scriptsAdd);
        console.log('scriptsDel', scriptsDel);
        console.log('scriptsBmp', scriptsBmp);
        scriptsAdd.forEach(ScriptUtils.add);
        scriptsDel.forEach(ScriptUtils.del);
        scriptsBmp.forEach(ScriptUtils.bmp);
    }
    static refreshScript(nfo) {
        ScriptUtils.bmp(nfo);
    }
    // ***********
    // * Private *
    // ***********
    static add(nfo) {
        const head = document.getElementsByTagName('head')[0];
        switch (nfo.type) {
            case 'Css': {
                const node = document.createElement('link');
                node.rel = 'stylesheet';
                node.type = 'text/css';
                node.href = nfo.link;
                head.appendChild(node);
                break;
            }
            case 'Js':
            case 'JsModule': {
                const node = document.createElement('script');
                node.src = nfo.link;
                if (nfo.type === 'JsModule')
                    node.type = 'module';
                head.appendChild(node);
                break;
            }
        }
    }
    static del(nfo) {
        const node = ScriptUtils.findNode(nfo);
        node.remove();
    }
    static bmp(nfo) {
        const node = ScriptUtils.findNode(nfo);
        nfo = {
            type: nfo.type,
            link: ScriptUtils.bmpUrl(ScriptUtils.getNodeUrl(node, nfo))
        };
        ScriptUtils.add(nfo);
        // Css:  <=50 causes the styles to disappear shortly on reload
        const delay = nfo.type === 'Css' ? 100 : 1000;
        setTimeout(() => {
            node.remove();
        }, delay);
    }
    static findNode(nfo) {
        function ret(arr) {
            switch (arr.length) {
                case 0: throw new Error(`Failed to find script: ${JSON.stringify(nfo)} (no match)`);
                case 1: return arr[0];
                default: throw new Error(`Failed to find script: ${JSON.stringify(nfo)} (too many matches)`);
            }
        }
        switch (nfo.type) {
            case 'Css':
                return ret(ScriptUtils.getHeadTags('link')
                    .filter(e => ScriptUtils.isFixable(e.href) && e.rel === 'stylesheet')
                    .filter(e => ScriptUtils.base(e.href) === ScriptUtils.base(nfo.link)));
            case 'Js':
            case 'JsModule':
                return ret(ScriptUtils.getHeadTags('script')
                    .filter(e => ScriptUtils.isFixable(e.src))
                    .filter(e => nfo.type === 'Js' || e.type === 'module')
                    .filter(e => ScriptUtils.base(e.src) === ScriptUtils.base(nfo.link)));
        }
    }
    static getDomScripts() {
        return [
            ...ScriptUtils.getHeadTags('link')
                .filter(e => ScriptUtils.isFixable(e.href) && e.rel === 'stylesheet')
                .map(e => ({
                type: 'Css',
                link: ScriptUtils.fix(e.href)
            })),
            ...ScriptUtils.getHeadTags('script')
                .filter(e => ScriptUtils.isFixable(e.src))
                .map(e => ({
                type: e.type === 'module' ? 'JsModule' : 'Js',
                link: ScriptUtils.fix(e.src)
            })),
        ];
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
    static isFixable(url) { return url.startsWith(window.location.origin); }
    // Input : http://box-pc:7000/other/style.css?cnt=3
    // Output:                    other/style.css?cnt=3
    static fix(url) { return url.substring(window.location.origin.length + 1); }
    // Input : http://box-pc:7000/other/style.css?cnt=3
    // Output:                    other/style.css
    static base(url) {
        if (ScriptUtils.isFixable(url))
            url = ScriptUtils.fix(url);
        const idx = url.indexOf("?");
        if (idx !== -1)
            url = url.substring(0, idx);
        return url;
    }
    // Input :                    js/simple.js?c=3
    // Output:                    js/simple.js?c=4
    // Input : http://box-pc:7000/js/simple.js
    // Output:                    js/simple.js?c=1
    static bmpUrl(url) {
        if (ScriptUtils.isFixable(url))
            url = ScriptUtils.fix(url);
        const baseLnk = this.base(url);
        const idx = url.indexOf("?c=");
        if (idx === -1)
            return `${baseLnk}?c=1`;
        const numStr = url.substring(idx + 3);
        const num = +numStr;
        return `${baseLnk}?c=${num + 1}`;
    }
    static getNodeUrl(node, nfo) {
        switch (nfo.type) {
            case 'Css':
            case 'Manifest':
                return node.href;
            case 'Js':
            case 'JsModule':
                return node.src;
        }
    }
}
window.syncScripts = ScriptUtils.syncScripts;
