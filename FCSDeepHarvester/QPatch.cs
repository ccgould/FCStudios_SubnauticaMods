using System;
using FCSDeepHarvester.Logging;
using UnityEngine;
using Logger = Utilites.Logger.Logger;

namespace FCSDeepHarvester
{
    public class QPatch
    {
        private static bool _success = true;


        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            Log.Info("Initializing FCS Deep Harvester");
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
            Log.Info("FCS Deep Harvester initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
