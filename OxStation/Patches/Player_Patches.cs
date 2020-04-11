using System.Collections.Generic;
using FCSCommon.Utilities;
using Harmony;
using MAC.OxStation.Config;
using MAC.OxStation.Managers;
using MAC.OxStation.Mono;
using UnityEngine;

namespace MAC.OxStation.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CanBreathe")]
    internal class Player_Patches
    {
        internal static float DefaultO2Level;
        private static readonly float _oxygenPerSecond = 10f;
        public static bool Prefix(ref Player __instance, ref bool __result)
        {
            GetDefaultO2Level(__instance);

            if (__instance.IsInBase())
            {
                if (__instance.IsUnderwater()) return true;

                var curBase = __instance.GetCurrentSub();

                var manager = BaseManager.FindManager(curBase);

                if (!IsThereAnyOxygenStationAttached(out __result, manager)) return false;

                if (PerformOxygenCheckForBases(__instance, out __result, manager)) return true;
            }
            return false;
        }

        private static bool IsThereAnyOxygenStationAttached(out bool outResult, BaseManager manager)
        {
            outResult = false;

            var unitsAvailable = manager.BaseUnits.Count > 0;

            if (!unitsAvailable)
            {
                return false;
            }
            
            return true;
        }

        private static bool PerformOxygenCheckForBases(Player instance, out bool outResult, BaseManager manager)
        {
            outResult = false;

            if (Mod.RTInstalled && Player.main.oxygenMgr.HasOxygenTank())
            {
                if (IsPlayerOxygenFullRtInstalled(instance)) return true;

                foreach (OxStationController baseUnit in manager.BaseUnits)
                {
                    if (baseUnit.OxygenManager.GetO2Level() <= 0 || !baseUnit.IsConstructed ||
                        baseUnit.HealthManager.IsDamageApplied()) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < DefaultO2Level)
                    {
                        var amount = _oxygenPerSecond * Time.deltaTime;
                        var result = baseUnit.OxygenManager.RemoveOxygen(amount);

                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                        }

                        outResult = true;
                    }

                    break;
                }
            }
            else
            {
                if (IsPlayerOxygenFull(instance)) return true;

                foreach (OxStationController baseUnit in manager.BaseUnits)
                {
                    if (baseUnit.OxygenManager.GetO2Level() <= 0 || !baseUnit.IsConstructed ||
                        baseUnit.HealthManager.IsDamageApplied()) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < Player.main.oxygenMgr.GetOxygenCapacity())
                    {
                        var amount = _oxygenPerSecond * Time.deltaTime;
                        var result = baseUnit.OxygenManager.RemoveOxygen(amount);

                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                        }

                        outResult = true;
                    }

                    break;
                }
            }

            return false;
        }

        private static bool IsPlayerOxygenFullRtInstalled(Player instance)
        {
            if (instance.oxygenMgr.GetOxygenAvailable() >= DefaultO2Level)
            {
                return true;
            }

            return false;
        }

        private static bool IsPlayerOxygenFull(Player instance)
        {
            return instance.oxygenMgr.GetOxygenAvailable() >= instance.oxygenMgr.GetOxygenCapacity();
        }

        private static void GetDefaultO2Level(Player instance)
        {
            if (DefaultO2Level <= 0)
            {
                var oxygen = instance.gameObject.GetComponentInParent<Oxygen>();

                if (oxygen == null)
                {
                    QuickLogger.Error("Failed to get player oxygen component");
                }
                else if (!oxygen.isPlayer)
                {
                    QuickLogger.Error("This oxygen component isnt the players");
                }
                else
                {
                    DefaultO2Level = oxygen.oxygenCapacity;
                    QuickLogger.Debug($"Default oxygen level is: {DefaultO2Level}");
                }
            }
        }
    }
}
