using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSSlotController : MonoBehaviour, IFCSDumpContainer, IHandTarget
    {
        private DumpContainerSimplified _dumpContainer;
        private InventoryItem _inventoryItem;
        private DSSWallServerRackController _controller;
        private string _slotName;
        private BoxCollider _collider;
        private FCSStorage _storage;
        private DSSServerController _mountedServer;
        public bool IsOccupied => transform.childCount > 1;

        private void Start()
        {
            _storage?.CleanUpDuplicatedStorageNoneRoutine();
        }

        private void Update()
        {
            if (_collider == null || _controller == null) return;
            _collider.isTrigger = !_controller.IsOpen;
        }

        internal void Initialize(string slotName,DSSWallServerRackController controller)
        {
            _collider = gameObject.GetComponent<BoxCollider>();
            _controller = controller;
            _slotName = slotName;
            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform,$"Add server to {_slotName}",this,1,1,gameObject.name);
            }


            if (_storage == null)
            {
                _storage = gameObject.AddComponent<FCSStorage>();
                _storage.Initialize(6,gameObject);
                _storage.ItemsContainer.onAddItem += ItemsContainerOnOnAddItem;
                _storage.ItemsContainer.onRemoveItem += ItemsContainerOnOnRemoveItem;
            }
        }

        private void ItemsContainerOnOnRemoveItem(InventoryItem item)
        {
            var server = item.item.gameObject.EnsureComponent<DSSServerController>();
            if (server == null)
            {
                QuickLogger.DebugError($"Server controller returned null on ItemsContainerOnOnAddItem. Object {item.item.gameObject.name}");
            }
            server?.UnDockServer();
            _mountedServer = null;
            _inventoryItem = null;
        }

        private void ItemsContainerOnOnAddItem(InventoryItem item)
        {
            ModelPrefab.ApplyShaders(item.item.gameObject);
            _inventoryItem = item;
            var server = item.item.gameObject.EnsureComponent<DSSServerController>();
            if (server == null)
            {
                QuickLogger.DebugError($"Server controller returned null on ItemsContainerOnOnAddItem. Object {item.item.gameObject.name}");
            }
            server?.DockServer(this, _controller);
        }

        public void OnHandHover(GUIHand hand)
        {
            if(_controller.IsOpen)
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Hand);
                main.SetInteractText(IsOccupied ? $"Remove server from {_slotName}" : $"Add server to {_slotName}");
            }
        }
        
        public void OnHandClick(GUIHand hand)
        {
            if (!_controller.IsOpen) return;
            if (IsOccupied)
            {
                PlayerInteractionHelper.GivePlayerItem(_inventoryItem);
            }
            else
            {
                _dumpContainer.OpenStorage();
            }
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _inventoryItem = item;
            _mountedServer = item.item.GetComponent<DSSServerController>();
            _mountedServer.DockServer(this, _controller);
            _controller.GetStorage().AddItem(item);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable.GetTechType() == Mod.GetDSSServerTechType();
        }

        public string GetSlotName()
        {
            return _slotName;
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            return _storage.Save(serializer);
        }

        public void RestoreItems(ProtobufSerializer serializer,byte[] data)
        {
            _storage.RestoreItems(serializer,data);
        }

        internal void SetIsVisible(bool value)
        {
            if (_mountedServer != null)
            {
                _mountedServer.IsVisible = value;
            }
        }
    }
}