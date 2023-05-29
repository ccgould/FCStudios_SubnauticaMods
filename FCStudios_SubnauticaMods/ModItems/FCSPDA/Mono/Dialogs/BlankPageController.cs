using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

public class BlankPageController : Page
{
    public override PDAPages PageType => PDAPages.None;

    public override void OnBackButtonClicked()
    {

    }
}