using AE.SeaCooker.Buildable;
using AE.SeaCooker.Mono;
using ARS_SeaBreezeFCS32.Mono;
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
        private readonly int _containerWidth = QPatch.Configuration.Config.StorageWidth;
        private readonly int _containerHeight = QPatch.Configuration.Config.StorageHeight;
        private Func<bool> _isConstructed;
        private ChildObjectIdentifier _containerRoot = null;
        private bool _exportToSeaBreeze = true;
        private ChildObjectIdentifier _exportContainerRoot = null;
        private bool _lockInputContainer;

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

                _container = new ItemsContainer(_containerWidth, _containerHeight, _containerRoot.transform,
                    SeaCookerBuildable.StorageLabel(), null);

                _container.isAllowedToAdd += IsAllowedToAddContainer;
            }

            if (_exportContainer == null)
            {
                QuickLogger.Debug("Initializing Export Container");

                _exportContainer = new ItemsContainer(_containerWidth, _containerHeight, _exportContainerRoot.transform,
                    SeaCookerBuildable.ExportStorageLabel(), null);
                _exportContainer.isAllowedToAdd += IsAllowedToAddExport;

            }

            _mono.FoodManager.OnFoodCookedAll += OnFoodCookedAll;
            _mono.FoodManager.OnCookingStart += OnCookingStart;
        }

        private void OnCookingStart(TechType arg1, TechType arg2)
        {
            _lockInputContainer = true;
            _mono.AudioManager.PlayMachineAudio();
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

            #region Disabled because of cooking all food
            //var food = _container.FirstOrDefault();

            //if (food == null)
            //{
            //    QuickLogger.Debug("Food is null", true);
            //    return;
            //} 
            #endregion

            _mono.FoodManager.CookAllFood(_container);
        }

        private void OnFoodCookedAll(TechType oldTechType, List<TechType> cookedTechTypes)
        {
            _mono.AudioManager.StopMachineAudio();

            foreach (TechType type in cookedTechTypes)
            {
                var newFood = GameObject.Instantiate(CraftData.GetPrefabForTechType(type));

#if SUBNAUTICA
                var item = new InventoryItem(newFood.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
                Pickupable pickupable = newFood.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif

                _exportContainer.UnsafeAdd(item);

                QuickLogger.Debug($"Food {oldTechType} has been cooked", true);
            }

            if (_exportToSeaBreeze)
            {
                SendToSeaBreeze();
            }

            _container.Clear();
            _lockInputContainer = false;

            //if (_container.count > 0)
            //{
            //    CookStoredFood();
            //    return;
            //}

            _mono.DisplayManager.ToggleProcessDisplay(false);
            _mono.UpdateIsRunning(false);
        }

        private void SendToSeaBreeze()
        {
            QuickLogger.Debug($"Sending to SeaBreeze: Available {_mono.SeaBreezes.Count}", true);

            if (_mono.AutoChooseSeabreeze)
            {
                foreach (KeyValuePair<string, ARSolutionsSeaBreezeController> breezeController in _mono.SeaBreezes)
                {
                    QuickLogger.Debug($"Current SeaBreeze: {breezeController.Value.PrefabId.Id}", true);

                    if (_exportContainer.count <= 0) break;

                    if (breezeController.Value.CanBeStored(_exportContainer.count))
                    {
                        QuickLogger.Debug($"SeaBreeze {breezeController.Value.PrefabId.Id}: Has all {_exportContainer.count} Available.", true);

                        for (int i = _exportContainer.count - 1; i > -1; i--)
                        {
                            var item = _exportContainer.FirstOrDefault();
                            var result = breezeController.Value.AddItemToFridge(item, out string reason);
                            _exportContainer.RemoveItem(item?.item);
                            QuickLogger.Debug($"SeaBreeze {breezeController.Value.PrefabId.Id}: Operation successful {result}", true);
                        }

                        break;
                    }

                    if (breezeController.Value.FreeSpace > 0)
                    {
                        for (int i = breezeController.Value.FreeSpace - 1; i > -1; i--)
                        {
                            var item = _exportContainer.FirstOrDefault();
                            var result = breezeController.Value.AddItemToFridge(item, out string reason);
                            _exportContainer.RemoveItem(item?.item);
                        }
                    }
                }
            }
            else if (_mono.IsSebreezeSelected)
            {
                if (_mono.SelectedSeaBreeze.CanBeStored(_exportContainer.count))
                {
                    for (int i = _exportContainer.count - 1; i > -1; i--)
                    {
                        var item = _exportContainer.FirstOrDefault();
                        var result = _mono.SelectedSeaBreeze.AddItemToFridge(item, out string reason);
                        _exportContainer.RemoveItem(item?.item);
                        QuickLogger.Debug($"SeaBreeze {_mono.SelectedSeaBreeze.PrefabId.Id}: Operation successful {result}", true);
                    }
                }
            }
            
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
            if (_lockInputContainer)
            {
                QuickLogger.Info(SeaCookerBuildable.CookingCantOpen(), true);
                return;
            }
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
#if SUBNAUTICA
                _exportContainer.UnsafeAdd(new InventoryItem(food.GetComponent<Pickupable>().Pickup(false)));
#elif BELOWZERO
                Pickupable pickupable = food.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                _container.UnsafeAdd(new InventoryItem(pickupable));
#endif
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
#if SUBNAUTICA
                _container.UnsafeAdd(new InventoryItem(food.GetComponent<Pickupable>().Pickup(false)));
#elif BELOWZERO
                Pickupable pickupable = food.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                _container.UnsafeAdd(new InventoryItem(pickupable));
#endif
            }
        }

        internal void SetExportToSeabreeze(bool value)
        {
            _exportToSeaBreeze = value;
            _mono.DisplayManager.SetSendToSeaBreeze(value);
        }

        internal bool GetExportToSeabreeze()
        {
            return _exportToSeaBreeze;
        }

        internal bool CanDeconstruct()
        {
            return _exportContainer.count <= 0 && _container.count <= 0;
        }

        internal bool HasRoomForAll()
        {
            List<Vector2int> items = _container.Select(item => new Vector2int(item.width, item.height)).ToList();
            if (_exportContainer.HasRoomFor(items)) return true;
            QuickLogger.Info(SeaCookerBuildable.NoEnoughRoom(), true);
            return false;

        }

        internal ItemsContainer GetContainer()
        {
            return _container;
        }
    }
}
