using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Interfaces;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class ARSolutionsSeaBreezeContainer : IFridgeContainer
    {
        public bool IsFull { get; }
        public int NumberOfItems => FridgeItems.Count;

        private const int ContainerWidth = 6;

        private const int ContainerHeight = 8;

        internal const int MaxAvailableSpaces = ContainerHeight * ContainerWidth;

        private ItemsContainer _fridgeContainer = null;

        private ChildObjectIdentifier _containerRoot = null;

        private Func<bool> isContstructed;
        private bool _isFridgeOpen = false;
        private bool _isCooling;

        public List<EatableEntities> FridgeItems { get; } = new List<EatableEntities>();

        #region Constructor
        internal ARSolutionsSeaBreezeContainer(ARSolutionsSeaBreezeController mono)
        {
            isContstructed = () => { return mono.IsConstructed; };

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing StorageRoot");
                var storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_fridgeContainer == null)
            {
                QuickLogger.Debug("Initializing Container");

                _fridgeContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    ARSSeaBreezeFCS32Buildable.StorageLabel(), null);

                _fridgeContainer.isAllowedToAdd += IsAllowedToAdd;
                _fridgeContainer.isAllowedToRemove += IsAllowedToRemove;

                _fridgeContainer.onAddItem += OnAddItemEvent;
                _fridgeContainer.onRemoveItem += OnRemoveItemEvent;

                _fridgeContainer.onAddItem += mono.OnAddItemEvent;
                _fridgeContainer.onRemoveItem += mono.OnRemoveItemEvent;
            }
        }
        #endregion


        private void OnRemoveItemEvent(InventoryItem item)
        {
            if (item == null) return;

            var prefabId = item.item.GetComponent<PrefabIdentifier>().Id;

            EatableEntities match = FindMatch(prefabId);

            if (_isCooling)
            {
                var eatable = item.item.GetComponent<Eatable>();


                if (match != null)
                {
                    eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
                    eatable.foodValue = match.FoodValue;
                    eatable.waterValue = match.WaterValue;
                    eatable.decomposes = match.Decomposes;
                    eatable.kDecayRate = match.kDecayRate;


                    QuickLogger.Debug($"Match Found", true);
                    FridgeItems.Remove(match);
                    QuickLogger.Debug($"Match Removed", true);
                }

            }
            else
            {
                foreach (EatableEntities eatableEntity in FridgeItems)
                {
                    FridgeItems.Remove(match);
                    break;
                }
            }

            QuickLogger.Debug($"Fridge Item Count: {FridgeItems.Count}", true);
        }

        private EatableEntities FindMatch(string prefabId)
        {
            foreach (EatableEntities eatableEntity in FridgeItems)
            {
                if (eatableEntity.PrefabID == prefabId)
                {
                    return eatableEntity;
                }
            }

            return null;
        }

        internal void DecayItems()
        {
            //if (!_isCooling) return;

            foreach (InventoryItem inventoryItem in _fridgeContainer)
            {
                var prefabId = inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>().Id;
                var eatable = inventoryItem.item.gameObject.GetComponent<Eatable>();

                foreach (EatableEntities eatableEntity in FridgeItems)
                {

                    if (eatableEntity.PrefabID == prefabId)
                    {
                        eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
                        eatable.decomposes = eatableEntity.Decomposes;
                        eatable.kDecayRate = eatableEntity.kDecayRate;

                        QuickLogger.Debug($"Decaying {inventoryItem.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
                        break;
                    }
                }
            }

            _isCooling = false;

            QuickLogger.Debug($"Decaying", true);
        }

        internal void CoolItems()
        {
            //if (_isCooling) return;

            //FridgeItems.Clear();

            foreach (InventoryItem inventoryItem in _fridgeContainer)
            {
                var eatable = inventoryItem.item.GetComponent<Eatable>();
                var f = eatable.foodValue;
                var w = eatable.waterValue;

                //Now lets set the changes
                eatable.kDecayRate = 0f;

                if (eatable.decomposes)
                {
                    eatable.decomposes = false;
                }

                eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
                eatable.foodValue = f;
                eatable.waterValue = w;
            }

            _isCooling = true;

            QuickLogger.Debug($"Cooling", true);
        }

        private void OnAddItemEvent(InventoryItem item)
        {
            if (item == null) return;

            CoolItem(item);

            QuickLogger.Debug($"Fridge Item Count: {FridgeItems.Count}", true);
        }

        private void CoolItem(InventoryItem item)
        {
            var eatable = item.item.GetComponent<Eatable>();
            var prefabId = item.item.GetComponent<PrefabIdentifier>().Id;

            var f = eatable.foodValue;
            var w = eatable.waterValue;
            QuickLogger.Debug($"Before Dict Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
            //Store Data about the item
            FridgeItems.Add(new EatableEntities
            {
                FoodValue = eatable.GetFoodValue(),
                WaterValue = eatable.GetWaterValue(),
                Name = eatable.name,
                TechType = item.item.GetTechType(),
                Decomposes = eatable.decomposes,
                PrefabID = prefabId,
                kDecayRate = eatable.kDecayRate
            });

            //Now lets set the changes
            eatable.kDecayRate = 0f;

            if (eatable.decomposes)
            {
                eatable.decomposes = false;
            }

            eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
            eatable.foodValue = f;
            eatable.waterValue = w;
            QuickLogger.Debug($"After Dict Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (ARSolutionsSeabreezeConfiguration.EatableEntities.ContainsKey(techType))
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("[Alterra Refrigeration] Food items allowed only.");
            return flag;
        }

        public void OpenStorage()
        {
            QuickLogger.Debug($"Storage Button Clicked", true);

            if (!isContstructed.Invoke())
                return;

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_fridgeContainer, false);
            pda.Open(PDATab.Inventory, null, OnFridgeClose, 4f);
            _isFridgeOpen = true;
        }

        private void OnFridgeClose(PDA pda)
        {
            _isFridgeOpen = false;
        }

        public int GetTechTypeAmount(TechType techType)
        {
            return _fridgeContainer.GetCount(techType);
        }

        internal List<EatableEntities> GetFridgeItems()
        {
            return FridgeItems;
        }

        public void LoadFoodItems(List<EatableEntities> savedDataFridgeContainer)
        {
            FridgeItems.Clear();

            foreach (EatableEntities eatableEntities in savedDataFridgeContainer)
            {
                QuickLogger.Debug($"Adding entity {eatableEntities.Name}");

                var food = GameObject.Instantiate(CraftData.GetPrefabForTechType(eatableEntities.TechType));
                food.gameObject.GetComponent<PrefabIdentifier>().Id = eatableEntities.PrefabID;

                var eatable = food.gameObject.GetComponent<Eatable>();
                eatable.foodValue = eatableEntities.FoodValue;
                eatable.waterValue = eatableEntities.WaterValue;

                var item = new InventoryItem(food.gameObject.GetComponent<Pickupable>().Pickup(false));

                _fridgeContainer.UnsafeAdd(item);
                QuickLogger.Debug($"Load Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
            }
        }

        public bool GetOpenState()
        {
            return _isFridgeOpen;
        }
    }
}
