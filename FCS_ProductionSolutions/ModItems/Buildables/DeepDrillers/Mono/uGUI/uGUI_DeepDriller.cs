using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DeepDriller : Page, IuGUIAdditionalPage
{
    [SerializeField]
    private uGUI_DDHomePage initialPage;
    private MenuController _menuController;

    private DrillSystem _sender;
    public IFCSObject GetController()
    {
        return _sender;
    }

    public override void Enter(object obj)
    {
        base.Enter(obj);

        if(_menuController is null )
        {
            _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
        }

        if (obj is not null)
        {
            _sender = obj as DrillSystem;

            _sender.OnBatteryLevelChange += OnBatteryLevelChange;
            _sender.OnOilLevelChange += OnOilLevelChange;
            _sender.GetDDContainer().OnContainerUpdate += RefreshInventory;
            FCSPDAController.Main.GetGUI().GetNavigationController().SetErrorButtonDevice(_sender);
        }
    }


    /// <summary>
    /// This method is called when push is completed for the Root Page
    /// </summary>
    public override void OnPushCompleted()
    {
        _menuController?.PushPage(initialPage, _sender);
    }

    public void PushPage(Page page)
    {
        _menuController.PushPage(page, _sender);
    }

    public void PopPage()
    {
        _menuController.PopAndPeek();
    }

    public override void Exit()
    {
        base.Exit();

        _sender.OnBatteryLevelChange -= OnBatteryLevelChange;
        _sender.OnOilLevelChange -= OnOilLevelChange;
        _sender.GetDDContainer().OnContainerUpdate -= RefreshInventory;
    }

    private void RefreshInventory(int i, int i1)
    {
        initialPage.RefreshInventory();
    }

    private void OnOilLevelChange(float obj)
    {
        initialPage.OnOilLevelChange(obj);
    }

    private void OnBatteryLevelChange(PowercellData obj)
    {
        initialPage.OnBatteryLevelChange(obj);
    }
}
