using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDHomePage : Page, IuGUIAdditionalPage
{
    private DrillSystem _sender;
    [SerializeField] private BatteryMeterController _batteryMeterController;
    [SerializeField] private BatteryMeterController _lubeMeterController;
    [SerializeField] private uGUI_DDInventoryPage _inventoryPage;
    [SerializeField] private uGUI_DDOilPage _lubricantPage;
    [SerializeField] private Text _oresPerDayAmount;
    [SerializeField] private Text _powerConsumptionAmount;
    [SerializeField] private Text _biomeLbl;
    [SerializeField] private Text _unitID;

    public override void Enter(object obj)
    {
        base.Enter(obj);


        if (obj is not null)
        {
            _sender = obj as DrillSystem;

            _unitID.text = _sender.UnitID;
            _oresPerDayAmount.text = _sender.GetOresPerDayCount();
            _powerConsumptionAmount.text = _sender.GetPowerUsageAmount();
            _biomeLbl.text = AuxPatchers.BiomeFormat(_sender.GetBiomeDetector().GetCurrentBiome());
        }

    }


    public IFCSObject GetController()
    {
        return _sender;
    }

    internal void RefreshInventory()
    {
        _inventoryPage.Refresh();
    }

    internal void OnOilLevelChange(float obj)
    {
        _lubeMeterController.UpdateStateByPercentage(obj);
        _lubricantPage.UpdateMeter(obj);
    }

    internal void OnBatteryLevelChange(PowercellData obj)
    {
        _batteryMeterController.UpdateBatteryStatus(obj);
    }
}
