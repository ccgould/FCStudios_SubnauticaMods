using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono.uGUI;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;
using static UWE.FreezeTime;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Buildable;
internal class HydroponicHarvesterBuildable : FCSBuildableModBase
{
    public TechType PatchedTechType { get; private set; }


    public HydroponicHarvesterBuildable() : base(PluginInfo.PLUGIN_NAME, "Harvester", FileSystemHelper.ModDirLocation, "HydroponicHarvester", "Hydroponic Harvester")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_HydroponicHarvester>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_HydroponicHarvester", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Production);
        };
    }


    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        foreach (var uniqueIdentifier in prefab.GetComponentsInChildren<UniqueIdentifier>())
            uniqueIdentifier.classId = ClassID;
        yield return prefab;
    }
}
