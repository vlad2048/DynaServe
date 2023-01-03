<Query Kind="Program">
  <Namespace>LINQPad.Controls</Namespace>
</Query>

const string ImgV1 = @"C:\tmp\ImagePad\ver_1\img.png";
const string ImgV2 = @"C:\tmp\ImagePad\ver_2\img.png";

const string LiveImg = @"C:\Dev_Nuget\Libs\DynaServe\Play\ExtrasPlay\serv\demo-livereload\img.png";
const string LiveCode = @"C:\Dev_Nuget\Libs\DynaServe\Play\ExtrasPlay\serv\demo-livereload\code.js";

static int CodeIdx = 22;

void Main()
{
	new Button("Inc Code", _ => File.WriteAllText(LiveCode, $$"""
		function myFun() {
			console.log('Hey {{CodeIdx++}}');
		}
		"""
	)).Dump();
	new Button("Img V1", _ => File.Copy(ImgV1, LiveImg, true)).Dump();
	new Button("Img V2", _ => File.Copy(ImgV2, LiveImg, true)).Dump();
}


