using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDOilPage : Page, IuGUIAdditionalPage
{
    [SerializeField] private BatteryMeterController lubeMeter;
    
    private DrillSystem _sender;


    public void AddLubricant()
    {
        if (PlayerInteractionHelper.HasItem(TechType.Lubricant))
        {
            if (_sender.GetOilHandler().IsAllowedToAdd(TechType.Lubricant, false))
            {
                PlayerInteractionHelper.TakeItemFromInventory(TechType.Lubricant);
                _sender.GetOilHandler().ReplenishOil();
            }
            else
            {
                FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.OilTankNotFormatEmpty(_sender.GetOilHandler().TimeTilRefuel()));
            }
        }
        else
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.NoLubricantFound());
        }
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        _sender = arg as DrillSystem;
        lubeMeter.UpdateStateByPercentage(_sender.GetOilPercentage());
    }

    public IFCSObject GetController()
    {
        return _sender;
    }

    internal void UpdateMeter(float obj)
    {
        lubeMeter.UpdateStateByPercentage(obj);
    }
}
