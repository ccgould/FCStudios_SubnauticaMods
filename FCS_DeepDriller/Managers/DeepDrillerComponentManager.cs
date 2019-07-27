using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Managers
{
    internal static class DeepDrillerComponentManager
    {
        private static GameObject _batteryCell1;
        private static GameObject _batteryCell2;
        private static GameObject _batteryCell3;
        private static GameObject _batteryCell4;
        private static GameObject _batteryModule;
        private static GameObject _solarPanelModule;
        private static GameObject _gameObject;
        private static FCSDeepDrillerController _mono;
        private static GameObject _focusModule;
        private static GameObject _digState;
        private static GameObject _hardLava;


        internal static bool FindAllComponents(FCSDeepDrillerController mono, GameObject solar, GameObject battery, GameObject focus)
        {
            //Modules
            _mono = mono;
            _gameObject = mono.gameObject;
            //_focusModule = focus;
            _solarPanelModule = solar;

            var model = _gameObject.FindChild("model")?.gameObject;
            if (model == null)
            {
                QuickLogger.Error("Couldnt find GameObject Model");
                return false;
            }

            _focusModule = model.FindChild("Scanner_Screen_Attachment")?.gameObject;
            if (_focusModule == null)
            {
                QuickLogger.Error("Couldnt find GameObject Scanner_Screen_Attachment");
                return false;
            }



            _focusModule.SetActive(false);

            #region Batteries

            _batteryModule = battery;

            var batCells = battery.FindChild("Bat_Cells")?.gameObject;
            if (batCells == null)
            {
                QuickLogger.Error("Couldnt find GameObject Bat_Cells");
                return false;
            }

            _batteryCell1 = batCells.FindChild("BatteryCell_1")?.gameObject;
            _batteryCell2 = batCells.FindChild("BatteryCell_2")?.gameObject;
            _batteryCell3 = batCells.FindChild("BatteryCell_3")?.gameObject;
            _batteryCell4 = batCells.FindChild("BatteryCell_4")?.gameObject;
            #endregion

            var lavaGroup = _gameObject.FindChild("model").FindChild("extra_models")?.gameObject;

            if (lavaGroup == null)
            {
                QuickLogger.Debug("Couldn't find extra_models");
            }

            _digState = lavaGroup.FindChild("DigState")?.gameObject;
            _hardLava = lavaGroup.FindChild("HardLava")?.gameObject;

            return true;
        }

        internal static GameObject GetBatteryCellModel(int slot)
        {
            GameObject go = null;

            switch (slot)
            {
                case 1:
                    go = _batteryCell1;
                    break;
                case 2:
                    go = _batteryCell2;
                    break;
                case 3:
                    go = _batteryCell3;
                    break;
                case 4:
                    go = _batteryCell4;
                    break;
            }

            return go;
        }

        internal static GameObject GetMount(FCSDeepDrillerController mono, DeepDrillerMountSpot screen)
        {
            switch (screen)
            {
                case DeepDrillerMountSpot.PowerSupply:
                    return mono.gameObject.FindChild("Mount_Point")?.gameObject;

                case DeepDrillerMountSpot.Screen:
                    return mono.gameObject.FindChild("Screen_Mount_Point")?.gameObject;

                default:
                    return null;
            }
        }

        internal static bool IsLavaHot()
        {
            return _digState.activeSelf;
        }

        internal static void ShowAttachment(DeepDrillModules module)
        {
            switch (module)
            {
                case DeepDrillModules.Solar:
                    _solarPanelModule.SetActive(true);
                    break;

                case DeepDrillModules.Battery:
                    _batteryModule.SetActive(true);
                    break;

                case DeepDrillModules.Focus:
                    _focusModule.SetActive(true);
                    break;
            }
        }

        internal static void HideAllPowerAttachments()
        {
            _batteryModule.SetActive(false);
            _solarPanelModule.SetActive(false);
        }

        internal static void HideAllAttachments()
        {
            _batteryModule.SetActive(false);
            _solarPanelModule.SetActive(false);
            _focusModule.SetActive(false);
        }

        internal static void ShowAllAttachments()
        {
            _batteryModule.SetActive(true);
            _solarPanelModule.SetActive(true);
            _focusModule.SetActive(true);

        }

        public static void Setup()
        {
            ShowAllAttachments();
            FCSDeepDrillerBuildable.ApplyShaders(_gameObject);
            HideAllAttachments();
            HideAllBatteries();
        }

        private static void HideAllBatteries()
        {
            _batteryCell1.SetActive(false);
            _batteryCell2.SetActive(false);
            _batteryCell3.SetActive(false);
            _batteryCell4.SetActive(false);
        }

        public static void HideAttachment(DeepDrillModules module)
        {
            switch (module)
            {
                case DeepDrillModules.Solar:
                    _solarPanelModule.SetActive(false);
                    break;

                case DeepDrillModules.Battery:
                    _batteryModule.SetActive(false);
                    break;

                case DeepDrillModules.Focus:
                    _focusModule.SetActive(false);
                    break;
            }
        }
    }
}
