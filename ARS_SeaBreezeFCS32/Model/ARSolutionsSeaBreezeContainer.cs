using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Enum;
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

        private readonly ItemsContainer _fridgeContainer = null;

        private readonly ChildObjectIdentifier _containerRoot = null;

        private readonly Func<bool> _isConstructed;
        private bool _isFridgeOpen = false;
        private FridgeCoolingState _coolingState;
        private const float Rate = 5.0f;

        public List<EatableEntities> FridgeItems { get; } = new List<EatableEntities>();

        #region Constructor
        internal ARSolutionsSeaBreezeContainer(ARSolutionsSeaBreezeController mono)
        {
            _isConstructed = () => { return mono.IsConstructed; };

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

        private void OnAddItemEvent(InventoryItem item)
        {
            CoolItem(item);
            QuickLogger.Debug($"Fridge Item Count: {FridgeItems.Count}", true);
        }
        private void OnRemoveItemEvent(InventoryItem item)
        {
            var eat = item.item.GetComponent<Eatable>();

            var prefabId = item.item.GetComponent<PrefabIdentifier>().Id;

            var f = eat.GetFoodValue();
            var w = eat.GetWaterValue();


            EatableEntities match = FindMatch(prefabId);

            if (_coolingState == FridgeCoolingState.Cooling)
            {
                var eatable = item.item.GetComponent<Eatable>();

                if (match != null)
                {
                    eatable.kDecayRate = match.KDecayRate;
                    eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
                    eatable.foodValue = f;
                    eatable.waterValue = w;

                    QuickLogger.Debug($"Match Found", true);
                    FridgeItems.Remove(match);
                    QuickLogger.Debug($"Match Removed", true);
                    QuickLogger.Debug($"Decay Reset {eatable.kDecayRate}", true);
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

            FridgeItems.Remove(match);

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
            QuickLogger.Debug($"Cooling State: {_coolingState.ToString()}", true);
            if (_coolingState == FridgeCoolingState.NotCooling) return;

            foreach (InventoryItem inventoryItem in _fridgeContainer)
            {
                var prefabId = inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>().Id;
                var eatable = inventoryItem.item.gameObject.GetComponent<Eatable>();

                foreach (EatableEntities eatableEntity in FridgeItems)
                {
                    if (eatableEntity.PrefabID != prefabId) continue;
                    eatable.kDecayRate = eatableEntity.KDecayRate;

                    QuickLogger.Debug($"Decaying {inventoryItem.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}", true);
                    break;
                }
            }

            _coolingState = FridgeCoolingState.NotCooling;
        }

        internal void CoolItems()
        {
            QuickLogger.Debug($"Cooling State: {_coolingState.ToString()}", true);

            if (_coolingState == FridgeCoolingState.Cooling) return;

            foreach (InventoryItem inventoryItem in _fridgeContainer)
            {
                var eatable = inventoryItem.item.GetComponent<Eatable>();
                var prefabId = inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>().Id;

                if (eatable.decomposes)
                {
                    eatable.kDecayRate = FindMatch(prefabId).KDecayRate / Rate;
                }

                QuickLogger.Debug($"Cooling {inventoryItem.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}", true);
            }
            _coolingState = FridgeCoolingState.Cooling;
        }

        private void CoolItem(InventoryItem item)
        {
            var eatable = item.item.GetComponent<Eatable>();
            var prefabId = item.item.GetComponent<PrefabIdentifier>().Id;

            //Store Data about the item
            var eatableEntity = new EatableEntities();
            eatableEntity.Initialize(item.item);
            FridgeItems.Add(eatableEntity);

            if (eatable.decomposes && _coolingState == FridgeCoolingState.Cooling)
            {
                eatable.kDecayRate = FindMatch(prefabId).KDecayRate / Rate;
                QuickLogger.Debug($"Cooling", true);
            }

            QuickLogger.Debug($"Tracker Count = {FridgeItems.Count}", true);

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

            if (!_isConstructed.Invoke())
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

        public void LoadFoodItems(IEnumerable<EatableEntities> savedDataFridgeContainer)
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

        internal IEnumerable<EatableEntities> GetSaveData()
        {
            foreach (EatableEntities eatableEntity in FridgeItems)
            {
                eatableEntity.SaveData();
                yield return eatableEntity;
            }
        }
    }
}
