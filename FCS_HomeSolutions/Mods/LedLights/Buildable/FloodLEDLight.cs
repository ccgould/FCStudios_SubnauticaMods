using FCS_AlterraHub.Extensions;
using FCS_HomeSolutions.Buildables;
using SMLHelper.V2.Crafting;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
#endif

namespace FCS_HomeSolutions.Mods.LedLights.Buildable
{
    internal class FloodLEDLight : LedLightPatch
    {
        public FloodLEDLight() : base(new LedLightData
        {
            classId = "FloodLEDLight",
            description = "Flood LED light . (Change the color with the Paint Tool) (Exterior use only)",
            friendlyName = "Flood Led Light",
            allowedInBase = false,
            allowedInSub = false,
            allowedOnGround = true,
            allowedOnWall = false,
            allowedOutside = true,
            categoryForPDA = TechCategory.ExteriorModule,
            groupForPda = TechGroup.ExteriorModules,
            size = Vector3.zero,
            center = Vector3.zero,
            prefab = ModelPrefab.GetPrefabFromGlobal("FCS_FloodLight"),
            TechData = LedLightStickWallIngredients
        })
        {
        }
        internal static RecipeData LedLightStickWallIngredients => new RecipeData
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient("FloodLEDLight_Kit".ToTechType(), 1)
            }
        };
    }
}
