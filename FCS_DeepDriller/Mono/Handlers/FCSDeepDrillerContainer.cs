using FCS_DeepDriller.Buildable;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_ExStorageDepot
#endif

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerContainer : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private ItemsContainer _container;
        private ChildObjectIdentifier _containerRoot;
        private Func<bool> _isConstructed;
        private int MaxContainerSlots => _containerHeight * _containerWidth;
        private int ContainerSlotsFilled => _container.count;
        internal bool IsContainerFull => _container.count == MaxContainerSlots || !_container.HasRoomFor(1, 1);
        private readonly int _containerWidth = FCSDeepDrillerBuildable.DeepDrillConfig.StorageSize.Width;
        private readonly int _containerHeight = FCSDeepDrillerBuildable.DeepDrillConfig.StorageSize.Height;
        private readonly Dictionary<TechType, int> _containerItemsTracker = new Dictionary<TechType, int>();


        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            _isConstructed = () => mono.IsConstructed;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Deep Driller StorageRoot");
                var storageRoot = new GameObject("DeepDrillerStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_container == null)
            {
                QuickLogger.Debug("Initializing Deep Driller Container");

                _container = new ItemsContainer(_containerWidth, _containerHeight, _containerRoot.transform,
                    FCSDeepDrillerBuildable.StorageContainerLabel(), null);
                _container.Resize(_containerWidth, _containerHeight);
                _container.isAllowedToAdd += IsAllowedToAdd;
                _container.onRemoveItem += OnRemoveItemEvent;

            }

            DayNightCycle main = DayNightCycle.main;

            //if (_timeSpawnMedKit < 0.0 && main)
            //{
            //    this._timeSpawnMedKit = (float)(main.timePassed + (!this.startWithMedKit ? (double)MedKitSpawnInterval : 0.0));
            //}
        }

        private void OnRemoveItemEvent(InventoryItem item)
        {
            //TODO if full reset system to generate ore
            var techType = item.item.GetTechType();

            if (!_containerItemsTracker.ContainsKey(techType)) return;

            if (_containerItemsTracker[techType] == 1)
            {
                _containerItemsTracker.Remove(techType);
            }
            else
            {
                _containerItemsTracker[techType] = _containerItemsTracker[techType] - 1;
            }
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        internal void AddItem(Pickupable pickupable)
        {
            var techType = pickupable.GetTechType();

            if (_containerItemsTracker.ContainsKey(techType))
            {
                _containerItemsTracker[techType] = _containerItemsTracker[techType] + 1;
            }
            else
            {
                _containerItemsTracker.Add(techType, 1);
            }

            QuickLogger.Debug($"Adding TechType to container: {techType}");
            _container.UnsafeAdd(new InventoryItem(pickupable));
        }

        public void OpenStorage()
        {
            if (_mono.IsInvalidPlacement()) return;


            if (_container == null)
            {
                QuickLogger.Debug("The container returned null", true);
                return;
            }
            var pda = Player.main.GetPDA();
            Inventory.main.SetUsedStorage(_container);
            pda.Open(PDATab.Inventory, _mono.transform, null, 10f);
        }

        public void LoadItems(IEnumerable<KeyValuePair<TechType, int>> savedDataFridgeContainer)
        {
            foreach (KeyValuePair<TechType, int> item in savedDataFridgeContainer)
            {
                for (var i = 0; i < item.Value; i++)
                {
                    AddItem(item.Key.ToPickupable());
                }
            }
        }

        internal IEnumerable<KeyValuePair<TechType, int>> GetItems()
        {
            foreach (var eatableEntity in _containerItemsTracker)
            {
                yield return eatableEntity;
            }
        }

        internal bool IsEmpty()
        {
            return _container.count <= 0;
        }

        public void SendToExStorage(InventoryItem inventoryItem)
        {
#if USE_ExStorageDepot
            foreach (InventoryItem item in _container)
            {
                //var successfulFlag = ExStorageDepotController.AddtoStorage(item, out var reason);

                //_mono.ExportStorage.Add

                //if (successfulFlag)
                //{

                //}
                //else
                //{
                //    QuickLogger.Info(reason);
                //}
            }
#endif
        }
    }
}
