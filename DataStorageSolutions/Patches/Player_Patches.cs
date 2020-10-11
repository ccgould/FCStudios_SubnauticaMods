using System;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Update
    {
        private static float _timeLeft = 1f;
        private static bool _error;
        private static bool _wasSet;

        [HarmonyPostfix]
        public static void Postfix(ref Player __instance)
        {
            try
            {
                _timeLeft -= DayNightCycle.main.deltaTime;
                if (_timeLeft < 0)
                {
                    BaseManager.RemoveDestroyedBases();
                    BaseManager.OnPlayerTick?.Invoke(__instance);
                    
                    //QuickLogger.Debug($"Is Ready: {LargeWorldStreamer.main.IsReady()}");
                    //QuickLogger.Debug($"Is World Settled: {LargeWorldStreamer.main.IsWorldSettled()}");

                    if (LargeWorldStreamer.main.IsReady() && !_wasSet)
                    {
                        BaseManager.SetAllowedToNotify(true);
                        _wasSet = true;
                    }

                    //BaseManager.PerformOperations();
                    //BaseManager.PerformCraft();
                    _timeLeft = 1f;
                }
            }
            catch (Exception e)
            {
                if (!_error)
                {
                    QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
                    _error = true;
                }
            }
        }
    }
}
