using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;
using FCSCommon.Helpers;
using SMLHelper.Crafting;
using System.Collections;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Buildable;

internal class IonCubeGeneratorBuildable : FCSBuildableModBase
{
    /// <summary>
    /// A class that defines the IonCube Generator mod
    /// Created by: Primesonic and FCStudios.
    /// </summary>
    public IonCubeGeneratorBuildable() : base(Main.MODNAME, "fcsIonCubeGenerator", FileSystemHelper.ModDirLocation, "CubeGenerator", "Ion Cube Generator")
    {

        var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(Main.MODNAME, ClassID);

        OnFinishedPatching += () => 
        {            
            FCSPDAController.AddAdditionalPage<uGUI_IonCube>(uGUI_IonCube.ID, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_Ioncube", bundleName, FileSystemHelper.ModDirLocation, false));
            //uGUIService.AddNewUI<IonCubeGeneratorGUI>("IonCubeGenUI", FCSAssetBundlesService.PublicAPI.GetPrefabByName("IonCubeGenerator_uGUI", bundleName, FileSystemHelper.ModDirLocation,false), new Vector3(1.8f, 1.8f, 1.8f));
        };
    }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        yield return base.GetGameObjectAsync(gameObject);

        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, Prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BaseSecondaryCol, Prefab, Color.green);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, Prefab, 5f);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BaseSecondaryCol, Prefab, 5f);
        Prefab.AddComponent<HoverInteraction>();
        Prefab.AddComponent<IonCubeGeneratorController>();
        yield break;
    }

    protected override TechData GetBlueprintRecipe()
    {
        return new TechData
        {
            Ingredients =
                {
                    //new Ingredient(AlienIngot.TechTypeID, 2),
                    //new Ingredient(AlienEletronicsCase.TechTypeID, 1),

                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.Lubricant, 1),
                    new Ingredient(TechType.Kyanite, 2),
                }
        };
    }
}
