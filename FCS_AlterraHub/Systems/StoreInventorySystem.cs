using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Structs;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    /// <summary>
    /// A class that deals with purchasing items, returning items and crafting items
    /// </summary>
    internal static class StoreInventorySystem
    { 
        private static readonly Dictionary<FCSStoreEntry, decimal> KnownPrices = new Dictionary<FCSStoreEntry, decimal>();
        private static readonly Dictionary<TechType, decimal> OrePrices = new Dictionary<TechType, decimal>
        {
            {TechType.Copper,2000},
            {TechType.Sulphur,50 },
            {TechType.Diamond,232553 },
            {TechType.Gold,9401 },
            {TechType.Kyanite, 291965 },
            {TechType.Lead,500 },
            {TechType.Lithium,5644 },
            {TechType.Magnetite,300 },
            {TechType.Nickel,330 },
            {TechType.Quartz,4800 },
            {TechType.AluminumOxide,10635 },
            {TechType.Silver,4777 },
            {TechType.UraniniteCrystal,836 }
        };
        
        internal static decimal GetPrice(TechType techType,bool checkKit = false)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            if (checkKit)
            {
                return StoreHasItem(techType,true) ? KnownPrices.FirstOrDefault(x => x.Key.ReceiveTechType == techType).Value : 0;
            }
            return StoreHasItem(techType) ? KnownPrices.FirstOrDefault(x => x.Key.TechType == techType).Value : 0;
        }

        internal static bool StoreHasItem(TechType techType,bool checkKit = false)
        {
            return checkKit ? KnownPrices.Any(x => x.Key.ReceiveTechType == techType) : KnownPrices.Any(x => x.Key.TechType == techType);
        }

        internal static decimal GetOrePrice(TechType techType)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            if (OrePrices.ContainsKey(techType))
            {
                return OrePrices[techType];
            }

            return 0;
        }

        internal static void AddNewStoreItem(FCSStoreEntry entry)
        {
            if (!KnownPrices.ContainsKey(entry))
            {
                KnownPrices.Add(entry, entry.Cost);
            }
        }

        internal static GameObject CreateStoreItem(FCSStoreEntry entry, Action<TechType, TechType> addItemCallBack)
        {
            var item = GameObject.Instantiate(AlterraHub.ItemPrefab);
            var storeItem = item.AddComponent<StoreItem>();
            storeItem.Initialize(LanguageHelpers.GetLanguage(entry.TechType), entry.TechType, entry.ReceiveTechType, entry.Cost, addItemCallBack, entry.StoreCategory);
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
    }
}