using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Mono;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCSPowerStorage.Managers
{
    internal class BaseManager
    {
        internal static readonly List<BaseManager> Managers = new List<BaseManager>();
        internal readonly List<FCSPowerStorageController> PowerStorageUnits = new List<FCSPowerStorageController>();
        internal readonly List<FCSPowerStorageController> BasePowerStorageUnits = new List<FCSPowerStorageController>();
        public readonly int InstanceID;
        public readonly SubRoot Habitat;
        private PowerRelay _connectedRelay;

        public BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
            _connectedRelay = habitat.powerRelay;
            var mono = habitat.gameObject.GetComponent<MonoBehaviour>();
            mono.StartCoroutine(AutoSystem());
        }

        public static BaseManager FindManager(SubRoot subRoot)
        {
            if (!subRoot.isBase || subRoot.isCyclops) return null;

            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()}");

            var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

            return manager ?? CreateNewManager(subRoot);
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
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
                    var powerTap = PowerStorageUnits[0].PowerManager;
                    var sum = GetSum();
                    var basePower = powerTap.GetBasePower() - sum;


                    var activationTarget = LoadData.BatteryConfiguration.AutoActivateAt;

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
                                QuickLogger.Debug($"Unit {controller.GetPrefabID()}", true);
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
                QuickLogger.Debug($"Removed Power Storage: {powerStorageUnit.GetPrefabID()}", true);
            }
        }

        internal void AddPowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            if (!PowerStorageUnits.Contains(powerStorageUnit) && powerStorageUnit.IsConstructed)
            {
                PowerStorageUnits.Add(powerStorageUnit);
                QuickLogger.Debug($"Add Power Storage Unit : {powerStorageUnit.GetPrefabID()}", true);
            }
        }

        internal void AddBasePowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            if (!BasePowerStorageUnits.Contains(powerStorageUnit) && powerStorageUnit.IsConstructed)
            {
                BasePowerStorageUnits.Add(powerStorageUnit);
                QuickLogger.Debug($"Add Power Storage Unit to Base List : {powerStorageUnit.GetPrefabID()}", true);
            }
        }

        internal void SyncUnits(FCSPowerStates powerState, PowerToggleStates toggleState, bool auto)
        {
            foreach (FCSPowerStorageController controller in BasePowerStorageUnits)
            {
                controller.PowerManager.SetAutoActivate(auto);
                controller.PowerManager.SetChargeMode(toggleState);
                controller.PowerManager.SetPowerState(powerState);
            }
        }

        public static void RemoveBasePowerStorage(FCSPowerStorageController powerStorageUnit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BasePowerStorageUnits.Contains(powerStorageUnit)) continue;
                manager.BasePowerStorageUnits.Remove(powerStorageUnit);
                QuickLogger.Debug($"Removed Base Power Storage : {powerStorageUnit.GetPrefabID()}", true);
            }
        }
    }
}
