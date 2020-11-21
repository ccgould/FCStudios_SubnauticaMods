using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono.AlterraHub;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    /// <summary>
    /// A class that deals with purchasing items, returning items and crafting items
    /// </summary>
    internal static class StoreInventorySystem
    {
        private const decimal PlayerPaymentPercentage = .5m;
        private static readonly Dictionary<TechType, decimal> KnownPrices = new Dictionary<TechType, decimal>();

        private static readonly Dictionary<TechType, decimal> OrePrices = new Dictionary<TechType, decimal>
        {
            {TechType.CrashPowder, 7.5m },
            {TechType.Copper,200},
            {TechType.Sulphur,50 },
            {TechType.Diamond,2834950000 },
            {TechType.Gold,1330428 },
            {TechType.Kyanite,2800 },
            {TechType.Lead,500 },
            {TechType.Lithium,800 },
            {TechType.Magnetite,300 },
            {TechType.Nickel,330 },
            {TechType.Quartz,19800 },
            {TechType.AluminumOxide,4535920 },
            {TechType.Silver,39000 },
            {TechType.UraniniteCrystal,7000000 }
        };

        internal static decimal GetPrice(TechType techType)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            return KnownPrices.ContainsKey(techType) ? KnownPrices[techType] : 0;
        }

        internal static decimal GetOrePrice(TechType techType)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            if (OrePrices.ContainsKey(techType))
            {
                return OrePrices[techType] * PlayerPaymentPercentage / 100;
            }

            return 0;
        }

        internal static void AddNewStoreItem(TechType techType, decimal cost)
        {
            if (!KnownPrices.ContainsKey(techType))
            {
                KnownPrices.Add(techType,cost);
            }
        }

        internal static GameObject CreateStoreItem(TechType techType, TechType receiveTechType, StoreCategory category, decimal cost, Action<TechType, TechType> addItemCallBack)
        {
            var item = GameObject.Instantiate(AlterraHub.ItemPrefab);
            var storeItem = item.AddComponent<StoreItem>();
            storeItem.Initialize(LanguageHelpers.GetLanguage(techType), techType, receiveTechType, cost, addItemCallBack, category);
            return item;
        }

        internal static decimal CalculateCost(int multiplier, TechType techType)
        {
            return multiplier * GetPrice(techType);
        }

        internal static bool ItemReturn(InventoryItem item)
        {
            if (item == null)
            {
                return false;
            }

            var getItemPrice = GetPrice(item.item.GetTechType());
            CardSystem.main.AddFinances(getItemPrice);
            return true;
        }

        internal static bool ValidResource(TechType techType)
        {
            return OrePrices.ContainsKey(techType);
        }

        public static bool ValidStoreItem(TechType techType)
        {
            return KnownPrices.ContainsKey(techType);
        }
    }
}