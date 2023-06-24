using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using Nautilus.Crafting;
using System.Collections;
using UnityEngine;
using static CraftData;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
internal class RemoteStorageBuildable : FCSBuildableModBase
{
    private TechType _kitTechType;

    public RemoteStorageBuildable() : base(PluginInfo.PLUGIN_NAME, "RemoteStorage", FileSystemHelper.ModDirLocation, "RemoteStorage", "Remote Storage")
    {
        OnStartRegister += () =>
        {
            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Storage);
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
