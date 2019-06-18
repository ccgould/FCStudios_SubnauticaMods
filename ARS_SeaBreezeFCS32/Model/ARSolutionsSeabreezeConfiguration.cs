using FCSCommon.Objects;
using SMLHelper.V2.Assets;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Model
{
    internal static class ARSolutionsSeabreezeConfiguration
    {
        public static Dictionary<TechType, ModPrefab> AllowedFilters { get; set; } = new Dictionary<TechType, ModPrefab>
        {

        };

        internal static Dictionary<TechType, EatableEntities> EatableEntities { get; set; } = new Dictionary<TechType, EatableEntities>
        {
            {
                TechType.BigFilteredWater,
                new EatableEntities{WaterValue = 50f, FoodValue = 0f, KDecayRate = 0}
            },
            {
                TechType.Coffee,
                new EatableEntities{WaterValue = 4f, FoodValue = 0f, KDecayRate = 0.02f}
            },
            {
                TechType.CookedBladderfish,
                new EatableEntities{WaterValue = 4f, FoodValue = 16f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedBoomerang,
                new EatableEntities{WaterValue = 3f, FoodValue = 21f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedEyeye,
                new EatableEntities{WaterValue = 10f, FoodValue = 18f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedGarryFish,
                new EatableEntities{WaterValue = 5f, FoodValue = 13f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedHoleFish,
                new EatableEntities{WaterValue = 3f, FoodValue = 21f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedHoopfish,
                new EatableEntities{WaterValue = 3f, FoodValue = 23f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedHoverfish,
                new EatableEntities{WaterValue = 3f, FoodValue = 23f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedLavaBoomerang,
                new EatableEntities{WaterValue = 3f, FoodValue = 20f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedLavaEyeye,
                new EatableEntities{WaterValue = 9f, FoodValue =18f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedOculus,
                new EatableEntities{WaterValue = 2f, FoodValue = 30f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedPeeper,
                new EatableEntities{WaterValue = 5f, FoodValue = 32f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedReginald,
                new EatableEntities{WaterValue = 4f, FoodValue = 44f, KDecayRate = 0.015f}
            },
            {
                TechType.CookedSpadefish,
                new EatableEntities{WaterValue = 3f, FoodValue = 23f, KDecayRate = 0.015f}
            },
            {
                TechType.CuredBladderfish,
                new EatableEntities{WaterValue = -2f, FoodValue = 16f, KDecayRate = 0f}
            },
            {
                TechType.CuredBoomerang,
                new EatableEntities{WaterValue = -2f, FoodValue = 21f, KDecayRate = 0f}
            },
            {
                TechType.CuredEyeye,
                new EatableEntities{WaterValue = -2f, FoodValue = 18f, KDecayRate = 0f}
            },
            {
                TechType.CuredGarryFish,
                new EatableEntities{WaterValue = -2f, FoodValue = 13f, KDecayRate = 0f}
            },
            {
                TechType.CuredHoleFish,
                new EatableEntities{WaterValue = -2f, FoodValue = 21f, KDecayRate = 0f}
            },
            {
                TechType.CuredHoopfish,
                new EatableEntities{WaterValue = -2f, FoodValue = 23f, KDecayRate = 0f}
            },
            {
                TechType.CuredHoverfish,
                new EatableEntities{WaterValue = -2f, FoodValue = 23f, KDecayRate = 0f}
            },
            {
                TechType.CuredLavaBoomerang,
                new EatableEntities{WaterValue = -2f, FoodValue = 21f, KDecayRate = 0f}
            },
            {
                TechType.CuredLavaEyeye,
                new EatableEntities{WaterValue = -2f, FoodValue = 18f, KDecayRate = 0f}
            },
            {
                TechType.CuredOculus,
                new EatableEntities{WaterValue = -2f, FoodValue = 30f, KDecayRate = 0f}
            },
            {
                TechType.CuredPeeper,
                new EatableEntities{WaterValue = -2f, FoodValue = 32f, KDecayRate = 0f}
            },
            {
                TechType.CuredReginald,
                new EatableEntities{WaterValue = -2f, FoodValue = 44f, KDecayRate = 0f}
            },
            {
                TechType.CuredSpadefish,
                new EatableEntities{WaterValue = -2f, FoodValue = 23f, KDecayRate = 0f}
            },
            {
                TechType.CuredSpinefish,
                new EatableEntities{WaterValue = -2f, FoodValue = 23f, KDecayRate = 0f}
            },
            {
                TechType.DisinfectedWater,
                new EatableEntities{WaterValue = 30f, FoodValue = 0f, KDecayRate = 0f}
            },
            {
                TechType.FilteredWater,
                new EatableEntities{WaterValue = 20f, FoodValue = 0f, KDecayRate = 0f}
            },
            {
                TechType.NutrientBlock,
                new EatableEntities{WaterValue = 0f, FoodValue = 75f, KDecayRate = 0f}
            },
            {
                TechType.Snack1,
                new EatableEntities{WaterValue = -2f, FoodValue = 3f, KDecayRate = 0f}
            },
            {
                TechType.Snack2,
                new EatableEntities{WaterValue = -2f, FoodValue = 3f, KDecayRate = 0f}
            },
            {
                TechType.Snack3,
                new EatableEntities{WaterValue = -2f, FoodValue = 3f, KDecayRate = 0f}
            },
            {
                TechType.StillsuitWater,
                new EatableEntities{WaterValue = 20f, FoodValue = -3f, KDecayRate = 0f}
            },
            {
                TechType.BulboTreePiece,
                new EatableEntities{WaterValue = 10f, FoodValue = 8f, KDecayRate = 0.02f}
            },
            {
                TechType.CreepvinePiece,
                new EatableEntities{WaterValue = 1f, FoodValue = 3f, KDecayRate = 0.02f}
            },
            {
                TechType.HangingFruit,
                new EatableEntities{WaterValue = 3f, FoodValue = 10f, KDecayRate = 0.01f}
            },
            {
                TechType.KooshChunk,
                new EatableEntities{WaterValue = 10f, FoodValue = 3f, KDecayRate = 0.02f}
            },
            {
                TechType.Melon,
                new EatableEntities{WaterValue = 14f, FoodValue = 12f, KDecayRate = 0.01f}
            },
            {
                TechType.PurpleVegetable,
                new EatableEntities{WaterValue = 3f, FoodValue = 12f, KDecayRate = 0.01f}
            },
            {
                TechType.SmallMelon,
                new EatableEntities{WaterValue = 7f, FoodValue = 11f, KDecayRate = 0.01f}
            },
            {
                TechType.JellyPlant,
                new EatableEntities{WaterValue = 4f, FoodValue = 5f, KDecayRate = 0f}
            },

        };
    }
}
