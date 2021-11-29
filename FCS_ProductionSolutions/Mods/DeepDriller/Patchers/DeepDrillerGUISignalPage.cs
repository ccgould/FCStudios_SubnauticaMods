using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    public class DeepDrillerGUISignalPage : DeepDrillerGUIPage
    {
        private FCSButton _toggleBTN;
        private FCSButton _editNameBTN;
        private Text _pingInformation;

        public override void Awake()
        {
            _pingInformation = GameObjectHelpers.FindGameObject(gameObject, "Information").GetComponent<Text>();


            var backBtn = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            backBtn?.onClick.AddListener((() =>
            {
                Hud.GoToPage(DeepDrillerHudPages.Settings);
            }));

            _toggleBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleIsVisibleBTN").AddComponent<FCSButton>();
            _toggleBTN.TextLineOne = "Go to moonpool configuration.";
            _toggleBTN.Subscribe((state) =>
            {
                Hud.GetSender().ToggleVisibility(state);

            });

            _editNameBTN = GameObjectHelpers.FindGameObject(gameObject, "EditNameButton").AddComponent<FCSButton>();
            _editNameBTN.TextLineOne = "Go to moonpool configuration.";
            _editNameBTN.Subscribe(() =>
            {
                uGUI.main.userInput.RequestString("Enter Beacon Name", "Rename", _pingInformation.text, 100,
                    (text =>
                    {
                        try
                        {
                            UpdateBeaconName(text);
                            Hud.Show(Hud.GetSender());
                            Hud.GoToPage(DeepDrillerHudPages.Beacon);
                        }
                        catch (Exception e)
                        {
                            QuickLogger.Error("Failed to rename drill beacon");
                            QuickLogger.Error(e.Message);
                            QuickLogger.Error(e.StackTrace);
                        }
                    }));
            });
        }

        public override void Show()
        {
            base.Show();

            if (Hud.GetSender().GetBeaconState())
            {
                _toggleBTN.Check();
            }
            else
            {
                _toggleBTN.UnCheck();
            }

            UpdateBeaconName(Hud.GetSender().GetPingName(),false);
        }

        internal void UpdateBeaconName(string beaconName = null, bool informController = true)
        {
            if (string.IsNullOrWhiteSpace(beaconName))
            {
                var defaultName = $"Deep Driller - {Hud.GetSender().UnitID}";
                if(informController) Hud.GetSender().SetPingName(defaultName);
                _pingInformation.text = defaultName;
            }
            else
            {
                if (informController) Hud.GetSender().SetPingName(beaconName);
                _pingInformation.text = beaconName;
            }
        }
    }
}