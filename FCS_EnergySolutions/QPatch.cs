using System.Reflection;
using FCS_AlterraHub.Configuration;
using FCS_EnergySolutions.AlterraGen.Buildables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_EnergySolutions
{

    /*
     * If you are trying to build the project after changing it for subnautica check the build settings
     * make sure all build settings line up with the correct engine.
     */

    [QModCore]
    public class QPatch
    {
        internal string Version { get; private set; }
        //internal static AlterraGenConfig Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<AlterraGenConfig>();

        [QModPatch]
        public void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");
            QuickLogger.ModName = Mod.ModName;

            ModelPrefab.Initialize();

            var alterraGen = new AlterraGenBuildable();
            alterraGen.Patch();
        }
    }
}
