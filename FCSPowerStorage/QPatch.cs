using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using FCSPowerStorage.Configuration;
using SMLHelper.V2.Handlers;
using System;
using Utilites.Logger;

namespace FCSPowerStorage
{
    public class QPatch
    {
        private static bool _success = true;

        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();

            QuickLogger.Info("Initializing FCS Power Storage");
            try
            {
#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif

                LoadData.Patch();
                OptionsPanelHandler.RegisterModOptions(new InGameOptions());
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
