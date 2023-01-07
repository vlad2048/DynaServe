import { handleServerMsg } from "./websockets-handlers.js";
import { getReqScriptsSyncMsg } from "./websockets-utils.js";
var socket = null;
function init() {
    const socketUrl = "{{WSLink}}";
    const statusEltId = "{{StatusEltId}}";
    function updateState() {
        const elt = document.getElementById(statusEltId);
        if (!elt)
            return;
        switch (socket.readyState) {
            case WebSocket.CONNECTING:
                elt.innerText = "CONNECTING";
                break;
            case WebSocket.OPEN:
                elt.innerText = "OPEN";
                break;
            case WebSocket.CLOSING:
                elt.innerText = "CLOSING";
                break;
            case WebSocket.CLOSED:
                elt.innerText = "CLOSED";
                break;
            default:
                elt.innerText = `Unknown:${socket.readyState}`;
                break;
        }
    }
    function connectSocket() {
        socket = new WebSocket(socketUrl);
        socket.onerror = () => updateState();
        socket.onopen = () => {
            updateState();
            /*const linkSet = getReqScriptsSyncMsg();
            send({
                type: "ReqScriptsSync",
                cssLinks: linkSet.cssLinks,
                jsLinks: linkSet.jsLinks,
            });*/
        };
        socket.onclose = () => {
            updateState();
            setTimeout(() => connectSocket(), 100);
        };
        socket.onmessage = (evtData) => {
            const evt = JSON.parse(evtData.data);
            console.log(`<== ${evt.type}`);
            handleServerMsg(evt);
            console.log(`<== ${evt.type} (done)`);
            if (evt.type === 'FullUpdate') {
                const linkSet = getReqScriptsSyncMsg();
                send({
                    type: "ReqScriptsSync",
                    cssLinks: linkSet.cssLinks,
                    jsLinks: linkSet.jsLinks,
                });
            }
        };
    }
    connectSocket();
}
// init() doesn't work properly on Safari without the delay
console.log('before init');
setTimeout(() => {
    console.log('init');
    init();
}, 0);
// **************************
// **************************
// ** Send Client Messages **
// **************************
// **************************
export function send(msg) {
    console.log(`==> ${msg.type}`);
    socket.send(JSON.stringify(msg));
}
function sockEvt(id, evtName) {
    send({
        type: "HookCalled",
        id,
        evtName,
    });
}
function sockEvtArg(id, evtName, evtArg) {
    if (Object.prototype.toString.call(evtArg) !== "[object String]") {
        evtArg = evtArg.toString();
    }
    send({
        type: "HookArgCalled",
        id,
        evtName,
        evtArg,
    });
}
window.sockEvt = sockEvt;
window.sockEvtArg = sockEvtArg;
