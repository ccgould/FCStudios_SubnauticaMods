using FCSCommon.Utilities;
using HorizontalDuck.Buildables;
using System;

namespace HorizontalDuck
{
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
                HorizontalDuckBuildable.PatchHelper();

                //var harmony = HarmonyInstance.Create("com.exstoragedepot.fcstudios");
                //harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
