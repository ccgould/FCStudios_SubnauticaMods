using FCSCommon.Utilities;
using FCSTesting.Buildables;
using Harmony;
using System;
using System.Reflection;

namespace FCSTesting
{
    public class QPatch
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

                CyclopsBioReactorBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fcstesting.fcstudios");

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
