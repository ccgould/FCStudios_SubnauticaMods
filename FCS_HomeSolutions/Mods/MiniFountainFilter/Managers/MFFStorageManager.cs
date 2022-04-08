using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_HomeSolutions.Mods.MiniFountainFilter.Managers
{
    internal class MFFStorageManager
    {
        private MiniFountainFilterController _mono;
        private Func<bool> _isConstructed;
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _container;
        private readonly int _containerWidth = QPatch.Configuration.MiniFountainFilterStorageWidth;
        private readonly int _containerHeight = QPatch.Configuration.MiniFountainFilterStorageHeight;
        private int MaxContainerSlots => _containerHeight * _containerWidth;
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
            if (!_mono.IsOperational || !QPatch.Configuration.MiniFountainFilterAutoGenerateMode) return;

            if (IsFull() || !_mono.TankManager.HasEnoughWater(50)) return;

            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= 2f)
            {
                _mono.TankManager.RemoveWater(50);
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

#if !SUBNAUTICA_STABLE
        internal IEnumerator LoadContainer(int waterBottleCount)
        {
            if (QPatch.Configuration.MiniFountainFilterAutoGenerateMode) yield break;

            for (int i = 0; i < waterBottleCount; i++)
            {
                var result = new TaskResult<InventoryItem>();
                yield return TechType.BigFilteredWater.ToInventoryItem(result);
                _container.UnsafeAdd(result.value);
            }

            OnWaterAdded?.Invoke();
        }
#endif

        internal bool IsFull()
        {
            return !_container.HasRoomFor(1, 1);
        }

        internal void OpenStorage()
        {
            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_container, false);
            pda.Open(PDATab.Inventory, null, null
#if SUBNAUTICA_STABLE
                , 4f
#endif
            );
        }

        private void RemoveSingleBottle()
        {
            IList<InventoryItem> bottle = _container.GetItems(TechType.BigFilteredWater);
            _container.RemoveItem(bottle[0].item);
            OnWaterRemoved?.Invoke();
        }

        private void SpawnBottle()
        {
#if SUBNAUTICA_STABLE
            var newInventoryItem = TechType.BigFilteredWater.ToInventoryItem();
            _container.UnsafeAdd(newInventoryItem);
#else
            CoroutineHost.StartCoroutine(TechType.BigFilteredWater.AddTechTypeToContainerUnSafe(_container));
#endif
            OnWaterAdded?.Invoke();
        }

        internal void GivePlayerBottle()
        {
            if (_mono.TankManager.TankLevel >= 50)
            {
                var techType = TechType.BigFilteredWater;
                var result = PlayerInteractionHelper.GivePlayerItem(techType);
                if (result)
                {
#if SUBNAUTICA
                    //uGUI_IconNotifier.main.Play(techType, uGUI_IconNotifier.AnimationType.From, null);
                    FMODUWE.PlayOneShot(CraftData.GetPickupSound(techType), Player.main.transform.position, 1f);
#else
                    FMODAsset pickupSound = Player.main.GetPickupSound(TechData.GetSoundType(techType));
                    if (pickupSound)
                    {
                        Utils.PlayFMODAsset(pickupSound, Player.main.transform.position, 20f);
                    }
#endif
                    _mono.TankManager.RemoveWater(50);
                }

                //var pickup = TechType.BigFilteredWater.ToPickupable();

                //if (Inventory.main.HasRoomFor(1, 1))
                //{
                //    _mono.TankManager.RemoveWater(50);

                //    Inventory.main.container.UnsafeAdd(pickup.ToInventoryItem());

                //    uGUI_IconNotifier.main.Play(TechType.BigFilteredWater, uGUI_IconNotifier.AnimationType.From, null);

                //    pickup.PlayPickupSound();
                //}
                //else
                //{
                //    GameObject.Destroy(pickup);
                //}
            }
            else
            {
                QuickLogger.ModMessage(AuxPatchers.NotEnoughWaterForBottle());
            }
        }

        public float ContainerPercentage()
        {
            return (float) _container.count / MaxContainerSlots;
        }
    }
}