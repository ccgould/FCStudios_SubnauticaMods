using FCSTerminal.Logging;
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
            Log.Info("Initializing FCS Power Storage");
            try
            {
                LoadItems.Patch();
            }
            catch (Exception e)
            {
                _success = false;
                Log.e(e);
                Log.Debug("Error in QPatch");
            }
            Log.Info("FCS Power Storage initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
