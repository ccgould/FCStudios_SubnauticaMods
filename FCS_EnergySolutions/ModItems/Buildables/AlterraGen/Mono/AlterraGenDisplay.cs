using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Models.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Enumerators;
using FCSCommon.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Mono;
internal class AlterraGenDisplay : MonoBehaviour
{
    [SerializeField] private Text _powerStateValue;
    [SerializeField] private Text _powerUnitValue;
    [SerializeField] private Image _breakerStatusLight; 
    [SerializeField] private Text _itemCounter;
    [SerializeField] private BatteryMeterController batteryController;
    [SerializeField] private AlterraGenPowerManager powerManager;
    [SerializeField] private AlterraGenController controller;
    [SerializeField] private List<uGUI_StorageItem> screenItems;
    private int _pageHash;

    private void Awake()
    {
        UpdateItemCount(0, controller.GetMaxSlots());
        _pageHash = Animator.StringToHash("Page");
        InvokeRepeating(nameof(UpdateScreenOnLoad), 1f, 1f);
        InvokeRepeating(nameof(OnPowerUpdateCycle), 0.5f, 0.5f);
        RefreshItems();
    }

    private void Update()
    {
    }


    internal void RefreshItems()
    {
        foreach (var item in screenItems)
        {
            item.Reset();
        }

        var items = controller.GetStorgeItems();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items.ElementAt(i);
            screenItems[i].Set(item.Key,item.Value);
        }
    }

    private void UpdateScreenOnLoad()
    {
        switch (powerManager.PowerState)
        {
            case FCSPowerStates.Powered:
                GoToPage(AlterraGenPages.HomePage);
                break;
            case FCSPowerStates.None:
            case FCSPowerStates.UnPowered:
                GoToPage(AlterraGenPages.BlackOut);
                break;
            case FCSPowerStates.Tripped:
                GoToPage(AlterraGenPages.PoweredOffPage);
                break;
        }

        //if (GetCurrentPage() != AlterraGenPages.PoweredOffPage)
        //{
        //    if (powerManager.PowerState == FCSPowerStates.Tripped)
        //    {
        //        GoToPage(AlterraGenPages.PoweredOffPage);
        //    }
        //}

        //if (GetCurrentPage() == AlterraGenPages.PoweredOffPage)
        //{
        //    QuickLogger.Debug("Canceling Invoke Repeating", true);
        //    CancelInvoke(nameof(UpdateScreenOnLoad));
        //}
    }

    private void UpdateItemCount(int amount, int maxAmount)
    {
        _itemCounter.text = Language.main.GetFormat("AES_ItemCounterFormat", amount, maxAmount);
    }

    private void OnPowerUpdateCycle()
    {

        UpdateItemCount(controller.GetItemCount(), controller.GetMaxSlots());


        //Update the Breaker Status Light
        _breakerStatusLight.color = powerManager.ProducingPower ? Color.green : Color.red;

        //Update the Power State
        _powerStateValue.text = powerManager.ProducingPower ? Language.main.Get("BaseNuclearReactorActive") : Language.main.Get("BaseNuclearReactorInactive");

        //Update the Power Amount Stored
        _powerUnitValue.text = powerManager.GetTotalPowerString();        

        //Update Battery Fill
        batteryController.UpdateBatteryStatus(powerManager.GetBatteryData());
    }

    internal void GoToPage(AlterraGenPages page)
    {
        controller.GetAnimationManager().SetIntHash(_pageHash, (int)page);
    }

    internal AlterraGenPages GetCurrentPage()
    {
        return (AlterraGenPages)controller.GetAnimationManager().GetIntHash(_pageHash);
    }
}
