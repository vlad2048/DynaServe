import { handleServerMsg } from "./websockets-handlers.js";
var socket = null;
console.log('LOADING MODULE websockets.js');
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
        socket.onerror = () => {
            console.log('ON-ERROR');
            updateState();
        };
        socket.onopen = () => {
            console.log('ON-OPEN');
            updateState();
        };
        socket.onclose = () => {
            console.log('ON-CLOSE');
            updateState();
            setTimeout(() => {
                console.log('ON-ERROR -> RECONNECT');
                connectSocket();
            }, 100);
        };
        socket.onmessage = (evtData) => {
            const evt = JSON.parse(evtData.data);
            console.log(`<== ${evt.type}`);
            handleServerMsg(evt);
            console.log(`<== ${evt.type} (done)`);
        };
    }
    connectSocket();
}
// init() doesn't work properly on Safari without the delay
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
