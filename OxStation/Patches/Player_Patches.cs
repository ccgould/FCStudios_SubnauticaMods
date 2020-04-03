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
            bool canBreathe = false;
            
            if (DefaultO2Level <= 0)
            {
                var oxygen = __instance.gameObject.GetComponentInParent<Oxygen>();
                
                if (oxygen == null )
                {
                    QuickLogger.Error("Failed to get player oxygen component");
                }
                else if(!oxygen.isPlayer)
                {
                    QuickLogger.Error("This oxygen component isnt the players");
                }
                else
                {
                    DefaultO2Level = oxygen.oxygenCapacity;
                    QuickLogger.Debug($"Default oxygen level is: {DefaultO2Level}");
                }
            }
            
            if (!__instance.IsInBase() || __instance.IsUnderwater()) return true;

            //QuickLogger.Debug($"Is InBase {__instance.IsInBase()}", true);

            var curBase = __instance.GetCurrentSub();

            var manager = BaseManager.FindManager(curBase);

            var unitsAvailable = manager.BaseUnits.Count > 0;

            if (!unitsAvailable)
            {
                //QuickLogger.Debug($"Can Breathe {canBreathe}", true);
                __result = canBreathe;
                return false;
            }

            QuickLogger.Debug($"RT_Installed: {Mod.RTInstalled} || HasOxygenTank: {Player.main.oxygenMgr.HasOxygenTank()}");

            if (Mod.RTInstalled && Player.main.oxygenMgr.HasOxygenTank())
            {
                //Check if the player oxygen level is full
                if (__instance.oxygenMgr.GetOxygenAvailable() >= DefaultO2Level)
                {
                    return true;
                }

                foreach (OxStationController baseUnit in manager.BaseUnits)
                {
                    if (baseUnit.OxygenManager.GetO2Level() <= 0 || !baseUnit.IsConstructed || baseUnit.HealthManager.IsDamageApplied()) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < DefaultO2Level)
                    {
                        var amount = _oxygenPerSecond * Time.deltaTime;
                        var result = baseUnit.OxygenManager.RemoveOxygen(amount);

                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                        }
                        canBreathe = true;
                    }
                    break;
                }
                QuickLogger.Debug($"Can Breathe Check 2 {canBreathe}", true);
                __result = canBreathe;
            }
            else
            {
                //Check if the player oxygen level is full
                if (__instance.oxygenMgr.GetOxygenAvailable() >= __instance.oxygenMgr.GetOxygenCapacity())
                {
                    return true;
                }

                foreach (OxStationController baseUnit in manager.BaseUnits)
                {
                    if (baseUnit.OxygenManager.GetO2Level() <= 0 || !baseUnit.IsConstructed || baseUnit.HealthManager.IsDamageApplied()) continue;

                    if (Player.main.oxygenMgr.GetOxygenAvailable() < Player.main.oxygenMgr.GetOxygenCapacity())
                    {
                        var amount = _oxygenPerSecond * Time.deltaTime;
                        var result = baseUnit.OxygenManager.RemoveOxygen(amount);

                        if (result)
                        {
                            Player.main.oxygenMgr.AddOxygen(amount);
                        }
                        canBreathe = true;
                    }
                    break;
                }
                //QuickLogger.Debug($"Can Breathe Check 2 {canBreathe}", true);
                __result = canBreathe;
            }

            
            return false;
        }
    }
}
