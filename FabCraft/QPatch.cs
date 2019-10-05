using FabCraft.Craftables;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.Reflection;

namespace FabCraft
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
                FabCraftCraftable.PatchSMLHelper();

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
