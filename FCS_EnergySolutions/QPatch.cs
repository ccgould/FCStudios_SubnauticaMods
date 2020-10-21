using System.Reflection;
using FCS_EnergySolutions.AlterraGen.Buildables;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;

namespace FCS_EnergySolutions
{

    /*
     * If you are trying to build the project after changing it for subnautica check the build settings
     * make surea ll build settings line up with the correct engine.
     */

    [QModCore]
    public class QPatch
    {
        internal string Version { get; private set; }

        [QModPatch]
        public void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");
            QuickLogger.ModName = Mod.ModName;

            var alterraGen = new AlterraGenBuildable();
            alterraGen.Patch();
        }
    }
}
