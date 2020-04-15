using Harmony;
using OxStationVehiclesModules.Configuration;

namespace OxStationVehiclesModules.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CanBreathe")]
    internal class Player_Patches
    {
        private static Player _player;

        public static bool Prefix(ref Player __instance, ref bool __result)
        {
            _player = Player.main;

            if (!_player.IsInBase() && !_player.IsInSubmarine() && (_player.IsInClawExosuit() || _player.IsPiloting()))
            {
                var curBase = __instance.currentMountedVehicle;

                if (!IsThereAnyOxStationModule(curBase))
                {
                    __result = false;
                    return false;
                }
            }

            return true; //return false to skip execution of the original.
        }

        private static bool IsThereAnyOxStationModule(Vehicle curSub)
        {
            if (curSub == null) return false;

            var oxStationCount = curSub.modules.GetCount(Mod.ModuleTechType);

            return oxStationCount > 0;
        }
    }
}
