using DynaServeExtrasLib.Components.DlgInputLogic;
using DynaServeExtrasLib.Components.FontAwesomeLogic;
using DynaServeExtrasLib.Utils;
using DynaServeLib;

namespace DynaServeExtrasLib.Components.EditListLogic;

public static class EditListRegisterExt
{
	public static void RegisterEditList(this ServOpt opt)
	{
		opt.RegisterFontAwesome();
		opt.RegisterDlgInput();
		opt.AddDynaServeExtraCssFolder("edit-list");
	}
}