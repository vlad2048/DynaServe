var socket = null;

function init() {
	const socketUrl = "{{WSLink}}";
	const statusEltId = '{{StatusEltId}}';

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
				type: 'ReqCssSync',
				cssLinks: cssGetWebLinks()
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
				propChanges: [],
				cssSyncRemove: [],
				cssSyncAdd: [],
				nodeId: '',
				cssLinkRefresh: '',
				methodName: ''
			}
// ReSharper restore AssignedValueIsNeverUsed
			data = JSON.parse(e.data);

			//console.log(`RECEIVED: ${data.type}`);


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




				case 'PropChangesDomUpdate':
				{
					for (let chg of data.propChanges) {
						const node = document.getElementById(chg.nodeId);
						switch (chg.type) {
							case 'Attr':
								if (chg.attrName === 'class') {
									node.className = chg.attrVal;
								/*} else if (chg.attrName === 'autofocus') {
									setTimeout(() => {
										console.log(`focus on ${node.id}`);
										node.focus();
									}, 10);*/
								} else {
									if (chg.attrVal === undefined || chg.attrVal === null)
										node.removeAttribute(chg.attrName);
									else
										node.setAttribute(chg.attrName, chg.attrVal);
								}
								break;
							case 'Text':
								node.innerText = chg.textVal;
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



				case 'CssSync':
				{
					cssNormalizeAllWebLinks();
					for (let lnk of data.cssSyncRemove)
						cssRemoveWebLink(lnk);
					for (let lnk of data.cssSyncAdd)
						cssAddWebLink(lnk);
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

				case 'RefreshCss':
				{
					const cssLinkRefresh = data.cssLinkRefresh;
					cssGetWebLinkNodes()
						.filter(e => cssCleanLink(e.href) === cssCleanLink(cssLinkRefresh))
						.forEach(e => e.href = cssLinkRefresh);
					break;
				}

				case 'ReqCallMethodOnNode':
				{
					const elt = document.getElementById(data.nodeId);
					//console.log(`calling ${data.methodName} on ${data.nodeId}`);
					elt[data.methodName]();
					//console.log('after calling');
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
		//console.error(`Argument for ${evtName} in ${id} is not of type string`);
		//console.error(`Argument is (${typeof evtArg})`, evtArg);
		evtArg = evtArg.toString();
	}
	sockSend({
		type: 'HookArgCalled',
		id,
		evtName,
		evtArg
	});
}
	
