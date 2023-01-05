using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DynaServeLib.DynaLogic.DomUtils;
using DynaServeLib.Utils.Exts;
using PowRxVar;

namespace DynaServeLib.DynaLogic;


static class DomCreator
{
	public static (IHtmlDocument, IDisposable) Create(IReadOnlyList<string> extraHtmlNodes)
	{
		var d = new Disp();

		var doc = InitialHtml.Parse().D(d);
		doc.AddExtraHtmlNodes(extraHtmlNodes);

		return (doc, d);
	}


	private const string InitialHtml = """
		<!DOCTYPE html>
		<html>
			<head>
				<link rel="icon" type="image/png" href="image/creepy-icon.png" />
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
				<title>DynaServe</title>
				<link href='http://fonts.googleapis.com/css?family=Roboto:400,100,100italic,300,300italic,400italic,500,500italic,700,700italic,900italic,900' rel='stylesheet' type='text/css'>
			</head>
			<body id="body">
			</body>
		</html>
		""";
}
