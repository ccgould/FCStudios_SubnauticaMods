using System.Reflection;
using FCS_AlterraHub.API;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;

namespace FCS_HomeSolutions
{
    /*
     * Alterra Home Solutions mod pack adds objects to subnautica that deals with bases and decorations
    */
    
    [QModCore]
    public class QPatch
    {
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            ModelPrefab.GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            ModelPrefab.ModBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.ModBundleName, Mod.GetModDirectory());


            var railing = new DecorationEntryPatch("ahsrailing","Large Railing","A railing to create a barrior",ModelPrefab.GetPrefab("Large_Rail_01"), new Settings
            {
                AllowedInBase = true,
                AllowedOutside = true
            });

            railing.Patch();
        }
    }
}
