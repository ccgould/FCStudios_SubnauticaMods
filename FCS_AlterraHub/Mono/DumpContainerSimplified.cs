using System;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class DumpContainerSimplified : MonoBehaviour
    {
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _dumpContainer;

        private IFCSDumpContainer _storage;

        public Action OnDumpContainerClosed { get; set; }

        public void Initialize(Transform trans, string label, IFCSDumpContainer storage, int width = 6, int height = 8, string name = "StorageRoot")
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
                pda.Open(PDATab.Inventory, null, OnDumpClose, 4f);
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

        public byte[] Save(ProtobufSerializer serializer)
        {
            if (serializer == null || _containerRoot == null)
            {
                QuickLogger.DebugError($"Failed to save: Serializer: {serializer} || Root {_containerRoot?.name}", true);
                return null;
            }
            return StorageHelper.Save(serializer, _containerRoot.gameObject);
        }

        public void RestoreItems(ProtobufSerializer serializer, byte[] serialData,bool runPDACloseWhenDone = false)
        {
            StorageHelper.RenewIdentifier(_containerRoot.gameObject);
            if (serialData == null)
            {
                return;
            }
            using (MemoryStream memoryStream = new MemoryStream(serialData))
            {
                QuickLogger.Debug("Getting Data from memory stream");
                GameObject gObj = serializer.DeserializeObjectTree(memoryStream, 0);
                QuickLogger.Debug($"Deserialized Object Stream. {gObj}");
                StorageHelper.TransferItems(gObj, _dumpContainer);
                Destroy(gObj);
            }

            CleanUpDuplicatedStorageNoneRoutine();

            if (runPDACloseWhenDone)
            {
                OnDumpClose(null);
            }

        }

        private void CleanUpDuplicatedStorageNoneRoutine()
        {
            QuickLogger.Debug("Cleaning Duplicates", true);
            Transform hostTransform = transform;
            StoreInformationIdentifier[] sids = gameObject.GetComponentsInChildren<StoreInformationIdentifier>(true);
#if DEBUG
            QuickLogger.Debug($"SIDS: {sids.Length}", true);
#endif

            int num;
            for (int i = sids.Length - 1; i >= 0; i = num - 1)
            {
                StoreInformationIdentifier storeInformationIdentifier = sids[i];
                if (storeInformationIdentifier != null && storeInformationIdentifier.name.StartsWith("SerializerEmptyGameObject", StringComparison.OrdinalIgnoreCase))
                {
                    Destroy(storeInformationIdentifier.gameObject);
                    QuickLogger.Debug($"Destroyed Duplicate", true);
                }
                num = i;
            }
        }

        public int GetItemCount()
        {
            return _dumpContainer.count;
        }
    }
}
