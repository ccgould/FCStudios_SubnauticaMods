using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class SCStorageManager
    {
        private SeaCookerController _mono;
        private ItemsContainer _container;
        private ItemsContainer _exportContainer;
        private const int ContainerWidth = 6;
        private const int ContainerHeight = 8;
        private Func<bool> _isConstructed;
        private ChildObjectIdentifier _containerRoot = null;
        private bool _exportToSeabreeze;
        private ChildObjectIdentifier _exportContainerRoot = null;

        public void Initialize(SeaCookerController mono)
        {
            _mono = mono;

            _isConstructed = () => mono.IsConstructed;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing StorageRoot");
                var storageRoot = new GameObject("StorageRoot");
                var exportStorageRoot = new GameObject("ExportStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _exportContainerRoot = exportStorageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_container == null)
            {
                QuickLogger.Debug("Initializing Container");

                _container = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    SeaCookerBuildable.StorageLabel(), null);

                _container.isAllowedToAdd += IsAllowedToAddContainer;
            }

            if (_exportContainer == null)
            {
                QuickLogger.Debug("Initializing Export Container");

                _exportContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _exportContainerRoot.transform,
                    SeaCookerBuildable.ExportStorageLabel(), null);
                _exportContainer.isAllowedToAdd += IsAllowedToAddExport;

            }

            _mono.FoodManager.OnFoodCooked += OnFoodCooked;
        }

        private bool IsAllowedToAddExport(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        private bool IsAllowedToAddContainer(Pickupable pickupable, bool verbose)
        {
            bool flag = false;

            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (pickupable.GetComponent<Eatable>() != null)
                {
                    flag = Configuration.Configuration.AllowedRawItems.Any(x => x == techType);
                }
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                QuickLogger.Error(SeaCookerBuildable.ItemNotAllowed());
            return flag;
        }
        public void OpenInputStorage()
        {
            OpenStorage(_container);
        }

        internal void CookStoredFood()
        {
            QuickLogger.Debug("In CookStoredFood", true);

            if (!HasItemsToCook())
            {
                QuickLogger.Info(SeaCookerBuildable.NoFoodToCook(), true);
                return;
            }
            var food = _container.FirstOrDefault();

            if (food == null)
            {
                QuickLogger.Debug("Food is null", true);
                return;
            }

            _mono.FoodManager.CookFood(food);
        }

        private void OnFoodCooked(TechType oldTechType, TechType newTechType)
        {
            QuickLogger.Debug($"Food {oldTechType} has been cooked", true);

            _container.RemoveItem(oldTechType);

            var newFood = GameObject.Instantiate(CraftData.GetPrefabForTechType(newTechType));

            var item = new InventoryItem(newFood.GetComponent<Pickupable>().Pickup(false));

            _exportContainer.UnsafeAdd(item);

            if (_container.count > 0)
            {
                CookStoredFood();
                return;
            }

            _mono.DisplayManager.ToggleProcessDisplay(false);
            _mono.UpdateIsRunning(false);
        }

        internal bool HasItemsToCook()
        {
            return _container?.count > 0;
        }

        internal void OpenExportStorage()
        {
            OpenStorage(_exportContainer);
        }

        private void OpenStorage(ItemsContainer container)
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(container, false);
            pda.Open(PDATab.Inventory, null, null, 4f);
        }

        public bool HasRoomFor(Pickupable item)
        {
            var result = _exportContainer.HasRoomFor(item);
            QuickLogger.Debug($"Has Room For {item.GetTechType()} : {result}", true);
            return result;
        }

        internal IEnumerable<EatableEntities> GetExportContainer()
        {
            foreach (InventoryItem item in _exportContainer)
            {
                CreateFoodInventoryItem(item.item);
                yield return CreateFoodInventoryItem(item.item);
            }
        }

        internal IEnumerable<EatableEntities> GetInputContainer()
        {
            foreach (InventoryItem item in _container)
            {

                yield return CreateFoodInventoryItem(item.item); ;
            }
        }

        private EatableEntities CreateFoodInventoryItem(Pickupable item)
        {
            var eatable = new EatableEntities();
            eatable.Initialize(item, false);
            return eatable;
        }

        internal void LoadExportContainer(IEnumerable<EatableEntities> dataExport)
        {
            foreach (EatableEntities eatableEntity in dataExport)
            {
                var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(eatableEntity.TechType));
                var food = go.GetComponent<Eatable>();
                food.waterValue = eatableEntity.WaterValue;
                food.foodValue = eatableEntity.FoodValue;
                _exportContainer.UnsafeAdd(new InventoryItem(food.GetComponent<Pickupable>().Pickup(false)));
            }
        }

        internal void LoadInputContainer(IEnumerable<EatableEntities> dataInput)
        {
            foreach (EatableEntities eatableEntity in dataInput)
            {
                var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(eatableEntity.TechType));
                var food = go.GetComponent<Eatable>();
                food.waterValue = eatableEntity.WaterValue;
                food.foodValue = eatableEntity.FoodValue;
                _container.UnsafeAdd(new InventoryItem(food.GetComponent<Pickupable>().Pickup(false)));
            }
        }

        internal void SetExportToSeabreeze(bool value)
        {
            _exportToSeabreeze = value;
        }

        internal bool GetExportToSeabreeze()
        {
            return _exportToSeabreeze;
        }

        internal bool CanDeconstruct()
        {
            return _exportContainer.count <= 0 && _container.count <= 0;
        }
    }
}
