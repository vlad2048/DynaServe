const httpLink = "{{HttpLink}}";


function cssCleanLink(link) {
	let charIdx = link.indexOf('?');
	if (charIdx !== -1)
		link = link.substring(0, charIdx);
	charIdx = link.indexOf('css/');
	if (charIdx !== -1)
		link = link.substring(charIdx);
	return link;
}

function cssGetWebLinkNodes() {
	const cssLinks = [];
	const domLinks = document.getElementsByTagName('link');
	for (let i = 0; i < domLinks.length; i++) {
		const domLink = domLinks[i];
		if (domLink.rel !== 'stylesheet') continue;
		if (!domLink.href.startsWith('file:///') && !domLink.href.startsWith(httpLink)) continue;
		cssLinks.push(domLink);
	}
	return cssLinks;
}

function cssGetWebLinks()
{
	return cssGetWebLinkNodes().map(e => cssCleanLink(e.href));
}

function cssRemoveWebLink(lnk) {
	cssGetWebLinkNodes().filter(e => cssCleanLink(e.href) === lnk).forEach(e => e.remove());
}

function cssAddWebLink(lnk) {
	const head = document.getElementsByTagName('head')[0];
	const webLink = document.createElement('link');
	webLink.rel = 'stylesheet';
	webLink.type = 'text/css';
	webLink.href = lnk;
	head.appendChild(webLink);
}

function cssNormalizeAllWebLinks() {
	cssGetWebLinkNodes().forEach(e => e.href = cssCleanLink(e.href));
}
