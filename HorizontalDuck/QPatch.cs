using FCSCommon.Utilities;
using HorizontalDuck.Buildables;
using System;
using System.Reflection;

namespace HorizontalDuck
{
    public static class QPatch
    {
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly()));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                HorizontalDuckBuildable.PatchHelper();
                
                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
