using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using System;
using System.Reflection;


namespace FCSPowerStorage
{
    public class QPatch
    {
        private static bool _success = true;

        public static void Patch()
        {
            QuickLogger.Info("Initializing FCS Power Storage");

            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));



            try
            {
#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif

                LoadData.Patch();
                //OptionsPanelHandler.RegisterModOptions(new InGameOptions());
                FCSPowerStorageBuildable.PatchHelper();
            }
            catch (Exception e)
            {
                _success = false;
                QuickLogger.Error($"Error in QPatch {e.Message}");
            }
            QuickLogger.Info("FCS Power Storage initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
