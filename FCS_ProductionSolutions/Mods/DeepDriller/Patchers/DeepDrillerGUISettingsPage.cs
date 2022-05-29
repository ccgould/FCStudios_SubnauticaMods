using FCS_AlterraHub.Helpers;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUISettingsPage : DeepDrillerGUIPage
    {
        public override void Awake()
        {
            base.Awake();


            var filterBTN = GameObjectHelpers.FindGameObject(gameObject, "FilterBTN")?.GetComponent<Button>();
            filterBTN.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Filter);
            }));

            var powercellDrainBTN = GameObjectHelpers.FindGameObject(gameObject, "PowercellDrainBTN")?.GetComponent<Button>();
            powercellDrainBTN.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Power);
            }));


            var lubeRefillBTN = GameObjectHelpers.FindGameObject(gameObject, "LubeRefillBTN")?.GetComponent<Button>();
            lubeRefillBTN.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Oil);
            }));

            var pingSettingsBTN = GameObjectHelpers.FindGameObject(gameObject, "PingSettingsBTN")?.GetComponent<Button>();
            pingSettingsBTN.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Beacon);
            }));
        }

    }
}