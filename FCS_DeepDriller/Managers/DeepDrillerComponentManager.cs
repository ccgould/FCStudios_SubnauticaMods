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
        private static bool _flag = true;

        public static GameObject MountingTarget { get; private set; }

        internal static bool FindAllComponents(FCSDeepDrillerController mono)
        {
            //Modules
            _mono = mono;
            _gameObject = mono.gameObject;

            MountingTarget = _gameObject.FindChild("Mount_Point")?.gameObject;
            if (MountingTarget == null)
            {
                QuickLogger.Error("Couldnt find GameObject Mount_Point");
                _flag = false;
                return false;
            }

            var model = _gameObject.FindChild("model")?.gameObject;
            if (model == null)
            {
                QuickLogger.Error("Couldnt find GameObject Model");
                _flag = false;
                return false;
            }

            #region Batteries
            var modules = model.FindChild("modules")?.gameObject;
            if (modules == null)
            {
                QuickLogger.Error("Couldnt find GameObject modules");
                _flag = false;
                return false;
            }

            _batteryModule = modules.FindChild("battery_module")?.gameObject;


            if (_batteryModule == null)
            {
                QuickLogger.Error("Couldnt find GameObject battery_module");
                _flag = false;
                return false;
            }

            var batCells = _batteryModule.FindChild("Bat_Cells")?.gameObject;
            if (batCells == null)
            {
                QuickLogger.Error("Couldnt find GameObject Bat_Cells");
                _flag = false;
                return false;
            }

            _batteryCell1 = batCells.FindChild("BatteryCell_1")?.gameObject;
            _batteryCell2 = batCells.FindChild("BatteryCell_2")?.gameObject;
            _batteryCell3 = batCells.FindChild("BatteryCell_3")?.gameObject;
            _batteryCell4 = batCells.FindChild("BatteryCell_4")?.gameObject;
            #endregion

            _solarPanelModule = modules.FindChild("solar_panel_module")?.gameObject;

            if (_solarPanelModule == null)
            {
                QuickLogger.Error("Couldnt find GameObject solar_panel_module");
                _flag = false;
                return false;
            }

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

        internal static void ShowAttachment(DeepDrillModules module)
        {
            if (!_flag) return;
            switch (module)
            {
                case DeepDrillModules.Solar:
                    _batteryModule.SetActive(false);
                    _solarPanelModule.SetActive(true);
                    break;

                case DeepDrillModules.Battery:
                    _batteryModule.SetActive(true);
                    _solarPanelModule.SetActive(false);
                    break;
            }
        }

        public static void HideAllAttachments()
        {
            if (!_flag) return;

            _batteryModule.SetActive(false);
            _solarPanelModule.SetActive(false);
        }

        public static void ShowAllAttachments()
        {
            if (!_flag) return;

            _batteryModule.SetActive(true);
            _solarPanelModule.SetActive(true);
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
            if (!_flag) return;

            _batteryCell1.SetActive(false);
            _batteryCell2.SetActive(false);
            _batteryCell3.SetActive(false);
            _batteryCell4.SetActive(false);
        }
    }
}
