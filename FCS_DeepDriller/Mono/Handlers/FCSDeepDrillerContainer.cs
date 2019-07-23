using FCS_DeepDriller.Buildable;
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
        private int MaxContainerSlots => ContainerHeight * ContainerWidth;
        private int ContainerSlotsFilled => _container.count;
        internal bool IsContainerFull => _container.count == MaxContainerSlots || !_container.HasRoomFor(1, 1);
        private const int ContainerWidth = 8;
        private const int ContainerHeight = 10;

        private readonly List<TechType> _allowedResources = new List<TechType> {
            TechType.Copper,
            TechType.Gold,
            TechType.Lead,
            TechType.Lithium,
            TechType.Magnetite,
            TechType.Nickel,
            TechType.Silver,
            TechType.Titanium,
            TechType.AluminumOxide,
            TechType.Diamond,
            TechType.Kyanite,
            TechType.Quartz,
            TechType.UraniniteCrystal,
            TechType.Sulphur
        };

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

                _container = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
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
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                if (_allowedResources.Contains(techType))
                    flag = true;
            }
            if (!flag && verbose)
                QuickLogger.Info(FCSDeepDrillerBuildable.ItemNotAllowed(), true);

            return flag;
        }

        internal void AddItem(Pickupable pickupable)
        {
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
    }
}
