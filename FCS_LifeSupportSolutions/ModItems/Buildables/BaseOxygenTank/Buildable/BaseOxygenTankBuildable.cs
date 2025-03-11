using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseUtilityUnit.Buildable;
internal class BaseOxygenTankBuildable : FCSBuildableModBase
{
    public TechType PatchedTechType { get; private set; }
    
    public BaseOxygenTankBuildable() : base(PluginInfo.PLUGIN_NAME, "oxTank", FileSystemHelper.ModDirLocation, "BaseOxygenTank", "Base Oxygen Tank")
    {
        OnStartRegister += () =>
        {

            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            //FCSPDAController.AddAdditionalPage<uGUI_SolarCluster>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_SolarCluster", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.LifeSupport);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield return null;
    }
}
