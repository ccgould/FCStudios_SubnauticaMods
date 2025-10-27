using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
internal class uGUI_DDHeavyOilPage : Page, IuGUIAdditionalPage
{
    //[SerializeField] private BatteryMeterController lubeMeter;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject oilMeterTemplate;

    private DeepDrillerOperatorController _sender;


    public void AddLubricant()
    {
        //if (PlayerInteractionHelper.HasItem(TechType.Lubricant))
        //{
        //    if (_sender.GetOilHandler().IsAllowedToAdd(TechType.Lubricant, false))
        //    {
        //        PlayerInteractionHelper.TakeItemFromInventory(TechType.Lubricant);
        //        _sender.GetOilHandler().ReplenishOil();
        //    }
        //    else
        //    {
        //        FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.OilTankNotFormatEmpty(_sender.GetOilHandler().TimeTilRefuel()));
        //    }
        //}
        //else
        //{
        //    FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.NoLubricantFound());
        //}
    }



    public override void Enter(object arg = null)
    {
        QuickLogger.Debug("Entering Oil Page", true);

        base.Enter(arg);

        _sender = arg as DeepDrillerOperatorController;

        if (_sender is not null)
        {
            QuickLogger.Debug($"Sender: {arg}", true); 

            var drills = _sender.GetConnectedDrills();

            QuickLogger.Debug($"Drills : {drills?.Count() ?? -1}", true);

            foreach (Transform oilMeter in scrollViewContent?.transform)
            {
                Destroy(oilMeter.gameObject);
            }

            foreach (var drill in drills)
            {
                QuickLogger.Debug($"Adding Oil Meter ",true);
                var oilHandler = drill.Value.GameObject.GetComponent<FCSDeepDrillerOilHandler>();
                var drillController = drill.Value.GameObject.GetComponent<DeepDrillerHeavyDutyController>();

                AddNewOilMeter(drillController, oilHandler);
                QuickLogger.Debug($"Added Oil Meter ", true);
            }
        }

        //lubeMeter.UpdateStateByPercentage(_sender.GetOilPercentage());
    }

    private void AddNewOilMeter(DeepDrillerHeavyDutyController drill, FCSDeepDrillerOilHandler oilHandler)
    {
        var template = Instantiate(oilMeterTemplate);
        var controller = template.GetComponent<OilMeterController>();
        controller.Set(drill, oilHandler);
        template.transform.SetParent(scrollViewContent.transform, false);
        template.SetActive(true);
        controller.UpdateStateByPercentage(drill.GetOilPercent());
    }

    public IFCSObject GetController()
    {
        return _sender;
    }

    internal void UpdateMeter(float obj)
    {
        //lubeMeter.UpdateStateByPercentage(obj);
    }
}
