using System;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCSTechFabricator.Components
{
    public class DumpContainer : MonoBehaviour
    {
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _dumpContainer;
        private string _itemNotAllowedMessage;
        private string _storageIsFullMessage;
        private IFCSStorage _storage;

        public void Initialize(Transform trans,string label, string itemNotAllowedMessage,string storageIsFullMessage, IFCSStorage storage, int width = 6, int height = 8, string name="StorageRoot")
        {
            _itemNotAllowedMessage = itemNotAllowedMessage;
            _storageIsFullMessage = storageIsFullMessage;
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

                _dumpContainer = new ItemsContainer(width, height, _containerRoot.transform,label, null);

                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
                _dumpContainer.onAddItem += DumpContainerOnOnAddItem;
            }
        }

        private void DumpContainerOnOnAddItem(InventoryItem item)
        {
            _storage.AddItemToContainer(item);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return _storage.IsAllowedToAdd(pickupable, verbose);
        }

        public void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_dumpContainer, false);
            pda.Open(PDATab.Inventory, null, OnDumpClose, 4f);
        }

        internal virtual void OnDumpClose(PDA pda)
        {
            
        }
    }
}
