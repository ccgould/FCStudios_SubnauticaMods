using FCSAlterraShipping.Buildable;
using FCSAlterraShipping.Interfaces;
using FCSAlterraShipping.Mono;
using FCSCommon.Utilities;
using System;
using System.Linq;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSAlterraShipping.Models
{
    internal class AlterraShippingContainer : IContainer
    {
        private readonly Func<bool> _isContstructed;
        private readonly ChildObjectIdentifier _containerRoot = null;
        private ItemsContainer _container = null;
        private AlterraShippingTarget _target;
        private const int ContainerHeight = 8;
        private const int ContainerWidth = 6;
        internal const int MaxContainerSlots = ContainerHeight * ContainerWidth;
        public Action OnPDAClose { get; set; }
        public bool HasRoomFor(Pickupable pickupable)
        {
            return _container.HasRoomFor(pickupable);
        }

        internal int GetUsedSpace()
        {
            var items = _container.ToList();
            int amount = 0;
            foreach (InventoryItem item in items)
            {
#if SUBNAUTICA
                var itemSize = CraftData.GetItemSize(item.item.GetTechType());
#elif BELOWZERO
                var itemSize = TechData.GetItemSize(item.item.GetTechType());
#endif
                amount += itemSize.x * itemSize.y;
            }

            return amount;
        }

        public bool IsFull()
        {
            return _container.IsFull();
        }

        public int NumberOfItems => _container.count;

        public bool HasItems()
        {
            return _container.count > 0;
        }

        internal AlterraShippingContainer(AlterraShippingTarget target)
        {
            _isContstructed = () => target.IsConstructed;

            _target = target;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing StorageRoot");
                var storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.SetParent(target.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            CreateNewContainer();
        }

        private void CreateNewContainer()
        {
            if (_container == null)
            {
                QuickLogger.Debug("Initializing Container");
                _container = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    AlterraShippingBuildable.StorageLabel(), null);
                _container.Resize(ContainerWidth,ContainerHeight);
                _container.isAllowedToAdd += IsAllowedToAdd;
                _container.isAllowedToRemove += IsAllowedToRemove;

                _container.onAddItem += _target.OnAddItemEvent;
                _container.onRemoveItem += _target.OnRemoveItemEvent;
            }
        }
        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public void AddItem(InventoryItem item)
        {
            QuickLogger.Debug($"Adding {item.item.GetTechName()}");
            _container.UnsafeAdd(item);
        }

        public void RemoveItem(Pickupable item)
        {
            QuickLogger.Debug($"Removing {item.GetTechName()}");
            _container.RemoveItem(item, true);
        }

        public void RemoveItem(TechType item)
        {
            QuickLogger.Debug($"Removing {item}");
            _container.RemoveItem(item);
        }

        public void OpenStorage()
        {
            QuickLogger.Debug($"Storage Button Clicked", true);

            if (!_isContstructed.Invoke())
                return;

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_container, false);
            pda.Open(PDATab.Inventory, null, new PDA.OnClose(OnPDACloseMethod), 4f);
        }

        private void OnPDACloseMethod(PDA pda)
        {
            OnPDAClose?.Invoke();
        }

        public ItemsContainer GetContainer()
        {
            return _container;
        }

        public void ClearContainer()
        {

        }
    }
}
