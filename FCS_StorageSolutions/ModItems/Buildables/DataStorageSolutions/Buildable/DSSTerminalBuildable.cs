using BepInEx;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Utility;
using System.Collections;
using UnityEngine;
using static CraftData;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
internal class DSSTerminalBuildable : FCSBuildableModBase
{
    public DSSTerminalBuildable() : base(PluginInfo.PLUGIN_NAME, "DSS_TerminalMonitor", FileSystemHelper.ModDirLocation, "DSSTerminal", "Terminal C48")
    {

        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Storage);

            FCSModsAPI.PublicAPI.RegisterBaseManagerModule(PluginInfo.PLUGIN_NAME, "DSSIntegrationModule", "DSS Integration Module", "Links Data Storage to the Base Manager, allowing DSS access to device storage and Transceiver automation from DSS. (Req Module Rack)", 9000,StoreCategory.Storage);



        };
    }

    public static TechType PatchedTechType { get; private set; }
    public TechType DSSIntegrationModuleTechType { get; private set; }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BaseSecondaryCol, prefab, Color.green);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, prefab, 5f);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BaseSecondaryCol, prefab, 5f);
        yield break;
    }

    public override RecipeData GetRecipe()
    {
        return new RecipeData
        {
            Ingredients =
                {
                    new Ingredient(_kitTechType, 1),
                }
        };
    }
}
