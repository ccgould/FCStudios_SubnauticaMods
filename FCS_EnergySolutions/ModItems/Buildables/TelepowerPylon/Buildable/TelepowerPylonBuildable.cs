using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono.UGUI;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Spawnable;
using FCSCommon.Helpers;
using System;
using System.Collections;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Buildable;

internal class TelepowerPylonBuildable : FCSBuildableModBase
{
    internal static TechType PatchedTechType;

    public TelepowerPylonBuildable() : base(PluginInfo.PLUGIN_NAME, "TelepowerPylon", FileSystemHelper.ModDirLocation, "TelepowerPylon", "Telepower Pylon")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_TelepowerPylon>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_TelepowerPylon", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Energy);

            AddUpgrades();
        };
    }

    private void AddUpgrades()
    {
        var mk2PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk2Upgrade", "Telepower MK2 Upgrade", Color.cyan);
        mk2PylonUpgrade.PatchSMLHelper();

        var mk3PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk3Upgrade", "Telepower MK3 Upgrade", Color.green);
        mk3PylonUpgrade.PatchSMLHelper();
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
       yield return null;
    }
}
