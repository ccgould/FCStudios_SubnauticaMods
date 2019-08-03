using ExStorageDepot.Buildable;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotStorageManager : MonoBehaviour
    {
        public List<ItemData> StorageContainer { get; } = new List<ItemData>();
        public readonly SortedDictionary<TechType, int> TrackedItems = new SortedDictionary<TechType, int>();
        internal Action<TechType> OnAddItem;
        internal Action<TechType> OnRemoveItem;
        private ChildObjectIdentifier _containerRoot;
        private StorageContainer _container;
        private int _containerWidth = 5;
        private int _containerHeight = 30;
        private bool _containerHasItems;
        private ExStorageDepotController _mono;
        private int _multiplier;
        private float _timerTillNextPickup = .0f;
        private float COOLDOWN_TIME_BETWEEN_PICKING_UP_LAST_ITEM_TYPE = 1f;
        private int _dumpContainerWidth = 8;
        private int _dumpContainerHeight = 10;
        private ItemsContainer _dumpContainer;
        private const int MaxItems = 640;
        private List<Vector2int> _blukSlots = new List<Vector2int>();

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing AMMiniMedBay StorageRoot");
                var storageRoot = new GameObject("AMMiniMedBayStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _mono = mono;
            }

            if (_dumpContainer == null)
            {
                QuickLogger.Debug("Initializing Dump Container");

                _dumpContainer = new ItemsContainer(_dumpContainerWidth, _dumpContainerHeight, _containerRoot.transform,
                    ExStorageDepotBuildable.DumpContainerLabel(), null);
                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
                _dumpContainer.onAddItem += OnDumpAddItemEvent;
                _dumpContainer.onRemoveItem += OnDumpRemoveItemEvent;
            }

            if (_container == null)
            {
                QuickLogger.Debug("Initializing Storage Container");

                _container = _mono.gameObject.GetComponentInChildren<StorageContainer>();
                _container.width = _containerWidth;
                _container.height = _containerHeight;
                _container.container.onRemoveItem += OnSCRemoveItemEvent;
                _container.container.onAddItem += OnSCAddItemEvent;
                _container.container.Resize(_containerWidth, _containerHeight);
                _container.storageLabel = ExStorageDepotBuildable.StorageContainerLabel();
            }
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            Vector2int itemSize = CraftData.GetItemSize(pickupable.GetTechType());
            _blukSlots.Add(itemSize);

            if (!_container.container.HasRoomFor(_blukSlots))
            {
                _blukSlots.Remove(itemSize);
                QuickLogger.Info($"No space for a {itemSize.x} x {itemSize.y} item", true);
                return false;
            }
            return true;
        }

        private void OnSCAddItemEvent(InventoryItem item)
        {
            int amountToAdd = 1;
            var techType = item.item.GetTechType();

            if (TrackedItems.ContainsKey(techType))
            {
                TrackedItems[techType] = TrackedItems[techType] + amountToAdd;
            }
            else
            {
                TrackedItems.Add(techType, 1);
            }

            //RemoveItemSize(item.item);

            _mono.Display?.ItemModified(techType, TrackedItems[techType]);
        }

        private void OnSCRemoveItemEvent(InventoryItem item)
        {
            int amountToRemove = 1;
            var techType = item.item.GetTechType();

            if (TrackedItems.ContainsKey(techType))
            {
                int newAmount = TrackedItems[techType] - amountToRemove;
                TrackedItems[techType] = newAmount;

                if (newAmount <= 0)
                {
                    TrackedItems.Remove(techType);
                }

                _mono.Display?.ItemModified(techType, newAmount);
            }

        }

        private void OnDumpRemoveItemEvent(InventoryItem item)
        {
            //_containerHasItems = _container.container.count > 0;
            //var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;

            //var itemMatch = _dumpContainer.SingleOrDefault(x => x.PrefabId == prefabId);

            //if (itemMatch == null) return;

            //_dumpContainer.Remove(itemMatch);
        }

        private void OnDumpAddItemEvent(InventoryItem item)
        {
            //_containerHasItems = true;
            //var techType = item.item.GetTechType();

            ////var itemMatch = _dumpContainer.SingleOrDefault(x => x.TechType == techType);

            ////if (itemMatch != null) return;
            //var newItem = new ItemData { InventoryItem = item };
            //newItem.ExposeInventoryData();
            //_dumpContainer.Add(newItem);
        }

        internal bool AddItem(InventoryItem item)
        {
            if (GetItemsCount() == MaxItems)
            {
                QuickLogger.Message(ExStorageDepotBuildable.ContainerFullMessage(), true);
                return false;
            }
            _containerHasItems = true;

            var techType = item.item.GetTechType();

            if (TrackedItems.ContainsKey(techType))
            {
                TrackedItems[techType] = TrackedItems[techType] + 1;
            }
            else
            {
                TrackedItems.Add(techType, 1);
            }

            //var itemMatch = _storageContainer.SingleOrDefault(x => x.TechType == techType);

            //if (itemMatch != null) return;
            var newItem = new ItemData { InventoryItem = item };
            newItem.ExposeInventoryData();
            StorageContainer.Add(newItem);

            _mono.Display.ItemModified(techType, TrackedItems[techType]);

            QuickLogger.Debug($"Added TechType {techType} to the container.", true);

            return true;
        }

        internal void RemoveItem(Pickupable item)
        {
            var prefabId = item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            var techType = item.GetTechType();

            var itemMatch = StorageContainer.SingleOrDefault(x => x.PrefabId == prefabId);

            if (itemMatch != null)
            {
                StorageContainer.Remove(itemMatch);
            }

            if (TrackedItems.ContainsKey(techType))
            {
                if (TrackedItems[techType] != 1)
                {
                    TrackedItems[techType] = TrackedItems[techType] - 1;
                }
                else
                {
                    TrackedItems.Remove(techType);
                }
            }

            _mono.Display.ItemModified(techType, TrackedItems[techType]);

            QuickLogger.Debug($"Removed TechType {techType} from the container.");
        }

        internal int GetTechTypeCount(TechType techType)
        {
            return StorageContainer.Count(x => x.TechType == techType);
        }

        internal int GetItemsCount()
        {
            return StorageContainer.Count;
        }

        internal void OpenStorage()
        {
            QuickLogger.Debug($"Dump Button Clicked", true);
            QuickLogger.Debug($"Container Slot Count = {_container.width * _container.height}", true);
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_dumpContainer, false);
            pda.Open(PDATab.Inventory, null, new PDA.OnClose(OnPDACloseMethod), 4f);
            _mono.AnimationManager.ToggleDriveState();
        }

        private void OnPDACloseMethod(PDA pda)
        {
            _mono.AnimationManager.ToggleDriveState();

            StartCoroutine(AddItems());
        }


        private IEnumerator AddItems()
        {
            yield return new WaitForEndOfFrame();

            foreach (var item in _dumpContainer.ToList())
            {
                //if (!_container.container.HasRoomFor(item.item)) continue;
                _dumpContainer.RemoveItem(item.item.Pickup(false));
                RemoveItemSize(item.item);
                _container.container.AddItem(item.item);
                //yield return null;
            }

        }

        private void RemoveItemSize(Pickupable pickupable)
        {
            Vector2int itemSize = CraftData.GetItemSize(pickupable.GetTechType());
            _blukSlots.Remove(itemSize);
        }

        public void SetMultiplier(int value)
        {
            _multiplier = value;
        }

        public void AttemptToTakeItem(TechType techType)
        {
            QuickLogger.Debug($"Attempting to take item {techType}", true);


            if (_timerTillNextPickup > 0f)
            {
                return;
            }

            if (TrackedItems.ContainsKey(techType))
            {
                //var trackedResource = TrackedItems[techType];
                int beforeRemoveAmount = TrackedItems.Count;
                if (TrackedItems.Count >= 1)
                {
                    //StorageContainer sc = trackedResource.Containers.ElementAt(0);
                    if (_container.container.Contains(techType))
                    {
                        Pickupable pickup = _container.container.RemoveItem(techType);
                        if (pickup != null)
                        {
                            if (Inventory.main.Pickup(pickup))
                            {
                                CrafterLogic.NotifyCraftEnd(Player.main.gameObject, techType);
                                if (beforeRemoveAmount == 1)
                                {
                                    _timerTillNextPickup = COOLDOWN_TIME_BETWEEN_PICKING_UP_LAST_ITEM_TYPE;
                                }
                            }
                            else
                            {
                                // If it fails to get added to the inventory lets add it back into the storage container.
                                _container.container.AddItem(pickup);
                            }
                        }
                    }
                }
            }
        }
    }
}
