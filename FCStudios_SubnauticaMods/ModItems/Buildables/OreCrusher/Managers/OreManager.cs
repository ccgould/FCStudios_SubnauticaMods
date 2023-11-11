using System.Collections.Generic;

namespace FCS_AlterraHub.ModItems.Buildables.OreCrusher.Managers;
internal static class OreManager
{
    internal static readonly Dictionary<TechType, decimal> OrePrices = new Dictionary<TechType, decimal>
        {
            {TechType.Titanium, 1500},
            {TechType.Copper,2250},
            {TechType.Quartz,6000 },
            {TechType.Lead,9000 },
            {TechType.Diamond,20475 },
            {TechType.Silver,11250 },
            {TechType.Gold,15750 },
            {TechType.Lithium,15750 },
            {TechType.Sulphur,22200 },
            {TechType.Magnetite,19425 },
            {TechType.Nickel,24000 },
            {TechType.AluminumOxide,25200 },
            {TechType.UraniniteCrystal,22050 },
            {TechType.Kyanite, 37500 }
        };

    internal static decimal GetOrePrice(TechType techType)
    {
        //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
        if (OrePrices.ContainsKey(techType))
        {
            return OrePrices[techType] * (decimal)Plugin.Configuration.OrePayoutMultiplier;
        }

        return 0;
    }

    internal static bool ValidResource(TechType techType)
    {
        return OrePrices.ContainsKey(techType);
    }
}
