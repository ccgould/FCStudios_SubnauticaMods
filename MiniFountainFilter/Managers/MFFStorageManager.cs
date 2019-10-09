using AE.MiniFountainFilter.Buildable;
using AE.MiniFountainFilter.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AE.MiniFountainFilter.Managers
{
    internal class MFFStorageManager
    {
        public bool startWithMedKit;
        private MiniFountainFilterController _mono;
        private Func<bool> _isConstructed;
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _container;
        private readonly int _containerWidth = QPatch.Configuration.Config.StorageWidth;
        private readonly int _containerHeight = QPatch.Configuration.Config.StorageHeight;
        private static TechType _bottleTechType = QPatch.BottleTechType; //QPatch.Configuration.Config.BottleTechType.ToTechType();
        private Vector2int _bottleSize;
        private readonly GameObject _bottle = CraftData.GetPrefabForTechType(_bottleTechType);
        private int MaxContainerSlots => _containerHeight * _containerWidth;
        private const float SpawnInterval = 840f;
        private float _timeSpawnBottle = -1f;
        private float _progress;
        private float _bottleWaterContent;

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

            _mono.OnMonoUpdate += OnMonoUpdate;

            DayNightCycle main = DayNightCycle.main;

            if (_timeSpawnBottle < 0.0 && main)
            {
                _timeSpawnBottle = (float)(main.timePassed + (!startWithMedKit ? SpawnInterval : 0.0));
            }
        }

        private void OnMonoUpdate()
        {
            if (!_mono.GetIsOperational() || !QPatch.Configuration.Config.AutoGenerateMode) return;

            if (IsFull()) return;

            DayNightCycle main = DayNightCycle.main;

            float a = _timeSpawnBottle - SpawnInterval;
            _progress = Mathf.InverseLerp(a, a + SpawnInterval, DayNightCycle.main.timePassedAsFloat);

            if (main.timePassed > _timeSpawnBottle)
            {
                NumberOfBottles++;
            }
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
            if (QPatch.Configuration.Config.AutoGenerateMode) return;

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
            IList<InventoryItem> bottle = _container.GetItems(TechType.PrecursorIonCrystal);
            _container.RemoveItem(bottle[0].item);
            OnWaterRemoved?.Invoke();
        }

        private void SpawnBottle()
        {
            var bottle = GameObject.Instantiate(_bottle);
            var newInventoryItem = new InventoryItem(bottle.GetComponent<Pickupable>().Pickup(false));
            _container.UnsafeAdd(newInventoryItem);
            _timeSpawnBottle = DayNightCycle.main.timePassedAsFloat + SpawnInterval;
            OnWaterAdded?.Invoke();
        }

        internal void GivePlayerBottle()
        {
            if (_mono.TankManager.TankLevel >= _bottleWaterContent)
            {
                var pickup = _bottleTechType.ToPickupable();

                if (Inventory.main.Pickup(pickup))
                {
                    _mono.TankManager.RemoveWater(_bottleWaterContent);
                    CrafterLogic.NotifyCraftEnd(Player.main.gameObject, _bottleTechType);
                }
                else
                {
                    GameObject.Destroy(pickup);
                }
            }
        }

        internal float GetTimeToSpawn()
        {
            return _timeSpawnBottle;
        }

        internal void SetTimeToSpawn(float value)
        {
            _timeSpawnBottle = value;
        }

        internal float GetProgress()
        {
            return _progress;
        }

        public float ContainerPercentage()
        {
            return (float)_container.count / MaxContainerSlots;
        }
    }
}
