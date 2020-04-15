using FCSCommon.Utilities;
using Harmony;
using MoreCyclopsUpgrades.API;
using OxStationCyclopsModule.Config;

namespace OxStationCyclopsModule.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CanBreathe")]
    internal class Player_Patches
    {
        private static Player _player;

        public static bool Prefix(ref Player __instance, ref bool __result)
        {
            _player = Player.main;

            if (!_player.IsInBase() && _player.IsInSub())
            {
                var curBase = __instance.currentSub;
                var isOxstationAvaliable = IsThereAnyOxStationModule(curBase);

                QuickLogger.Debug($"IsOxstationAvaliable: {isOxstationAvaliable} || TechType: {Mod.ModuleTechType}",true);

                if (!isOxstationAvaliable)
                {
                    __result = false;
                    return false;
                }
            }

            return true; //return false to skip execution of the original.
        }

        private static bool IsThereAnyOxStationModule(SubRoot curSub)
        {
            if (curSub == null) return false;
            
            return MCUServices.CrossMod.HasUpgradeInstalled(curSub, Mod.ModuleTechType);
        }
    }
}
