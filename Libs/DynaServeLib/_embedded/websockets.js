var socket = null;

function init() {
	const socketUrl = "{{WSLink}}";
	const statusEltId = '{{StatusEltId}}';
	//const socketUrl = "ws://box-pc:7000/";
	//const statusEltId = 'syncserv-status';

	function updateState() {
		const elt = document.getElementById(statusEltId);
		if (!elt) return;
		switch (socket.readyState) {
			case WebSocket.CONNECTING: elt.innerText = 'CONNECTING'; break;
			case WebSocket.OPEN: elt.innerText = 'OPEN'; break;
			case WebSocket.CLOSING: elt.innerText = 'CLOSING'; break;
			case WebSocket.CLOSED: elt.innerText = 'CLOSED'; break;
			default: elt.innerText = `Unknown:${socket.readyState}`; break;
		}
	}

	function connectSocket() {
		socket = new WebSocket(socketUrl);

		socket.onerror = () => updateState();
		socket.onopen = () => {
			updateState();
			sockSend({
				type: 'ReqScriptsSync',
				reqScriptsSyncMsg: getReqScriptsSyncMsg()
			});
		}
		socket.onclose = () => {
			updateState();
			setTimeout(() => connectSocket(), 100);
		}

		socket.onmessage = e => {
// ReSharper disable AssignedValueIsNeverUsed
			let data = {
				type: '',
				html: '',
				replyScriptsSyncMsg: {},
				scriptRefreshNfo: {},
				chgs: [],
				nodeId: '',
				methodName: ''
			}
// ReSharper restore AssignedValueIsNeverUsed
			data = JSON.parse(e.data);

			console.log(`RECEIVED: ${data.type}`);


			// ****************************
			// ****************************
			// ** Handle Server Messages **
			// ****************************
			// ****************************
			switch (data.type) {
				case 'FullUpdate':
				{
					// Empty the body (except for #syncserv-status)
					const body = document.getElementsByTagName('body')[0];
					for (let i = body.children.length - 1; i >= 0; i--) {
						const child = body.children[i];
						if (child.id !== statusEltId)
							child.remove();
					}
					// Add the dynamic nodes to body
					body.innerHTML += data.html;
					break;
				}
					
				case 'ReplyScriptsSync':
				{
					handleReplyScriptsSync(data.replyScriptsSyncMsg);
          break;
				}

				case 'ScriptRefresh':
				{
					handleScriptRefresh(data.scriptRefreshNfo);
					break;
				}




				case 'ChgsDomUpdate':
				{
					for (let chg of data.chgs) {
            console.log(chg);
						const node = document.getElementById(chg.nodeId);
						switch (chg.type) {

							case 'Text':
								node.innerText = chg.val;
								break;

							case 'Attr':
								if (chg.name === 'class') {
									node.className = chg.val;
								} else {
									if (chg.val === undefined || chg.val === null)
										node.removeAttribute(chg.name);
									else
										node.setAttribute(chg.name, chg.val);
								}
								break;

              case 'Prop':
                switch (chg.propType) {
                  case 'Str':
                    node[chg.name] = chg.val;
                    break;
                  
                  case 'Bool':
                    node[chg.name] = !!chg.val;
                    break;
                  
                  default:
                    throw new Error(`Invalid PropType: ${chg.chg.propType}`)
                }
                //node[chg.propName] = chg.propVal;
                break;

							default:
								throw new Error(`Invalid PropChangeType: ${chg.type}`)
						}
					}
					break;
				}

				
				case 'ReplaceChildrenDomUpdate':
				{
					const elt = document.getElementById(data.nodeId);
					if (!!elt)
						elt.innerHTML = data.html;
					break;
				}



				case 'ReqDomSnapshot':
				{
					sockSend({
						type: 'AnswerDomSnapshot',
						clientDomSnapshot: {
							head: document.head.outerHTML,
							body: document.body.outerHTML,
						}
					});
					break;
				}




				case 'AddChildToBody':
				{
					const body = document.getElementsByTagName('body')[0];
					body.innerHTML += data.html;
					const autofocusElt = document.querySelector('[autofocus]');
					if (!!autofocusElt) {
						autofocusElt.focus();
					}
					break;
				}

				case 'RemoveChildFromBody':
				{
					var nodeId = data.nodeId;
					var node = document.getElementById(nodeId);
					if (!!node)
						node.remove();
					break;
				}

				case 'ReqCallMethodOnNode':
				{
					const elt = document.getElementById(data.nodeId);
					elt[data.methodName]();
					break;
				}

				default:
					throw new Error(`Invalid ServerMsgType: ${data.type}`);
			}
		}
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
		type: 'HookCalled',
		id,
		evtName
	});
}
	
function sockEvtArg(id, evtName, evtArg) {
	if (Object.prototype.toString.call(evtArg) !== "[object String]") {
		evtArg = evtArg.toString();
	}
	sockSend({
		type: 'HookArgCalled',
		id,
		evtName,
		evtArg
	});
}
	
