var socket = null;

function init() {
    const socketUrl = "{{WSLink}}";
    const statusEltId = "{{StatusEltId}}";
    //const socketUrl = "ws://box-pc:7000/";
    //const statusEltId = 'syncserv-status';

    function updateState() {
        const elt = document.getElementById(statusEltId);
        if (!elt) return;
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
            sockSend({
                type: "ReqScriptsSync",
                reqScriptsSyncMsg: getReqScriptsSyncMsg(),
            });
        };
        socket.onclose = () => {
            updateState();
            setTimeout(() => connectSocket(), 100);
        };

        socket.onmessage = (evt) => {
          handleServerMsg(evt);
        };
    }

    connectSocket();
}

// init() doesn't work properly on Safari without the delay
setTimeout(() => {
    init();
}, 0);

// **************************
// **************************
// ** Send Client Messages **
// **************************
// **************************
function sockSend(obj) {
    const str = JSON.stringify(obj);
    socket.send(str);
}

function sockEvt(id, evtName) {
    sockSend({
        type: "HookCalled",
        id,
        evtName,
    });
}

function sockEvtArg(id, evtName, evtArg) {
    if (Object.prototype.toString.call(evtArg) !== "[object String]") {
        evtArg = evtArg.toString();
    }
    sockSend({
        type: "HookArgCalled",
        id,
        evtName,
        evtArg,
    });
}
