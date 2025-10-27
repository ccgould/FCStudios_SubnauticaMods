using FCSCommon.Utilities;
using System.Collections.Generic;
using System.Linq;


namespace FCS_AlterraHub.Core.Helpers;

public static class TechDataHelpers
{
    private static Dictionary<TechType, int> _knownIngredientCounts = new Dictionary<TechType, int>();
    private static HashSet<TechType> batteryTech;
    private static HashSet<TechType> powercellTech;
    private static HashSet<TechType> cellConcatTech;
    private static List<Ingredient> _ingredients = new List<Ingredient>();

    public static HashSet<TechType> CellConcatTech
    {
        get
        {
            if (cellConcatTech is null || cellConcatTech.Count == 0)
            {
                cellConcatTech = new HashSet<TechType>(BatteryCharger.compatibleTech).Concat(PowerCellCharger.compatibleTech).ToHashSet();
            }

            return cellConcatTech;
        }
    }

    public static HashSet<TechType> BatteryTech
    {
        get
        {
            if (batteryTech is null || batteryTech.Count == 0)
            {
                batteryTech = new HashSet<TechType>(BatteryCharger.compatibleTech);
            }

            return batteryTech;
        }
    }

    public static HashSet<TechType> PowercellTech
    {
        get
        {
            if (powercellTech is null || powercellTech.Count == 0)
            {
                powercellTech = new HashSet<TechType>(PowerCellCharger.compatibleTech);
            }

            return powercellTech;
        }
    }

    public static bool ContainsValidCraftData(TechType techType)
    {
        
        var data = TechData.GetCraftAmount(techType);
        if (data > 1)
        {
            QuickLogger.Debug($"techType '{techType}' has no valid recipe for recycling.");
            return false;
        }

        var linkedItems = TechData.GetLinkedItems(techType);

        if (linkedItems is not null && linkedItems.Count > 0)
        {
            Dictionary<TechType, int> pairs = new Dictionary<TechType, int>();

            foreach (var item in linkedItems)
            {
                if (pairs.ContainsKey(item))
                    pairs[item] += 1;
                else
                    pairs[item] = 1;
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
        return GetIngredientCount(pickup.GetTechType());
    }

    public static int GetIngredientCount(TechType techType)
    {
        if (!_knownIngredientCounts.ContainsKey(techType))
        {
            _knownIngredientCounts.Add(techType,
                CalculateIngredientCount(TechData.GetIngredients(techType).ToList()));
        }

        return _knownIngredientCounts[techType];
    }

    private static int CalculateIngredientCount(List<Ingredient> ingredients)
    {
        int total = 0;
        foreach (Ingredient ingredient in ingredients)
        {
            total += ingredient.amount;
        }

        return total;
    }

    public static List<Ingredient> GetIngredientsWithOutBatteries(TechType techType)
    {
        _ingredients.Clear();

        List<Ingredient> readOnlyCollection = TechData.GetIngredients(techType).ToList();

        foreach (Ingredient ingredient in readOnlyCollection)
        {
            if (CellConcatTech.Contains(techType) || !CellConcatTech.Contains(ingredient.techType))
            {
                _ingredients.Add(ingredient);
            }
        }

        return _ingredients;
    }

    public static void AddPowercell(TechType techType)
    {
        if (PowerCellCharger.compatibleTech.Contains(techType)) return;
        PowerCellCharger.compatibleTech.Add(techType);
    }

    public static Vector2int GetItemSize(TechType techType)
    {
        var size = TechData.GetItemSize(techType);
        return size;
    }
}
