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
        public int NumberOfItems { get; set; }

        private const int ContainerWidth = 6;

        private const int ContainerHeight = 8;

        internal const int MaxAvailableSpaces = ContainerHeight * ContainerWidth;

        private ItemsContainer _fridgeContainer = null;

        private ChildObjectIdentifier _containerRoot = null;

        private Func<bool> isContstructed;

        private static List<EatableEntities> _fridgeItems =
            new List<EatableEntities>();

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

        private void OnRemoveItemEvent(InventoryItem item)
        {
            var eatable = item.item.GetComponent<Eatable>();

            QuickLogger.Debug($"Item Name: {eatable.name}", true);

            foreach (EatableEntities eatableEntity in _fridgeItems)
            {
                if (eatableEntity.Name == eatable.name && eatableEntity.FoodValue == eatable.GetFoodValue() &&
                    eatableEntity.WaterValue == eatable.GetWaterValue())
                {
                    QuickLogger.Debug($"Match Found", true);
                    _fridgeItems.Remove(eatableEntity);
                }
            }

            QuickLogger.Debug($"Fridge Item Count: {_fridgeItems.Count}", true);
        }

        private void OnAddItemEvent(InventoryItem item)
        {
            var eatable = item.item.GetComponent<Eatable>();

            if (eatable.decomposes)
            {

            }

            QuickLogger.Debug($"Item Name: {eatable.name}", true);



            _fridgeItems.Add(new EatableEntities
            {
                FoodValue = eatable.GetFoodValue(),
                WaterValue = eatable.GetWaterValue(),
                Name = eatable.name
            });

            QuickLogger.Debug($"Fridge Item Count: {_fridgeItems.Count}", true);
        }

        #endregion

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
                ErrorMessage.AddMessage("[Alterra Refrigeration] food allowed only");
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
            pda.Open(PDATab.Inventory, null, null, 4f);
        }

        internal List<EatableEntities> GetFridgeItems()
        {
            return _fridgeItems;
        }
    }
}
