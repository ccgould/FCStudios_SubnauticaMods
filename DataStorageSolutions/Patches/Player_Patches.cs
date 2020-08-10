using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Update
    {
        private static float _timeLeft = 1f;

        [HarmonyPostfix]
        public static void Postfix(ref Player __instance)
        {
            _timeLeft -= DayNightCycle.main.deltaTime;
            if (_timeLeft < 0)
            {
                BaseManager.RemoveDestroyedBases();
                BaseManager.OnPlayerTick?.Invoke();
                _timeLeft = 1f;
            }
        }
    }
}
