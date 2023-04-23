using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;
using FCSCommon.Helpers;
using SMLHelper.Crafting;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Buildable;

internal class IonCubeGeneratorBuildable : FCSBuildableModBase
{
    private TechType _kitTechType;

    /// <summary>
    /// A class that defines the IonCube Generator mod
    /// Created by: Primesonic and FCStudios.
    /// </summary>
    public IonCubeGeneratorBuildable() : base(Main.MODNAME, "fcsIonCubeGenerator", FileSystemHelper.ModDirLocation, "CubeGenerator", "Ion Cube Generator")
    {

        var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(Main.MODNAME, ClassID);


        OnStartedPatching += () =>
        {
            var kit = new FCSKit(ClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;
        };

        OnFinishedPatching += () => 
        {
            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_IonCube>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_Ioncube", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType,1, _settings.ItemCost, StoreCategory.Production);
            //uGUIService.AddNewUI<IonCubeGeneratorGUI>("IonCubeGenUI", FCSAssetBundlesService.PublicAPI.GetPrefabByName("IonCubeGenerator_uGUI", bundleName, FileSystemHelper.ModDirLocation,false), new Vector3(1.8f, 1.8f, 1.8f));
        };
    }

    public static TechType PatchedTechType { get; private set; }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        yield return base.GetGameObjectAsync(gameObject);

        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BasePrimaryCol, Prefab, Color.cyan);
        MaterialHelpers.ChangeEmissionColor(ModPrefabService.BaseSecondaryCol, Prefab, Color.green);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BasePrimaryCol, Prefab, 5f);
        MaterialHelpers.ChangeEmissionStrength(ModPrefabService.BaseSecondaryCol, Prefab, 5f);
        Prefab.AddComponent<ColorManager>();
        Prefab.AddComponent<IonCubeGeneratorController>();
        yield break;
    }

    protected override TechData GetBlueprintRecipe()
    {
        return new TechData
        {
            Ingredients =
                {
                    new Ingredient(_kitTechType, 1),
                }
        };
    }
}
