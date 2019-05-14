using FCSTechWorkBench.Configuration;
using FCSTechWorkBench.Logging;
using System;
using Utilites.Logger;

namespace FCSTechWorkBench
{
    public class QPatch
    {
        private static bool _success = true;


        public static void Patch()
        {
            //Clear log file
            Logger.ClearCustomLog();
            Log.Info($"Initializing {Information.ModFriendly}");
            try
            {
                LoadItems.Instance.Patch();

            }
            catch (Exception e)
            {
                _success = false;
                Log.e(e);
                Log.Debug("Error in QPatch");
            }
            Log.Info($"{Information.ModFriendly} initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
