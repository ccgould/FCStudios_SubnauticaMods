using System;
using FCSTerminal.Logging;
using Logger = Utilites.Logger.Logger;

namespace FCSTerminal
{
    public class QPatch
    {
        private static bool _success = true;

        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            Log.Info("Initializing FCS Terminal");
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
            Log.Info("FCSTerminal initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
