using System;
using System.Reflection;
using BepInEx;
using CyclopsUpgradeConsole.Buildables;
using CyclopsUpgradeConsole.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using static OVRPlugin;

namespace CyclopsUpgradeConsole
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main
    {
        #region [Declarations]

        public const string
            MODNAME = "FCS_StorageSolutions",
            AUTHOR = "FieldCreatorsStudios",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        internal static string Version { get; set; }
        #endregion


        private  void Awake()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");

            try
            {
                var alterraGen = new CUCBuildable();
                alterraGen.Patch();

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}
