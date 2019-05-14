using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using System;
using Utilites.Logger;

namespace FCSAlterraIndustrialSolutions
{
    public class QPatch
    {
        private static bool _success = true;


        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            Log.Info($"Initializing FCS {Information.ModFriendly}");
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
            Log.Info($"FCS {Information.ModFriendly} initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
