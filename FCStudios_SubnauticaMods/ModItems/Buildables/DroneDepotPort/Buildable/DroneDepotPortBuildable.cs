using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Mono;
using FCSCommon.Helpers;
using Nautilus.Crafting;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static CraftData;

namespace FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Buildable;

internal class DroneDepotPortBuildable : FCSBuildableModBase
{
    private TechType _kitTechType;

    public DroneDepotPortBuildable() : base(PluginInfo.PLUGIN_NAME, "DeepDrillerMK3", FileSystemHelper.ModDirLocation, "DepotDronePort", "Drone Port")
    {
        //DeepDrillerMK3
        //fcsDepotDronePort
    }

    public override void OnFinishRegistering()
    {
        var kit = new FCSKit(ClassID, FriendlyName);
        kit.PatchSMLHelper();
        _kitTechType = kit.TechType;

        PatchedTechType = TechType;
        //FCSPDAController.AddAdditionalPage<uGUI_IonCube>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_Ioncube", bundleName, FileSystemHelper.ModDirLocation, false));
        FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Misc);
    }

    public static TechType PatchedTechType { get; private set; }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        prefab.AddComponent<ColorManager>();
        prefab.AddComponent<DroneDepotPortController>();
        //MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", "AH");
        yield break;
    }

    public override RecipeData GetRecipe()
    {
        return new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<CraftData.Ingredient>
            {
                new Ingredient(TechType.Glass, 2),
                new Ingredient(TechType.TitaniumIngot, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1)
            }
        };
    }
}
