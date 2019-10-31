using System;
using System.IO;
using static FCSModdingUtility.DI;

namespace FCSModdingUtility
{
    internal static class ApplicationHelpers
    {
        internal static bool SafeDeleteDirectory(string location, bool notify = true)
        {
            try
            {
                if (Directory.Exists(location))
                {
                    Directory.Delete(location, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (notify)
                {
                    ViewModelApplication.DefaultError(e.Message);
                    //TODO report message to log and window
                }
            }
            return false;
        }

        internal static bool SafeDeleteFile(string location, bool notify = true)
        {
            try
            {
                if (File.Exists(location))
                {
                    File.Delete(location);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (notify)
                {
                    ViewModelApplication.DefaultError(e.Message);
                    //TODO report message to log and window
                }
            }
            return false;
        }
    }
}
