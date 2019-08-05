using ExStorageDepot.Buildable;
using FCSCommon.Utilities;
using System;

namespace ExStorageDepot
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
                ExStorageDepotBuildable.PatchHelper();

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
