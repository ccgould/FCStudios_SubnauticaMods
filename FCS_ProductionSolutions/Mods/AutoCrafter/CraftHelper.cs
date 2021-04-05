using System.Collections.Generic;
using QModManager.API;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
#endif
namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    public static class CrafterLogicHelper
    {
        public static List<TechType> blackList = new List<TechType>() {TechType.Titanium, TechType.Copper};

        public static bool IsItemUnlocked(TechType techType)
        {
            if (GameModeUtils.RequiresBlueprints())
            {
                if (!QModServices.Main.ModPresent("UITweaks"))
                {
                    RecipeData data = GetData(techType);
                    int ingredientCount = data?.ingredientCount ?? 0;
                    for (int i = 0; i < ingredientCount; i++)
                    {
                        Ingredient ingredient = data.Ingredients[i];
                        if (!blackList.Contains(techType) && !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                        {
                            return false;
                        }
                    }
                }
                else
                {
#if SUBNAUTICA
                    if (CraftData.techData.TryGetValue(techType, out CraftData.TechData data))
                    {
                        int ingredientCount = data?.ingredientCount ?? 0;
                        for (int i = 0; i < ingredientCount; i++)
                        {
                            IIngredient ingredient = data.GetIngredient(i);
                            if (!blackList.Contains(techType) &&
                                !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                            {
                                return false;
                            }
                        }
                    }
#elif BELOWZERO
#endif
                }
            }

            return true;
        }

        internal static RecipeData GetData(TechType techType)
        {
#if SUBNAUTICA
            return CraftDataHandler.GetTechData(techType);
#elif BELOWZERO
            return CraftDataHandler.GetRecipeData(techType);
#endif
        }
    }
}
