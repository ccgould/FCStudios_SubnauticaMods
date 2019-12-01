using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Mono;
using Oculus.Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Enums;
using UnityEngine;

namespace FCSPowerStorage.Managers
{
    internal class BaseManager
    {

        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal int InstanceID { get; }

        internal readonly List<FCSPowerStorageController> PowerStorageUnits = new List<FCSPowerStorageController>();
        internal readonly List<FCSPowerStorageController> BasePowerStorageUnits = new List<FCSPowerStorageController>();
        internal readonly SubRoot Habitat;

        /// <summary>
        /// Saves all the bases settings
        /// </summary>
        internal static void SaveBases()
        {
            QuickLogger.Debug("Save Bases");
            var saveDirectory = Information.GetSaveFileDirectory();
            var SaveFile = Path.Combine(saveDirectory, "Bases.json");

            QuickLogger.Debug($"SD {saveDirectory} || SF {SaveFile}");

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);


            var output = JsonConvert.SerializeObject(Managers, Formatting.Indented);
            File.WriteAllText(SaveFile, output);
            LoadData.CleanOldSaveData();
        }

        public BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
            var mono = habitat.gameObject.GetComponent<MonoBehaviour>();
            mono.StartCoroutine(AutoSystem());
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {

            //if (!subRoot.isBase) return null; //Disabled to allow Cyclops Operation

            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");

            var pre = subRoot.gameObject.GetComponent<PrefabIdentifier>();

            var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

            if (manager == null)
            {
                QuickLogger.Debug("No manager found on base");
            }

            return manager ?? CreateNewManager(subRoot);
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
        }

        private IEnumerator AutoSystem()
        {
            while (true)
            {
                QuickLogger.Debug($"PSCount {PowerStorageUnits.Count} || BPS {BasePowerStorageUnits.Count}");

                if (PowerStorageUnits.Count > 0)
                {
                    var fController = PowerStorageUnits[0];
                    var powerTap = fController.PowerManager;
                    var sum = GetSum();
                    var basePower = powerTap.GetBasePower() - sum;


                    var activationTarget = fController.GetAutoActivateAt();

                    QuickLogger.Debug($"Sum: {sum} || Base Power {basePower}");
                    QuickLogger.Debug($"Activation Target: {activationTarget} || Base Power {powerTap.GetBasePower()}");


                    if (basePower > activationTarget)
                    {
                        foreach (FCSPowerStorageController controller in PowerStorageUnits)
                        {
                            var unit = controller.PowerManager;

                            if (unit == null || unit.GetPowerState() == FCSPowerStates.Unpowered) continue;

                            unit.SetChargeMode(PowerToggleStates.ChargeMode);

                            QuickLogger.Debug("Auto Charge Power Storage", true);
                        }
                    }
                    else if (basePower <= activationTarget)
                    {

                        foreach (FCSPowerStorageController controller in PowerStorageUnits)
                        {
                            var unit = controller.PowerManager;

                            if (unit != null)
                            {
                                QuickLogger.Debug($"Unit {controller.GetPrefabIDString()}", true);
                                var chargeMode = unit.GetChargeMode();
                                var powerState = unit.GetPowerState();

                                if (powerState == FCSPowerStates.Unpowered) continue;

                                QuickLogger.Debug(unit.GetBasePower().ToString(), true);

                                if (chargeMode == PowerToggleStates.ChargeMode)
                                {
                                    QuickLogger.Debug("Auto Activating Power Storage", true);
                                    unit.SetChargeMode(PowerToggleStates.TrickleMode);
                                }
                            }
                            else
                            {
                                QuickLogger.Info("Unit is null continuing with operation.");
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }

        }

        private float GetSum()
        {
            float sum = 0;

            foreach (FCSPowerStorageController controller in PowerStorageUnits)
            {
                if (controller.PowerManager.GetPowerState() != FCSPowerStates.Unpowered &&
                    controller.PowerManager.GetChargeMode() == PowerToggleStates.TrickleMode)
                {
                    var unit = controller.PowerManager.GetPowerSum();
                    sum += unit;
                    QuickLogger.Debug($"Add To Sum {unit}");
                }

            }

            return sum;
        }

        internal static void RemovePowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.PowerStorageUnits.Contains(powerStorageUnit)) continue;
                manager.PowerStorageUnits.Remove(powerStorageUnit);
                QuickLogger.Debug($"Removed Power Storage: {powerStorageUnit.GetPrefabIDString()}", true);
            }
        }

        internal void AddPowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            if (!PowerStorageUnits.Contains(powerStorageUnit) && powerStorageUnit.IsConstructed)
            {
                PowerStorageUnits.Add(powerStorageUnit);
                QuickLogger.Debug($"Add Power Storage Unit : {powerStorageUnit.GetPrefabIDString()}", true);
            }
        }

        internal void AddBasePowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            if (!BasePowerStorageUnits.Contains(powerStorageUnit))
            {
                BasePowerStorageUnits.Add(powerStorageUnit);
                QuickLogger.Debug($"Add Power Storage Unit to Base List : {powerStorageUnit.GetPrefabIDString()}", true);
                QuickLogger.Debug($"Power Storage has been connected to base list Count {BasePowerStorageUnits.Count}", true);
            }
        }

        internal void SyncUnits(FCSPowerStates powerState, PowerToggleStates toggleState, bool auto, bool baseDrainState)
        {
            QuickLogger.Debug($"BPU Count = {BasePowerStorageUnits.Count}", true);

            foreach (FCSPowerStorageController controller in BasePowerStorageUnits)
            {
                if (controller.PowerManager.GetPowerState() == FCSPowerStates.Unpowered) continue;
                controller.SetAutoActivate(auto);
                controller.PowerManager.SetChargeMode(toggleState);
                controller.PowerManager.SetPowerState(powerState);
                controller.SetBaseDrainProtection(baseDrainState);
            }
        }

        internal static void RemoveBasePowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BasePowerStorageUnits.Contains(powerStorageUnit)) continue;
                manager.BasePowerStorageUnits.Remove(powerStorageUnit);
                QuickLogger.Debug($"Removed Base Power Storage : {powerStorageUnit.GetPrefabIDString()}", true);
            }
        }

        internal void UpdateBaseDrain(bool value)
        {
            foreach (FCSPowerStorageController controller in BasePowerStorageUnits)
            {
                controller.SetBaseDrainProtection(value);
            }
        }

        internal void UpdateTextBoxes(int getAutoActivateAt, int getBasePowerProtectionGoal)
        {
            foreach (FCSPowerStorageController controller in BasePowerStorageUnits)
            {
                controller.Display.UpdateTextBoxes(getAutoActivateAt, getBasePowerProtectionGoal);
            }
        }
    }
}
