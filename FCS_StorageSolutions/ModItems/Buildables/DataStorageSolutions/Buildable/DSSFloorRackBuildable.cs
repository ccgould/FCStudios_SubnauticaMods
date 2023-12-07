using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
using FCSCommon.Helpers;
using Nautilus.Crafting;
using System.Collections;
using UnityEngine;
using static CraftData;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
internal class DSSFloorRackBuildable : FCSBuildableModBase
{
    public DSSFloorRackBuildable() : base(PluginInfo.PLUGIN_NAME, "DSS_FloorServerRack", FileSystemHelper.ModDirLocation, "DSSFloorServerRack", "Floor Server Rack")
    {

        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Storage);

            FCSPDAController.AddAdditionalPage<uGUI_ServerRack>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_ServerRack", bundleName, FileSystemHelper.ModDirLocation, false));

        };
    }

    public static TechType PatchedTechType { get; private set; }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BaseSecondaryCol, prefab, Color.green);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BaseSecondaryCol, prefab, 5f);
        yield break;
    }
}
