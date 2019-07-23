using FCS_DeepDriller.Buildable;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        private Dictionary<TechType, int> ContainerItemsTracker = new Dictionary<TechType, int>();


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

            if (!ContainerItemsTracker.ContainsKey(techType)) return;

            if (ContainerItemsTracker[techType] == 1)
            {
                ContainerItemsTracker.Remove(techType);
            }
            else
            {
                ContainerItemsTracker[techType] = ContainerItemsTracker[techType] - 1;
            }
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        internal void AddItem(Pickupable pickupable)
        {

            var techType = pickupable.GetTechType();

            if (ContainerItemsTracker.ContainsKey(techType))
            {
                ContainerItemsTracker[techType] = ContainerItemsTracker[techType] + 1;
            }
            else
            {
                ContainerItemsTracker.Add(techType, 1);
            }

            QuickLogger.Debug($"Adding TechType to container: {techType}");
            _container.UnsafeAdd(new InventoryItem(pickupable));
        }

        public void OpenStorage()
        {
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
            foreach (var eatableEntity in ContainerItemsTracker)
            {
                yield return eatableEntity;
            }
        }
    }
}
