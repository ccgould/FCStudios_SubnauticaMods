using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using HarmonyLib;

namespace FCS_HomeSolutions.Patches
{
    [HarmonyPatch(typeof(TechLight))]
    [HarmonyPatch("Start")]
    internal class TechLight_Start
    {
        [HarmonyPostfix]
        public static void Postfix(ref TechLight __instance)
        {
            FCSAlterraHubService.PublicAPI.RegisterTechLight(__instance);
        }
    }

    [HarmonyPatch(typeof(TechLight))]
    [HarmonyPatch("UpdatePower")]
    internal class TechLight_UpdatePower
    {

        //this way the reflection would be done only once, insted of getting this vars every function call.
        private static readonly FieldInfo PowerRelayInfo =
            typeof(TechLight).GetField("powerRelay", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo SetLightsActiveMethod =
            typeof(TechLight).GetMethod("SetLightsActive", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo PowerPerSecInfo =
            typeof(TechLight).GetField("powerPerSecond", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly FieldInfo UpdateIntervalInfo =
            typeof(TechLight).GetField("updateInterval", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly FieldInfo SearchingField =
            typeof(TechLight).GetField("searching", BindingFlags.Instance | BindingFlags.NonPublic);

        static bool Prefix(TechLight __instance)
        {
            var powerRelay = (PowerRelay) PowerRelayInfo.GetValue(__instance);
            var powerPerSecond = (float) PowerPerSecInfo.GetValue(null);
            var updateInterval = (float) UpdateIntervalInfo.GetValue(null);

            var searching = (bool) SearchingField.GetValue(__instance);

            var manager = BaseManager.FindManager(__instance);

            if (manager == null) return true;

            if (!__instance.placedByPlayer || !__instance.constructable.constructed) return false;
            if (powerRelay)
            {
                if (powerRelay.GetPowerStatus() == PowerSystem.Status.Normal &&
                    manager.IsBaseExternalLightsActivated)
                {
                    SetLightsActiveMethod.Invoke(__instance, new object[] {true});
                    float num;
                    powerRelay.ConsumeEnergy(powerPerSecond * updateInterval, out num);
                }
                else
                {
                    SetLightsActiveMethod.Invoke(__instance, new object[] {false});
                }
            }
            else
            {
                SetLightsActiveMethod.Invoke(__instance, new object[] {false});
                if (searching) return false;
                SearchingField.SetValue(__instance, true);
                __instance.InvokeRepeating("FindNearestValidRelay", 0f, 2f);
            }
            return false;
        }
    }
}




