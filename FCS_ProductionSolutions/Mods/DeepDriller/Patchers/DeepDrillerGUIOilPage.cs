using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUIOilPage : DeepDrillerGUIPage
    {
        internal BatteryMeterController LubeMeter;

        public override void Awake()
        {
            var backBtn = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            backBtn?.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Settings);
            }));

            LubeMeter = InterfaceHelpers.FindGameObject(gameObject, "LubeMeter")?.AddComponent<BatteryMeterController>();
            LubeMeter?.Initialize();

            var addLubricantBTN = GameObjectHelpers.FindGameObject(gameObject, "AddLubricantBTN")?.AddComponent<FCSButton>();
            addLubricantBTN.Subscribe(() =>
            {
                if (PlayerInteractionHelper.HasItem(TechType.Lubricant))
                {
                    if (Hud.GetSender().OilHandler.IsAllowedToAdd(TechType.Lubricant, false))
                    {
                        PlayerInteractionHelper.TakeItemFromInventory(TechType.Lubricant);
                        Hud.GetSender().OilHandler.ReplenishOil();
                    }
                    else
                    {
                        Hud.ShowMessage(FCSDeepDrillerBuildable.OilTankNotFormatEmpty(Hud.GetSender().OilHandler.TimeTilRefuel()));
                    }
                }
                else
                {
                    Hud.ShowMessage(FCSDeepDrillerBuildable.NoLubricantFound());
                }
            });
        }

        public override void Show()
        {
            base.Show();
            LubeMeter.UpdateStateByPercentage(Hud.GetSender().OilHandler.GetOilPercent());
        }
    }
}