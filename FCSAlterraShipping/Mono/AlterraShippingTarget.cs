using FCSAlterraShipping.Display;
using FCSAlterraShipping.Interfaces;
using FCSAlterraShipping.Models;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCSAlterraShipping.Mono
{
    internal class AlterraShippingTarget : MonoBehaviour, IContainer, IConstructable
    {
        #region Private Members
        private Constructable _buildable = null;
        private IContainer _container;
        private AlterraShippingTransferHandler _transferHandler;
        #endregion

        #region Public Properties
        public string Name { get; set; } = "Shipping Box";
        public int ID { get; set; }
        public bool Recieved { get; set; }
        internal bool IsConstructed => _buildable != null && _buildable.constructed;
        public bool IsReceivingTransfer { get; set; }
        public Action OnReceivingTransfer;
        public Action OnItemSent;
        public bool IsFull(TechType techType)
        {
            return _container.IsFull(techType);
        }

        public int NumberOfItems
        {
            get => _container.NumberOfItems;
        }

        public ShippingTargetManager Manager { get; private set; }

        #endregion

        private void Awake()
        {
            ID = gameObject.GetInstanceID();

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            if (_transferHandler == null)
            {
                _transferHandler = GetComponentInParent<AlterraShippingTransferHandler>();
            }

            Name = $"Shipping Box {ShippingTargetManager.GlobalShippingTargets.Count}";

            if (_transferHandler == null)
            {
                QuickLogger.Error($"Transfer Handler is null.");
            }

            _container = new AlterraShippingContainer(this);

            InvokeRepeating("CargoContainer", 1, 0.5f);
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.ShippingTargets.Remove(this);
                Manager.UpdateGlobalTargets();
            }
            else
                ShippingTargetManager.RemoveShippingTarget(this);
        }

        internal AlterraShippingTransferHandler GetTransferHandler()
        {
            return _transferHandler;
        }
        public void OnAddItemEvent(InventoryItem item)
        {
            _buildable.deconstructionAllowed = false;
        }

        public void OnRemoveItemEvent(InventoryItem item)
        {
            _buildable.deconstructionAllowed = _container.NumberOfItems == 0;

        }

        public bool HasItems()
        {
            return _container.HasItems();
        }

        public void OpenStorage()
        {
            _container.OpenStorage();
        }

        public ItemsContainer GetContainer()
        {
            return _container.GetContainer();
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (IsReceivingTransfer) return false;
            return _buildable.deconstructionAllowed;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                var display = gameObject.GetOrAddComponent<AlterraShippingDisplay>();
                SubRoot root = GetComponentInParent<SubRoot>();

                if (root != null)
                {
                    QuickLogger.Debug(root.name);
                    AddToManager(root);
                    QuickLogger.Debug($"Added to manager");
                    gameObject.GetOrAddComponent<AlterraShippingDisplay>();
                }
                else
                {
                    QuickLogger.Error($"Root returned null");
                }
            }
        }

        private void CargoContainer()
        {
            if (gameObject == null || _container == null)
            {
                QuickLogger.Error($"The gameObject/Container is null.");
                return;
            }

            GameObject container = gameObject.FindChild("model")
                .FindChild("mesh_body")
                .FindChild("cargo_container")?.gameObject;

            if (container != null) container.SetActive(_container.NumberOfItems != 0);
        }

        public void AddToManager(SubRoot subRoot, ShippingTargetManager managers = null)
        {
            Manager = managers ?? ShippingTargetManager.FindManager(subRoot);
            Manager.AddShippingTarget(this);

            QuickLogger.Debug("Target has been connected", true);
        }

        public void TransferItems(AlterraShippingTarget target)
        {
            _transferHandler.SendItems(_container.GetContainer(), target);
        }

        public void ClearContainer()
        {
            _container.ClearContainer();
        }

        public void AddItem(InventoryItem item)
        {
            _container.AddItem(item);
        }

        public void RemoveItem(Pickupable item)
        {
            _container.RemoveItem(item);
        }

        public void RemoveItem(TechType item)
        {
            _container.RemoveItem(item);
        }

        public bool CanFit()
        {
            return _container.CanFit();
        }
    }
}
