using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Mono;
using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono;
using FCS_LifeSupportSolutions.Mods.OxygenTank.Mono;
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

            var unitsAvailable = manager.GetDevicesCount(Mod.BaseUtilityUnitTabID) + manager.GetDevicesCount(Mod.BaseOxygenTankTabID) > 0;

            if (!unitsAvailable)
            {
                return false;
            }

            return true;
        }

        private static void PerformOxygenCheckForBases(Player instance, out bool outResult, BaseManager manager)
        {
            outResult = false;
            float o2Available = instance.oxygenMgr.GetOxygenAvailable();
            float o2Capacity = QPatch.IsRefillableOxygenTanksInstalled ? DefaultO2Level : Player.main.oxygenMgr.GetOxygenCapacity();

            if (o2Available >= o2Capacity)
                return;

            if (TryAddOxygen(ref outResult, manager))
                return;

        }


        private static bool TryAddOxygen(ref bool outResult, BaseManager manager)
        {

            var baseUtilityUnits = manager.GetDevices(Mod.BaseUtilityUnitTabID);
            foreach (var baseUnit in baseUtilityUnits)
            {
                var utility = (BaseUtilityUnitController)baseUnit;

                if (utility.OxygenManager.GetO2Level() <= 0 || !utility.IsOperational)
                    continue;

                var amount = _oxygenPerSecond * DayNightCycle.main.deltaTime;
                var result = utility.OxygenManager.RemoveOxygen(amount);

                if (result)
                {
                    Player.main.oxygenMgr.AddOxygen(amount);
                    outResult = true;
                    return outResult;
                }
            }

            var baseOxygenTanks = manager.GetDevices(Mod.BaseOxygenTankTabID);
            bool hardcore = QPatch.BaseUtilityUnitConfiguration.SmallBaseOxygenHardcore;

            int bigRooms = 0;
            int smallRooms = 0;

            Base baseComponent = manager.Habitat.GetComponent<Base>();

            foreach(Int3 cell in baseComponent.AllCells)
            {
                Base.CellType cellType = baseComponent.GetCell(cell);

                switch (cellType)
                {
                    case Base.CellType.Corridor:
                        smallRooms += 1;
                        break;
                    case Base.CellType.MapRoom:
                        if (hardcore)
                            bigRooms += 1;
                        else
                            smallRooms += 1;
                        break;
                    case Base.CellType.MapRoomRotated:
                        if (hardcore)
                            bigRooms += 1;
                        else
                            smallRooms += 1;
                        break;
                    case Base.CellType.Moonpool:
                        bigRooms += 1;
                        break;
                    case Base.CellType.Room:
                        bigRooms += 1;
                        break;
                }
            }

            int RequiredTankCount = (bigRooms / (hardcore ? 2 : 1)) + (smallRooms / (hardcore ? 4:10)) + (hardcore? 1: 0);


            List<IPipeConnection> floaters = new List<IPipeConnection>();

            int ActiveTankCount = 0;
            foreach (var baseUnit in baseOxygenTanks)
            {
                var utility = baseUnit as BaseOxygenTankController;
                var rootFloater = utility.GetRootOxygenProvider();
                if (utility.IsOperational && rootFloater != null && rootFloater is PipeSurfaceFloater && rootFloater.GetProvidesOxygen() && !floaters.Contains(rootFloater))
                {
                    floaters.Add(rootFloater);
                    ActiveTankCount++;
                }
            }

            if (ActiveTankCount >= RequiredTankCount)
            {
                var amount = _oxygenPerSecond * DayNightCycle.main.deltaTime;
                Player.main.oxygenMgr.AddOxygen(amount);
                outResult = true;
            }
            return outResult;
        }


        private static void GetDefaultO2Level(Player instance)
        {
            if (DefaultO2Level <= 0)
            {
                var oxygen = instance.gameObject.GetComponentInParent<Oxygen>();

                if (oxygen == null || !oxygen.isPlayer) return;
                
                DefaultO2Level = oxygen.oxygenCapacity;
            }
        }
    }
}
