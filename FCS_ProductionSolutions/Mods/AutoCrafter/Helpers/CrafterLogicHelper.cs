﻿using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
#endif
namespace FCS_ProductionSolutions.Mods.AutoCrafter.Helpers
{
    public static class CrafterLogicHelper
    {
        public static List<TechType> BlackList = new List<TechType>() { TechType.Titanium, TechType.Copper };

        public static bool IsItemUnlocked(TechType techType, bool useDefault = false)
        {
#if DEBUG
            QuickLogger.Debug($"Checking if {Language.main.Get(techType)} is unlocked");
#endif
            if (useDefault)
            {
                return CrafterLogic.IsCraftRecipeUnlocked(techType);
            }


            if (UWEHelpers.RequiresIngredients())
            {
                
                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Any(x => x.Metadata.Name.Equals("UITweaks")))
                {
                    RecipeData data = GetData(techType);
                    int ingredientCount = data?.ingredientCount ?? 0;
                    for (int i = 0; i < ingredientCount; i++)
                    {
                        Ingredient ingredient = data.Ingredients[i];
                        if (!BlackList.Contains(techType) && !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                        {
#if DEBUG
                            QuickLogger.Debug($"{Language.main.Get(ingredient.techType)} is locked");
#endif
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
                            if (!BlackList.Contains(techType) &&
                                !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                            {
#if DEBUG
                                QuickLogger.Debug($"{Language.main.Get(techType)} is locked");
#endif
                                return false;
                            }
                        }
                    }
#elif BELOWZERO
#endif
                }
            }

#if DEBUG
            QuickLogger.Debug($"{Language.main.Get(techType)} is unlocked");
#endif
            return true;
        }

        internal static RecipeData GetData(TechType techType)
        {
            return CraftDataHandler.GetTechData(techType);
        }

        public static void Inc<T>(this Dictionary<T, int> dictionary, T key, int value = 1)
        {
            int num;
            dictionary.TryGetValue(key, out num);
            dictionary[key] = num + value;
        }
    }
}
