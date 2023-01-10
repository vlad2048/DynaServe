export class DomUtils {
	private static readonly statusEltId = "{{StatusEltId}}";


	static replaceBody(html: string): void {
		// Empty the body (except for #syncserv-status)
		const body = document.getElementsByTagName("body")[0];
		for (let i = body.children.length - 1; i >= 0; i--) {
			const child = body.children[i];
			if (child.id !== DomUtils.statusEltId) child.remove();
		}
		// Add the dynamic nodes to body
		body.innerHTML += html;
	}


	// ****************
	// * FixAutoFocus *
	// ****************
	static fixAutofocus(): void {
		const autofocusElt = document.querySelector<HTMLElement>("[autofocus]");
		if (!!autofocusElt) {
			autofocusElt.focus();
		}
	}



	// *************
	// * ShowError *
	// *************
	static showError(err: Error): string {
		if (!err) return;
		const errorPanelId = 'error-panel';

		var div = document.getElementById(errorPanelId);
		if (!div) {
			const body = document.body;
			div = document.createElement('div');
			div.id = errorPanelId;
			div.style.position = 'fixed';
			div.style.left = '20px';
			div.style.top = '10px';
			div.style.width = '90%';
			div.style.height = '80%';
			div.style.padding = '5px';
			div.style.border = '2px solid black';
			div.style.backgroundColor = '#333C';
			div.style.color = '#FFF';
			div.style.zIndex = '20';
			body.append(div);
		}
		div.innerText += `${err.message}\n${JSON.stringify(err)}\n\n`;
		div.style.left = '20px';
	}

}