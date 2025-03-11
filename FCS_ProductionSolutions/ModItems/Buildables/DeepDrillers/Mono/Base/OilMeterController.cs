using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
internal class OilMeterController : BatteryMeterController,  IPointerHoverHandler, IPointerExitHandler
{
    [SerializeField] private Button addOilButton;  
    [SerializeField] private Text unitID;
    private DeepDrillerHeavyDutyController controller;    
    private FCSDeepDrillerOilHandler oilHandler;



    public void Set(DeepDrillerHeavyDutyController ddcontroller, FCSDeepDrillerOilHandler oilHandler)
    {
        controller = ddcontroller;
        this.oilHandler = oilHandler;
        unitID.text = ddcontroller.UnitID;
    }


    public void AddLubricant()
    {
        if (PlayerInteractionHelper.HasItem(TechType.Lubricant))
        {
            if (oilHandler.IsAllowedToAdd(TechType.Lubricant, false))
            {
                PlayerInteractionHelper.TakeItemFromInventory(TechType.Lubricant);
                oilHandler.ReplenishOil();
                UpdateStateByPercentage(oilHandler.GetOilPercent());
            }
            else
            {
                FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.OilTankNotFormatEmpty(oilHandler.TimeTilRefuel()));
            }
        }
        else
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(AuxPatchers.NoLubricantFound());
        }
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        if(!addOilButton.isActiveAndEnabled)
        {
            addOilButton.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (addOilButton.isActiveAndEnabled)
        {
            addOilButton.gameObject.SetActive(false);
        }
    }
}
