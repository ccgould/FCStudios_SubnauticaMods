using FCSCommon.Utilities;
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

            QuickLogger.Debug("Initializing FCS Power Storage");
            try
            {
#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif
                LoadItems.Patch();
            }
            catch (Exception e)
            {
                _success = false;
                QuickLogger.Error("Error in QPatch");
            }
            QuickLogger.Debug("FCS Power Storage initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
