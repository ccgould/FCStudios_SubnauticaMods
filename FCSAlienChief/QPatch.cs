using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCSAlienChief
{
    public class QPatch
    {
        private static bool _success = true;

        public static void Patch()
        {
            Log.Info("Initializing FCSAlienChief");
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
            Log.Info("FCSAlienChief initializ" + (!_success ? "ation failed." : "ed successfully."));
        }
    }
}
