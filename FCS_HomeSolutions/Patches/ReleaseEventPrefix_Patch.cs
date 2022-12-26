using FCS_HomeSolutions.Mods.JukeBox.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_HomeSolutions.Patches
{
    internal static class ReleaseEventPrefix_Patch
    {
        [HarmonyPatch(typeof(FMOD_CustomEmitter), "ReleaseEvent")]
        [HarmonyPrefix]
        private static bool ReleaseEventPrefix(FMOD_CustomEmitter __instance)
        {
            if (__instance == null || !Main.Configuration.IsJukeBoxEnabled) return true;

            var baseJukeBox = Player.main.currentSub.gameObject.GetComponentInChildren<BaseJukeBox>();

            if (baseJukeBox != null)
            {
                QuickLogger.Debug("Attempting to stop music",true);
                {
                    if (__instance.asset?.path != null && __instance.asset.path.Contains("event:/env/music/") && baseJukeBox.IsPlaying)
                    {
                        QuickLogger.Debug($"Stopping Music: {__instance.asset.name} from playing due to JukeBox",true);
                        __instance.Stop();
                        return false;
                    }
                }
            }

            return true;
        }
	}
}
