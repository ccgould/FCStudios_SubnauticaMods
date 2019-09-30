namespace FCStudioDebugger.Patchers
{
    //[HarmonyPatch(typeof(Player))]
    //[HarmonyPatch("CanBreathe")]
    //internal class Player_Patches
    //{
    //    public static bool Prefix(ref Player __instance)
    //    {
    //        if (DebugMenu.main == null) return false;
    //        QuickLogger.Info($"Can breathe {DebugMenu.main._canBreathe }");
    //        if (__instance.currentSub != null)
    //        {
    //            return DebugMenu.main._canBreathe && __instance.currentSub.powerRelay != null && __instance.currentSub.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline && !__instance.IsUnderwater();
    //        }
    //        return !__instance.IsUnderwater();
    //    }
    //}
}
