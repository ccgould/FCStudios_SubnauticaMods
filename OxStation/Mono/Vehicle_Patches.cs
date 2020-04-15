using Harmony;

namespace OxStationVehiclesModules.Patches
{
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("ReplenishOxygen")]
    internal class Vehicle_Patches
    {
        private static Player _player;

        public static bool Prefix(ref Vehicle __instance)
        {
            _player = Player.main;
            if (!_player.IsInSubmarine() && (_player.IsInClawExosuit() || _player.IsPiloting()))
            {
                return false;
            }
            return true;
        }
    }
}

