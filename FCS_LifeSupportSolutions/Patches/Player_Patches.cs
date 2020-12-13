using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Mono;
using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    internal class Player_Awake
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            __instance.gameObject.EnsureComponent<PlayerAdrenaline>();
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Update
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            if (uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD != null)
            {
                uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD.ToggleVisibility();
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CanBreathe")]
    internal class Player_CanBreathe
    {
        internal static float DefaultO2Level;

        private static readonly float _oxygenPerSecond = 10f;
        
        public static bool Prefix(ref Player __instance, ref bool __result)
        {
            if (!QPatch.BaseUtilityUnitConfiguration.AffectPlayerOxygen|| !QPatch.BaseUtilityUnitConfiguration.IsModEnabled) return true;

            GetDefaultO2Level(__instance);

            if (__instance.IsInBase())
            {
                if (__instance.IsUnderwater()) return true;

                var curBase = __instance.GetCurrentSub();

                var manager = BaseManager.FindManager(curBase);

                if (!IsThereAnyBaseUtilityUnitAttached(out __result, manager)) return false;

                PerformOxygenCheckForBases(__instance, out __result, manager);

                return false;
            }

            return true; //return false to skip execution of the original.
        }

        private static bool IsThereAnyOxStationModule(Vehicle curSub)
        {
            if (curSub == null) return false;

            var oxStationCount = curSub.modules.GetCount(TechType.SeamothSonarModule);

            if (oxStationCount <= 0)
            {
                return false;
            }

            return true;
        }

        private static bool IsThereAnyBaseUtilityUnitAttached(out bool outResult, BaseManager manager)
        {
            outResult = false;

            var unitsAvailable = manager.GetDevicesCount(Mod.BaseUtilityUnitTabID) > 0;

            if (!unitsAvailable)
            {
                return false;
            }

            return true;
        }

        private static bool PerformOxygenCheckForBases(Player instance, out bool outResult, BaseManager manager)
        {
            outResult = false;
            var baseUtilityUnits = manager.GetDevices(Mod.BaseUtilityUnitTabID);

            QuickLogger.Debug($"Amount Base Utility : {baseUtilityUnits.Count()}",true);

            if (QPatch.IsRefillableOxygenTanksInstalled && Player.main.oxygenMgr.HasOxygenTank())
            {
                if (IsPlayerOxygenFullRtInstalled(instance)) return false;
                
                QuickLogger.Debug("Passed RTTest",true);

                foreach (var baseUnit in baseUtilityUnits)
                {

                    var utility = (BaseUtilityUnitController) baseUnit;

                    if (utility.OxygenManager.GetO2Level() <= 0 || !utility.IsOperational) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < DefaultO2Level)
                    {
                        var amount = _oxygenPerSecond * Time.deltaTime;
                        var result = utility.OxygenManager.RemoveOxygen(amount);

                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                            outResult = true;
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (IsPlayerOxygenFull(instance)) return true;

                foreach (var baseUnit in baseUtilityUnits)
                {
                    var utility = (BaseUtilityUnitController)baseUnit;

                    if (utility.OxygenManager.GetO2Level() <= 0 || !baseUnit.IsOperational) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < Player.main.oxygenMgr.GetOxygenCapacity())
                    {
                        var amount = _oxygenPerSecond * DayNightCycle.main.deltaTime;
                        var result = utility.OxygenManager.RemoveOxygen(amount);
                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                            outResult = true;
                            return true;
                        }
                    }
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
