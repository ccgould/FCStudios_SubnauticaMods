using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class DumpContainer : MonoBehaviour
    {
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _dumpContainer;

        private IFCSStorage _storage;

        public Action OnDumpContainerClosed { get; set; }

        public void Initialize(Transform trans, string label, IFCSStorage storage, int width = 6, int height = 8, string name = "StorageRoot")
        {
            _storage = storage;

            if (_containerRoot == null)
            {
                var storageRoot = new GameObject(name);
                storageRoot.transform.SetParent(trans, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_dumpContainer == null)
            {
                QuickLogger.Debug("Initializing Container");

                _dumpContainer = new ItemsContainer(width, height, _containerRoot.transform, label, null);

                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
                _dumpContainer.onAddItem += DumpContainerOnOnAddItem;
            }
        }

        public ItemsContainer GetItemsContainer()
        {
            return _dumpContainer;
        }

        public int GetTechTypeCount(TechType techType)
        {
            return _dumpContainer.GetItems(techType)?.Count ?? 0;
        }

        public IList<InventoryItem> GetTechType(TechType techType)
        {
            return _dumpContainer.GetItems(techType);
        }
        
        private void DumpContainerOnOnAddItem(InventoryItem item)
        {

        }

        public int GetCount()
        {
            return _dumpContainer.count;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var result = _storage.IsAllowedToAdd(pickupable, verbose);
            return result;
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
            QuickLogger.Debug($"Store Items Dump Count: {_dumpContainer.count}");
            OnDumpContainerClosed?.Invoke();
            var amount = _dumpContainer.count;

            for (int i = amount - 1; i > -1; i--)
            {
                QuickLogger.Debug($"Number of iteration: {i}");
                var item = _dumpContainer.ElementAt(0);
                _dumpContainer.RemoveItem(item.item, true);
                _storage.AddItemToContainer(item);
            }

        }
    }
}
