using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Navigation;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono.UGUI;
internal class uGUI_TelepowerPylonMainPage : Page
{
    [SerializeField] private Transform grid;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private uGUI_TelepowerPylon masterPage;
    [SerializeField] private Text _status;
     private List<uGUI_TelepowerManagerItem> _trackedToggles = new();

    private TelepowerPylonController _sender;

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        if(arg is not null)
        {
            _sender = arg as TelepowerPylonController;
            UpdateStatus();
        }

        LoadGrid();
    }

    private void UpdateStatus()
    {
        var baseManager = _sender.GetTelepowerBaseManager();
        var checkedAmount = _trackedToggles.Count(x=>x.IsChecked());
        _status.text = Language.main.GetFormat("AES_TelepowerStatus",baseManager.GetUpgrade(), checkedAmount,baseManager.GetMaxConnectionLimit(),_sender.GetTelepowerBaseManager().GetCurrentMode());
    }



    private void LoadGrid()
    {
        foreach(Transform item in grid)
        {
            if (item == itemTemplate) continue;

            Destroy(item.gameObject);
        }

        _trackedToggles.Clear();

        var managers =  BaseTelepowerPylonManager.GetGlobalTelePowerPylonsManagers();

        List<BaseTelepowerPylonManager> manager = new();

        switch (_sender.GetTelepowerBaseManager().GetCurrentMode())
        {
            case Enumerators.TelepowerPylonMode.RELAY:
            case Enumerators.TelepowerPylonMode.NONE:
                break;

            case Enumerators.TelepowerPylonMode.PULL:
                manager = managers.Where(x=>x.GetCurrentMode() == Enumerators.TelepowerPylonMode.PUSH).ToList();
                break;
            case Enumerators.TelepowerPylonMode.PUSH:
                manager = managers.Where(x => x.GetCurrentMode() == Enumerators.TelepowerPylonMode.PULL).ToList();
                break;
            default:
                break;
        }

        foreach ( var currentManagaer in manager)
        {
            
            if (currentManagaer == _sender.GetTelepowerBaseManager() || !currentManagaer.GetHasBeenSet()) continue;
            var g = Instantiate(itemTemplate);
            g.SetParent(grid,false);
            g.gameObject.SetActive(true);
            var component = g.GetComponent<uGUI_TelepowerManagerItem>();
            component.Set(currentManagaer, _sender.GetTelepowerBaseManager());
            _trackedToggles.Add(component);
        }
    }

    public void OnEndConnection()
    {
        FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("AES_PylonEndConnectionQuestion"),FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents.FCSMessageButton.YESNO,(result) =>
        {
            if(result == FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents.FCSMessageResult.OKYES)
            {
                masterPage.PopPage();
                _sender.SetMode(Enumerators.TelepowerPylonMode.NONE);
            }
        });
    }

    public void OnUpgradeBTNClicked()
    {
        _sender.OpenUpgradeContainer();
    }

}
