using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Buildable;
internal class PowerStorageBuildable : FCSBuildableModBase
{
    public static TechType PatchedTechType { get; private set; }

    public PowerStorageBuildable() : base(PluginInfo.PLUGIN_NAME, "PowerStorage", FileSystemHelper.ModDirLocation, "PowerStorage", "Power Storage")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
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
