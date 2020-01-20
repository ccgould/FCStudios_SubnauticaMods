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
        private ARSolutionsSeaBreezeController _mono;
        private readonly Func<bool> _isConstructed;
        private ItemsContainer _DumpContainer;
        private const int ContainerWidth = 6;
        private const int ContainerHeight = 8;
        private readonly ChildObjectIdentifier _containerRoot = null;
        public Action<int, int> OnContainerUpdate { get; set; }
        public bool IsFull => NumberOfItems >= QPatch.Configuration.StorageLimit;

        public int NumberOfItems => FridgeItems.Count;
        public void AttemptToTakeItem(TechType techType)
        {
            RemoveItem(techType);
        }

        public List<EatableEntities> FridgeItems { get; } = new List<EatableEntities>();
        public Dictionary<TechType, int> TrackedItems { get; } = new Dictionary<TechType, int>();

        public ARSolutionsSeaBreezeContainer(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;
            _isConstructed = () => mono.IsConstructed;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing StorageRoot");
                var storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_DumpContainer == null)
            {
                QuickLogger.Debug("Initializing Container");

                _DumpContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    ARSSeaBreezeFCS32Buildable.StorageLabel(), null);

                _DumpContainer.isAllowedToAdd += IsAllowedToAdd;
            }

        }

        public int GetTechTypeAmount(TechType techType)
        {
            throw new System.NotImplementedException();
        }



        #region Container Methods

        private void AddItem(InventoryItem item)
        {
            FreezeItem(item);

            if (!_isConstructed.Invoke()) return;
            {
                _mono.SetDeconstructionAllowed(false);
                _mono.Display.ItemModified(TechType.None);
            }
            OnContainerUpdate?.Invoke(NumberOfItems, QPatch.Configuration.StorageLimit);

            QuickLogger.Debug($"Fridge Item Count: {FridgeItems.Count}", true);
        }

        internal void RemoveItem(TechType techType)
        {
            EatableEntities match = FindMatch(techType);

            if (match != null)
            {
                var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(techType));
                var eatable = go.GetComponent<Eatable>();
                var pickup = go.GetComponent<Pickupable>();

                eatable.foodValue = match.FoodValue;
                eatable.waterValue = match.WaterValue;

                if (Inventory.main.Pickup(pickup))
                {
                    QuickLogger.Debug($"Removed Match Before || Fridge Count {FridgeItems.Count}");
                    FridgeItems.Remove(match);
                    QuickLogger.Debug($"Removed Match || Fridge Count {FridgeItems.Count}");

                    CrafterLogic.NotifyCraftEnd(Player.main.gameObject, techType);

                    if (TrackedItems.ContainsKey(techType))
                    {
                        if (TrackedItems[techType] != 1)
                        {
                            TrackedItems[techType] = TrackedItems[techType] - 1;
                        }
                        else
                        {
                            TrackedItems.Remove(techType);
                        }
                    }

                    _mono.SetDeconstructionAllowed(NumberOfItems == 0);

                    _mono.Display.ItemModified(TechType.None);
                }

                OnContainerUpdate?.Invoke(NumberOfItems, QPatch.Configuration.StorageLimit);
            }
        }

        public void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_DumpContainer, false);
            pda.Open(PDATab.Inventory, null, OnFridgeClose, 4f);
        }

        private void OnFridgeClose(PDA pda)
        {
            foreach (InventoryItem item in _DumpContainer)
            {
                AddItem(item);
                GameObject.Destroy(item.item.gameObject);
            }
        }

        private EatableEntities FindMatch(TechType techType)
        {
            foreach (EatableEntities eatableEntity in FridgeItems)
            {
                if (eatableEntity.TechType == techType)
                {
                    return eatableEntity;
                }
            }

            return null;
        }

        #endregion

        private void FreezeItem(InventoryItem item)
        {
            var techType = item.item.GetTechType();

            if (TrackedItems.ContainsKey(techType))
            {
                TrackedItems[techType] = TrackedItems[techType] + 1;
            }
            else
            {
                TrackedItems.Add(techType, 1);
            }

            var eatableEntity = new EatableEntities();
            eatableEntity.Initialize(item.item);
            FridgeItems.Add(eatableEntity);
        }

        #region Internal Methods

        internal IEnumerable<EatableEntities> GetSaveData()
        {
            foreach (EatableEntities eatableEntity in FridgeItems)
            {
                yield return eatableEntity;
            }
        }

        internal void LoadFoodItems(IEnumerable<EatableEntities> savedDataFridgeContainer)
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

#if SUBNAUTICA
                var item = new InventoryItem(food.gameObject.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
                Pickupable pickupable = food.gameObject.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif

                AddItem(item);

                QuickLogger.Debug(
                    $"Load Item {item.item.name}|| Decompose: {eatable.decomposes} || DRate: {eatable.kDecayRate}");
            }
        }

        #endregion

        #region Condition Checking
        internal bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (pickupable.GetComponent<Eatable>() != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                QuickLogger.Error(ARSSeaBreezeFCS32Buildable.ItemNotAllowed());
            return flag;
        }
        #endregion

        internal void AddItemFromExternal(InventoryItem item)
        {
            AddItem(item);
        }

        internal bool HasRoomFor(int amount)
        {
            return NumberOfItems + amount <= QPatch.Configuration.StorageLimit;
        }
    }
}