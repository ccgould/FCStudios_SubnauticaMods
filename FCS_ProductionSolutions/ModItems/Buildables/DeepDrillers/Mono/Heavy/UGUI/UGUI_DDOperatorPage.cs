using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
internal class UGUI_DDOperatorPage : Page, IuGUIAdditionalPage
{
    [SerializeField] private Page initialPage;
    private DeepDrillerOperatorController _sender;
    private MenuController _menuController;

    private void Start()
    {
        _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        var sender = arg as DeepDrillerOperatorController;

        if (sender is not null)
        {
            _sender = sender;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public IFCSObject GetController()
    {
        return _sender;
    }

    public override void OnPushCompleted()
    {
        FCSPDAController.Main.GetGUI().GetMenuController().PushPage(initialPage, _sender);
    }

    public void PushPage(Page page)
    {
        _menuController.PushPage(page,_sender);
    }

    public void PopPage()
    {
        _menuController.PopAndPeek();
    }
}