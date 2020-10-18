using System;
using System.Collections;
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
        private const float PlayerPaymentPercentage = .5f;
        private static Dictionary<TechType, float> _knownPrices = new Dictionary<TechType, float>();

        private static Dictionary<TechType, float> _orePrices = new Dictionary<TechType, float>
        {
            {TechType.CrashPowder,7.5f },
            {TechType.Copper,200f },
            {TechType.Sulphur,50f },
            {TechType.Diamond,2834950000f },
            {TechType.Gold,1330428f },
            {TechType.Kyanite,2800f },
            {TechType.Lead,500f },
            {TechType.Lithium,800f },
            {TechType.Magnetite,300f },
            {TechType.Nickel,330f },
            {TechType.Quartz,19800f },
            {TechType.AluminumOxide,4535920f },
            {TechType.Silver,39000f },
            {TechType.UraniniteCrystal,7000000f }};

        internal static float GetPrice(TechType techType)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            if (_knownPrices.ContainsKey(techType))
            {
                return _knownPrices[techType];
            }

            return 0f;
        }

        internal static float GetOrePrice(TechType techType)
        {
            //Price will be calculated by the ingredients of an item if an ingredient is unknown it will apply a default value to that item
            if (_orePrices.ContainsKey(techType))
            {
                return _orePrices[techType] * PlayerPaymentPercentage / 100f;
            }

            return 0f;
        }

        internal static void AddNewStoreItem(TechType techType, float cost)
        {
            if (!_knownPrices.ContainsKey(techType))
            {
                _knownPrices.Add(techType,cost);
            }
        }

        internal static GameObject CreateStoreItem(TechType techType, TechType receiveTechType, StoreCategory category, float cost, Action<TechType, TechType> addItemCallBack)
        {
            var item = GameObject.Instantiate(AlterraHub.ItemPrefab);
            var storeItem = item.AddComponent<StoreItem>();
            storeItem.Initialize(LanguageHelpers.GetLanguage(techType), techType, receiveTechType, cost, addItemCallBack, category);
            return item;
        }

        internal static float CalculateCost(int multiplyer, TechType techType)
        {
            return multiplyer * GetPrice(techType);
        }

        internal static bool ItemReturn(string cardNumber, InventoryItem item)
        {
            if (item == null)
            {
                MessageBoxHandler.main.Show(string.Format(AlterraHub.AccountNotFoundFormat(), cardNumber));
                return false;
            }

            var getItemPrice = GetPrice(item.item.GetTechType());
            CardSystem.main.AddFinances(getItemPrice);
            return true;

        }

        internal static bool VaildResource(TechType techType)
        {
            return _orePrices.ContainsKey(techType);
        }
    }
}
