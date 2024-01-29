using Nautilus.Assets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Nautilus.Crafting;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;

namespace FCS_ProductionSolutions.ModItems.Spawnables;

public class FCSGlass
{
    // To access the TechType anywhere in the project
    public static PrefabInfo Info { get; private set; }

    public static void Register()
    {
        Info = PrefabInfo.WithTechType("FCSGlass", "Sand Infused Glass", "SiO2. Pure fused sand glass.")
            .WithIcon(SpriteManager.Get(TechType.Glass));
        
        var prefab = new CustomPrefab(Info);

        // Add our item to the sets category
        prefab.SetPdaGroupCategory(TechGroup.Resources, TechCategory.BasicMaterials);

       

        KnownTechHandler.UnlockOnStart(prefab.Info.TechType);

       var f =  prefab.SetRecipe(GetRecipe());
        f.WithFabricatorType(CraftTree.Type.Fabricator);
        f.WithStepsToFabricatorTab("Resources", "BasicMaterials");

        // Set our prefab to a clone of the Seamoth electrical defense module
        prefab.SetGameObject(GetGameObjectAsync);

        // register the coal to the game
        prefab.Register();
    }

    public static IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var task = new TaskResult<GameObject>();
        yield return CraftData.GetPrefabForTechTypeAsync(TechType.Glass, false, task);
        gameObject.Set(GameObject.Instantiate(task.Get()));
    }

    public static RecipeData GetRecipe()
    {
        return new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<CraftData.Ingredient>
            {
                new CraftData.Ingredient(SandBagSpawnable.PatchedTechType,1)
            }
        };
    }
}