using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.BaseUtilityUnit.Buildable;
internal class BaseUtilityUnitBuildable : FCSBuildableModBase
{
    internal static bool IsRefillableOxygenTanksInstalled { get; } = EnumHandler.ModdedEnumExists<TechType>("HighCapacityTankRefill");
    public TechType PatchedTechType { get; private set; }

    public BaseUtilityUnitBuildable() : base(PluginInfo.PLUGIN_NAME, "BaseUtilityUnit", FileSystemHelper.ModDirLocation, "BaseUtilityUnit", "Base Utility Unit")
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
