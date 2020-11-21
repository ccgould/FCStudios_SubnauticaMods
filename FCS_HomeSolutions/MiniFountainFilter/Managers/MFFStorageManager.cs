using System;
using System.Collections.Generic;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.MiniFountainFilter.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.MiniFountainFilter.Managers
{
    internal class MFFStorageManager
    {
        private MiniFountainFilterController _mono;
        private Func<bool> _isConstructed;
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _container;
        private readonly int _containerWidth = QPatch.MiniFountainFilterConfiguration.StorageWidth;
        private readonly int _containerHeight = QPatch.MiniFountainFilterConfiguration.StorageHeight;
        private static TechType _bottleTechType = QPatch.MiniFountainFilterConfiguration.BottleTechType;
        private Vector2int _bottleSize;
        private readonly GameObject _bottle = CraftData.GetPrefabForTechType(_bottleTechType);
        private int MaxContainerSlots => _containerHeight * _containerWidth;
        private float _bottleWaterContent;
        private float _passedTime;

        internal int NumberOfBottles
        {
            get => _container.count;
            set
            {
                if (value < 0 || value > MaxContainerSlots)
                    return;

                if (value < _container.count)
                {
                    do
                    {
                        RemoveSingleBottle();
                    } while (value < _container.count);
                }
                else if (value > _container.count)
                {
                    do
                    {
                        SpawnBottle();
                    } while (value > _container.count);
                }
            }
        }

        public Action OnWaterAdded { get; set; }
        public Action OnWaterRemoved { get; set; }

        internal void Initialize(MiniFountainFilterController mono)
        {
            _mono = mono;

            if (_bottleTechType == TechType.None)
            {
                QuickLogger.Error($"TechType for Bottle TechType is None");
                return;
            }

            var bottle = _bottleTechType.ToInventoryItem();

            _bottleSize = new Vector2int(bottle.width, bottle.height);

            var eatable = bottle.item.transform.GetComponentInChildren<Eatable>();

            if (eatable == null)
            {
                QuickLogger.Error($"Eatable for Bottle TechType is null");
                GameObject.Destroy(bottle.item);
                return;
            }

            _bottleWaterContent = eatable.waterValue;
            QuickLogger.Debug($"Water Content Set To: {eatable.waterValue}");

            GameObject.Destroy(bottle.item);

            _isConstructed = () => mono.IsConstructed;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing StorageRoot");
                var storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_container != null) return;

            QuickLogger.Debug("Initializing Container");

            _container = new ItemsContainer(_containerWidth, _containerHeight, _containerRoot.transform,
                MiniFountainFilterBuildable.StorageLabel(), null);

            _container.isAllowedToAdd += IsAllowedToAddContainer;
            _container.onRemoveItem += ContainerOnRemoveItem;
        }

        internal void AttemptSpawnBottle()
        {
            if (!_mono.GetIsOperational() || !QPatch.MiniFountainFilterConfiguration.AutoGenerateMode) return;

            if (IsFull() || !_mono.TankManager.HasEnoughWater(_bottleWaterContent)) return;
            

            DayNightCycle main = DayNightCycle.main;

            _passedTime += DayNightCycle.main.deltaTime;

            if(_passedTime >= 2f)
            {
                _mono.TankManager.RemoveWater(_bottleWaterContent);
                NumberOfBottles++;
                _passedTime = 0;
            }
        }


        private void ContainerOnRemoveItem(InventoryItem item)
        {
            OnWaterRemoved?.Invoke();
        }
        
        private bool IsAllowedToAddContainer(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        internal bool CanDeconstruct()
        {
            return _container.count <= 0;
        }

        internal void LoadContainer(int waterBottleCount)
        {
            if (QPatch.MiniFountainFilterConfiguration.AutoGenerateMode) return;

            for (int i = 0; i < waterBottleCount; i++)
            {
                _container.UnsafeAdd(TechType.BigFilteredWater.ToInventoryItem());
            }

            OnWaterAdded?.Invoke();
        }

        internal bool IsFull()
        {
            return !_container.HasRoomFor(_bottleSize.x, _bottleSize.y);
        }

        internal void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_container, false);
            pda.Open(PDATab.Inventory, null, null, 4f);
        }

        private void RemoveSingleBottle()
        {
            IList<InventoryItem> bottle = _container.GetItems(_bottleTechType);
            _container.RemoveItem(bottle[0].item);
            OnWaterRemoved?.Invoke();
        }

        private void SpawnBottle()
        {
            var bottle = GameObject.Instantiate(_bottle);
#if SUBNAUTICA
            var newInventoryItem = new InventoryItem(bottle.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
            Pickupable pickupable = bottle.GetComponent<Pickupable>();
            pickupable.Pickup(false);
            var newInventoryItem = new InventoryItem(pickupable);
#endif
            _container.UnsafeAdd(newInventoryItem);
            OnWaterAdded?.Invoke();
        }

        internal void GivePlayerBottle()
        {
            if (_mono.TankManager.TankLevel >= _bottleWaterContent)
            {
                var pickup = _bottleTechType.ToPickupable();

                if (Inventory.main.HasRoomFor(pickup))
                {
                    _mono.TankManager.RemoveWater(_bottleWaterContent);
                    
                    Inventory.main.container.UnsafeAdd(_bottleTechType.ToInventoryItem());

                    uGUI_IconNotifier.main.Play(_bottleTechType, uGUI_IconNotifier.AnimationType.From, null);

                    pickup.PlayPickupSound();
                }
                else
                {
                    GameObject.Destroy(pickup);
                }
            }
        }
        
        public float ContainerPercentage()
        {
            return (float)_container.count / MaxContainerSlots;
        }
    }
}
