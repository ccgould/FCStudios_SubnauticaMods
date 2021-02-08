using System.Collections.Generic;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class TechDataHelpers
    {
        private static Dictionary<TechType,int> _knownIngredientCounts = new Dictionary<TechType, int>();
        private static HashSet<TechType> batteryTech;
        private static List<IIngredient> _ingredients = new List<IIngredient>();
        public static HashSet<TechType> BatteryTech
        {
            get
            {
                if (batteryTech is null || batteryTech.Count == 0)
                {
                    batteryTech = new HashSet<TechType>(BatteryCharger.compatibleTech).Concat(PowerCellCharger.compatibleTech).ToHashSet();
                }
                return batteryTech;
            }
        }
        public static bool ContainsValidCraftData(TechType techType)
        {
            var data = CraftData.Get(techType, true);
            if (data == null || data.craftAmount > 1)
            {
                QuickLogger.Debug($"TechType '{techType}' has no valid recipe for recycling.");
                return false;
            }

            if (data.linkedItemCount > 0)
            {
                Dictionary<TechType, int> pairs = new Dictionary<TechType, int>();
                for (int i = 0; i < data.linkedItemCount; i++)
                {
                    TechType techType2 = data.GetLinkedItem(i);
                    if (pairs.ContainsKey(techType2))
                        pairs[techType2] += 1;
                    else
                        pairs[techType2] = 1;
                }

                ItemsContainer inventory = Inventory.main?.container;

                if (inventory is null)
                    return false;

                foreach (KeyValuePair<TechType, int> pair in pairs)
                {
                    if (inventory.GetCount(pair.Key) < pair.Value)
                        return false;
                }
            }

            return true;
        }

        public static bool IsUsedBattery(Pickupable pickupable)
        {
            IBattery component = pickupable.GetComponent<IBattery>();
            return component != null && (double)component.charge < (double)component.capacity * 0.985f;
        }

        public static int GetIngredientCount(Pickupable pickup)
        {
            if (!_knownIngredientCounts.ContainsKey(pickup.GetTechType()))
            {
                _knownIngredientCounts.Add(pickup.GetTechType(),CalculateIngredientCount(GetIngredients(pickup.GetTechType())));
            }
            return _knownIngredientCounts[pickup.GetTechType()];
        }

        private static int CalculateIngredientCount(List<IIngredient> ingredients)
        {
            int total = 0;
            foreach (IIngredient ingredient in ingredients)
            {
                total += ingredient.amount;
            }

            return total;
        }

        public static List<IIngredient> GetIngredients(TechType techType)
        {
            _ingredients.Clear();

            var it = CraftData.Get(techType);

            QuickLogger.Debug($"Original Ingredient Count: {it.ingredientCount}", true);

            if (it != null)
            {
                List<IIngredient> readOnlyCollection = new List<IIngredient>();
                for (int i = 0; i < it.ingredientCount; i++)
                {
                    readOnlyCollection.Add(it.GetIngredient(i));
                }

                foreach (IIngredient ingredient in readOnlyCollection)
                {
                    if (BatteryTech.Contains(techType) || !BatteryTech.Contains(ingredient.techType))
                    {
                        _ingredients.Add(ingredient);
                    }
                }
            }
            return _ingredients;
        }
    }
}
