using Harmony;
using AE.DropAllOnDeath.Config;
using SMLHelper.V2.Handlers;
using System;
using System.Reflection;
using QModManager.API.ModLoading;

namespace AE.DropAllOnDeath
{
    [QModCore]
    public class QPatch
    {
        [QModPatch]
        public static void Patch()
        {
            //QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                Mod.Configuration = Mod.LoadConfiguration();

                OptionsPanelHandler.RegisterModOptions(new Options());

                HarmonyInstance.Create("com.dropallondeath.MAC").PatchAll(Assembly.GetExecutingAssembly());

                //QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                //QuickLogger.Error(ex);
            }
        }
    }
}
