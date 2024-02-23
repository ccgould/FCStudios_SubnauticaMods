using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono.UGUI;

internal class uGUI_TelepowerPylon : Page, IuGUIAdditionalPage
{
    [SerializeField]
    private Page initialPage;
    private TelepowerPylonController _sender;
    [SerializeField] private Page homePage;
    [SerializeField] private Page mainPage;

    public void PushPage(Page page)
    {
        FCSPDAController.Main.GetGUI().GetMenuController().PushPage(page, _sender);
    }

    public void PopPage()
    {
        FCSPDAController.Main.GetGUI().GetMenuController().PopAndPeek();
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        var sender = arg as TelepowerPylonController;

        if(sender is not null) 
        {
            _sender = sender;

            if(_sender.GetTelepowerBaseManager().GetHasBeenSet())
            {
                initialPage = mainPage;
            }
            else
            {
                initialPage = homePage;
            }
        }
    }

    public override void OnPushCompleted()
    {
        FCSPDAController.Main.GetGUI().GetMenuController().PushPage(initialPage,_sender);
    }

    public IFCSObject GetController()
    {
        return _sender;
    }
}