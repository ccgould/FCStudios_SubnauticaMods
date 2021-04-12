using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Interfaces;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class HomePageController : MonoBehaviour, IPageController
    {
        private Text _status;
        private Text _info;
        
        internal void Initialize(DSSAutoCrafterDisplay display)
        {
            

            var manualBTN = GameObjectHelpers.FindGameObject(gameObject, "ManualBTN").GetComponent<Button>();
            var manualBtn = manualBTN.gameObject.AddComponent<FCSButton>();
            manualBtn.ShowMouseClick = true;
            manualBtn.TextLineOne = "Manual Operation Page.";
            manualBTN.onClick.AddListener(() =>
            {
                if (display.GetController().CraftManager.IsRunning() || display.GetController().CurrentCrafterMode == AutoCrafterMode.StandBy)
                {
                    display.GetController().ShowMessage("Cannot enter manual mode:\nPlease cancel any operations this crafter may be working on or turn off StandBy Mode.");
                    return;
                }
                display.GoToPage(AutoCrafterPages.Manual);
                display.GetController().SetManual();
            });

            var automaticBTN = GameObjectHelpers.FindGameObject(gameObject, "AutomatedBTN").GetComponent<Button>();
            var automaticBtn = automaticBTN.gameObject.AddComponent<FCSButton>();
            automaticBtn.ShowMouseClick = true;
            automaticBtn.TextLineOne = "Operations Page.";
            automaticBTN.onClick.AddListener((() =>
            {
                display.GoToPage(AutoCrafterPages.Automatic);
                if (display.GetController().CurrentCrafterMode != AutoCrafterMode.StandBy)
                {
                    display.GetController().SetAutomatic();
                }
            }));

            _info = GameObjectHelpers.FindGameObject(gameObject, "Info").GetComponent<Text>();

            _status = GameObjectHelpers.FindGameObject(gameObject, "Status").GetComponent<Text>();

            display.OnStatusUpdate += status => { _status.text = $"Status - {status}"; };

            display.OnLoadComplete += () => { _info.text = $"Auto Crafter UnitID - {display.GetController().UnitID}"; };

        }
        
        internal void Show()
        {
            gameObject.SetActive(true);
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            
        }
    }
}