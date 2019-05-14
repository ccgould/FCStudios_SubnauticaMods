using FCSCommon.Utilities;
using FCSCyclopsDock.Configuration;
using System;
using Logger = Utilites.Logger.Logger;

namespace FCSCyclopsDock
{
    public class QPatch
    {
        private static bool _success = true;


        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            QuickLogger.Info($"Initializing FCS {Information.ModFriendly} Version: {QuickLogger.GetAssemblyVersion()}");

            //Code from PrimeSonic
#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif
            //Code from PrimeSonic
            try
            {
                LoadItems.Patch();

            }
            catch (Exception e)
            {
                _success = false;
                QuickLogger.Error($"Error in QPatch: {e}");
            }
            QuickLogger.Info($"FCS {Information.ModFriendly} initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
