using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSTechFabricator.Components
{
    public class FridgeDumpContainer : MonoBehaviour
    {
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _dumpContainer;
        private string _itemNotAllowedMessage;
        private string _storageIsFullMessage;
        private Fridge _fridge;

        public void Initialize(Transform trans,string label, string itemNotAllowedMessage,string storageIsFullMessage, Fridge fridge, int width = 6, int height = 8, string name="StorageRoot")
        {
            _itemNotAllowedMessage = itemNotAllowedMessage;
            _storageIsFullMessage = storageIsFullMessage;
            _fridge = fridge;

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
            _fridge.AddItem(item);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;

            if (_fridge.IsFull)
            {
                QuickLogger.Message(_storageIsFullMessage, true);
                return false;
            }

            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                QuickLogger.Debug(techType.ToString());

                if (pickupable.GetComponent<Eatable>() != null)
                    flag = true;
            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
            {
                QuickLogger.Message(_itemNotAllowedMessage,true);
                flag =  false;
            }
            
            return flag;
        }

        public void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_dumpContainer);
            pda.Open(PDATab.Inventory, null, OnFridgeClose, 4f);
        }

        private void OnFridgeClose(PDA pda)
        {
            foreach (InventoryItem item in _dumpContainer)
            {
                
                GameObject.Destroy(item.item.gameObject);
            }
        }
    }
}
