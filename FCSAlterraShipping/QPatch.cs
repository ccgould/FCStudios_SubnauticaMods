using FCSAlterraShipping.Buildable;

namespace FCSAlterraShipping
{
    using FCSCommon.Utilities;
    using Harmony;
    using System;
    using System.Reflection;

    public static class QPatch
    {
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                AlterraShippingBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.alterrashipping.fcstudios");
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
