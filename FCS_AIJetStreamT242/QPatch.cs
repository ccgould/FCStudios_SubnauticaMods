using FCS_AIJetStreamT242.Buildable;

namespace FCS_AIJetStreamT242
{
    using FCSCommon.Utilities;
    using Harmony;
    using System;
    using System.Reflection;

    public static class QPatch
    {
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {

                AIJetStreamT242Buildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.aijetstreamt242.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
