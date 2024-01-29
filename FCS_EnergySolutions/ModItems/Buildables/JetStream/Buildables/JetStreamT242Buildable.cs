using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.JetStream.Buildables;
internal class JetStreamT242Buildable : FCSBuildableModBase
{
    public static TechType PatchedTechType { get; private set; }

    public JetStreamT242Buildable() : base(PluginInfo.PLUGIN_NAME, "JetStreamT242", FileSystemHelper.ModDirLocation, "JetStreamT242", "Jet Stream T242")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            //FCSPDAController.AddAdditionalPage<uGUI_SolarCluster>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_SolarCluster", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Energy);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel);
        yield return taskResult;

        PowerRelay solarPowerRelay = taskResult.GetResult().GetComponent<PowerRelay>();

        var pFX = prefab.GetComponent<PowerFX>();
        pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab;
    }
}
