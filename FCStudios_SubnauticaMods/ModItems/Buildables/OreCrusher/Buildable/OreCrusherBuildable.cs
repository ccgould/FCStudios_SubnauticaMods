using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;
using FCSCommon.Helpers;
using Nautilus.Crafting;
using System.Collections;
using UnityEngine;
using static CraftData;

namespace FCS_AlterraHub.ModItems.Buildables.OreCrusher.Buildable;
internal class OreCrusherBuildable : FCSBuildableModBase
{

    public static float OreProcessingTime { get; set; } = 90;
    public static TechType PatchedTechType { get; private set; }
    private TechType _kitTechType;

    public OreCrusherBuildable() : base(PluginInfo.PLUGIN_NAME, "OreCrusher", FileSystemHelper.ModDirLocation, "OreCrusher", "Ore Crusher")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(ClassID, FriendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;
            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);
            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_OreCrusher>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_OreCrusher", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.AlterraHub);
        };
    }

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
                new Ingredient(TechType.Titanium, 1),
            }
        };
    }
}
