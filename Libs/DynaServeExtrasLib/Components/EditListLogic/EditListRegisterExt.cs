using DynaServeExtrasLib.Components.DlgInputLogic;
using DynaServeExtrasLib.Components.FontAwesomeLogic;
using DynaServeLib;
using DynaServeLib.Serving.FileServing.StructsEnum;

namespace DynaServeExtrasLib.Components.EditListLogic;

public static class EditListRegisterExt
{
	public static void RegisterEditList(this ServOpt opt)
	{
		opt.RegisterFontAwesome();
		opt.RegisterDlgInput();
		opt.Serve(FCat.Css, "edit-list");
	}
}