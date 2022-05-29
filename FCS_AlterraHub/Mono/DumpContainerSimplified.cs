using System;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class DumpContainerSimplified : MonoBehaviour
    {
        private ItemsContainer _dumpContainer;

        private IFCSDumpContainer _storage;
        private GameObject _storageRoot;

        public Action OnDumpContainerClosed { get; set; }

        public void Initialize(Transform trans, string label, IFCSDumpContainer storage, int width = 6, int height = 8, string name = "StorageRoot")
        {
            _storage = storage;

            if (_storageRoot == null)
            {
                _storageRoot = new GameObject(name);
                _storageRoot.transform.SetParent(trans, false);
            }

            if (_dumpContainer == null)
            {
                QuickLogger.Debug("Initializing Container");

                _dumpContainer = new ItemsContainer(width, height, _storageRoot.transform, label, null);

                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
                _dumpContainer.onAddItem += DumpContainerOnAddItem;
            }
        }
        
        private void DumpContainerOnAddItem(InventoryItem item)
        {
            QuickLogger.Debug($"Adding {item.item.GetTechType()} to dump container");
            OnItemAdded?.Invoke(item);
        }

        public Action<InventoryItem> OnItemAdded { get; set; }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return _storage.IsAllowedToAdd(pickupable.GetTechType(), verbose) || _storage.IsAllowedToAdd(pickupable,verbose);
        }
        
        public void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            if (_dumpContainer != null && pda != null)
            {
                Inventory.main.SetUsedStorage(_dumpContainer);
#if SUBNAUTICA_STABLE
                pda.Open(PDATab.Inventory, null, OnDumpClose, 4f);
#else
                pda.Open(PDATab.Inventory, null, OnDumpClose);
#endif
            }
            else
            {
                QuickLogger.Error($"Failed to open the pda values: PDA = {pda} || Dump Container: {_dumpContainer}");
            }
        }

        internal virtual void OnDumpClose(PDA pda)
        {
            var amount = _dumpContainer.count;
            
            for (int i = amount - 1; i > -1; i--)
            {
                QuickLogger.Debug($"Number of iteration: {i}");
                var item = _dumpContainer.ElementAt(0);
                _dumpContainer.RemoveItem(item.item, true);
                _storage.AddItemToContainer(item);
            }
            
            QuickLogger.Debug($"Store Items Dump Count: {_dumpContainer.count}");
            
            OnDumpContainerClosed?.Invoke();
        }

        public int GetItemCount()
        {
            return _dumpContainer.count;
        }
    }
}
