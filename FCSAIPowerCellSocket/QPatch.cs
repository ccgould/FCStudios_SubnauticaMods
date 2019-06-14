using FCSAIPowerCellSocket.Buildables;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.Reflection;

namespace FCSAIPowerCellSocket
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
                AIPowerCellSocketBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.fcsaipowercellsocket.fcstudios");
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
