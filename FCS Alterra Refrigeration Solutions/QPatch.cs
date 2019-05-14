using FCS_Alterra_Refrigeration_Solutions.Logging;
using System;
using Utilites.Logger;

namespace FCS_Alterra_Refrigeration_Solutions
{
    public class QPatch
    {
        private static bool _success = true;

        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            Log.Info("Initializing FCS A.R Solutions");
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
            Log.Info("FCS A.R Solutions initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
