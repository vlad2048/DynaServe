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
				cssSyncRemove: [],
				cssSyncAdd: [],
				nodeId: '',
				scriptLink: '',
				cssLinkRefresh: '',
				attrKey: '',
				attrVal: '',
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
					break;
				}

				case 'ReplaceChildren':
				{
					const elt = document.getElementById(data.nodeId);
					if (!!elt)
						elt.innerHTML = data.html;
					break;
				}

				case 'AddScriptCss':
				{
					const head = document.getElementsByTagName('head')[0];
					const lnk = document.createElement('link');
					lnk.rel = 'stylesheet';
					lnk.type = 'text/css';
					lnk.href = data.scriptLink;
					head.appendChild(lnk);
					break;
				}

				case 'AddScriptJs':
				{
					const head = document.getElementsByTagName('head')[0];
					const lnk = document.createElement('script');
					lnk.source = data.scriptLink;
					head.appendChild(lnk);
					break;
				}

				case 'RefreshCss':
				{
					const cssLinkRefresh = data.cssLinkRefresh;
					cssGetWebLinkNodes()
						.filter(e => cssCleanLink(e.href) === cssCleanLink(cssLinkRefresh))
						.forEach(e => {
							/*console.log(' ');
							console.log('Refresh');
							console.log(`  '${e.href}'`);
							console.log('with');
							console.log(`  '${cssLinkRefresh}'`);*/
							e.href = cssLinkRefresh;
						});
					break;
				}

				case 'SetAttr':
				{
					const elt = document.getElementById(data.nodeId);
					if (!!elt)
						elt[data.attrKey] = data.attrVal;
					break;
				}

				case 'ReqCallMethodOnNode':
				{
					const elt = document.getElementById(data.nodeId);
					console.log(`calling ${data.methodName} on ${data.nodeId}`);
					elt[data.methodName]();
					console.log('after calling');
					break;
				}
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
	
