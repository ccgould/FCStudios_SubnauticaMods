using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.Converters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.AlienChef.Mono;
using FCS_HomeSolutions.Mods.Stove.Struct;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Stove.Mono
{
    /*
     * This class handles all cooking aspects of the Alien chief weither curing cooking and crafting
     *
     */
    internal class Cooker : MonoBehaviour
    {
        private Queue<CookingQueue> _cookingQueue;
        private StoveController _mono;
        private bool _isInitialized;
        private const int MaxQueue = 16;
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private float _cookingTime;
        public bool PauseUpdates { get; set; }
        public bool IsFull => _mono.StorageSystem.GetFreeSpace() == 0;
        public bool NotAllowToCook => PauseUpdates || _mono == null ||!_mono.IsConstructed || _cookingQueue.Count == 0;
        public bool IsCooking => GenerationProgress > -1;
        internal float GenerationProgress
        {
            get => _progress[(int)CookingPhases.Cooking];
            set => _progress[(int)CookingPhases.Cooking] = value;
        }
        
        private void Update()
        {
            Cook();

            if (IsCooking)
            {
                _mono.StorageSystem.IsAllowedToOpen = false;
                _mono.StorageSystem.hoverText = "Cannot open cooking. Please wait...";
            }
            else
            {
                _mono.StorageSystem.IsAllowedToOpen = true;
                _mono.StorageSystem.hoverText = "Open Stove";
            }
        }

        private void Cook()
        {
            if (NotAllowToCook)
            {
                return;
            }

            var energyToConsume = DayNightCycle.main.deltaTime;
            

            if (!_mono.HasPowerToConsume())
                return;

            if (GenerationProgress >= _cookingTime)
            {
                QuickLogger.Debug($"[Stove Cooked] {_cookingQueue.Peek()}", true);
                PauseUpdates = true;
                GenerationProgress = -1f;
                TryStartingNextFoodItem();
                PauseUpdates = false;
                AddItemToStorage();
            }
            else if (GenerationProgress >= 0f)
            {
                // Is currently generating clone
                GenerationProgress = Mathf.Min(_cookingTime, GenerationProgress + energyToConsume);

            }
            
            if(_cookingQueue?.Count == 0)
            {
                GenerationProgress = -1;
            }
        }

        private void TryStartingNextFoodItem()
        {
            QuickLogger.Debug("Trying to cook another item", true);

            if (IsFull)
            {
                QuickLogger.ModMessage(AuxPatchers.CookerInventoryFull());
                return;
            }

            if (GenerationProgress != -1f)
            {
                return;
            }

            if (_cookingQueue.Count == 0)
            {
                QuickLogger.ModMessage(AuxPatchers.NothingToCook());
            }

            QuickLogger.Debug("[Alien Chief] Cooking", true);
            _cookingTime = GetCookingTime();
            GenerationProgress = 0f;
        }

        internal void StartCooking()
        {
            for (int i = _mono.StorageSystem.container.count - 1; i >= 0; i--)
            {
                var item = _mono.StorageSystem.container.ElementAt(i);

                var itemData = StoveController.GetCookingItemData(item.item.GetTechType());

                if (itemData.ReturnItem != TechType.None)
                {
                    AddToQueue(itemData);
                    _mono.StorageSystem.container.RemoveItem(item.item);
                    Destroy(item.item.gameObject);
                }
            }

            StartCookingOperation();
        }

        private void StartCookingOperation()
        {
            if (_cookingQueue.Any())
            {
                TryStartingNextFoodItem();
                QuickLogger.ModMessage($"Stove {_mono.UnitID} is cooking.");
            }
        }

        private void AddItemToStorage()
        {
            var item  = _cookingQueue.Dequeue();

            if (_mono.IsSendingToSeaBreeze)
            {
                _mono.SendToSeaBreeze(item.CookedTechType.ToInventoryItem());
            }
            else
            {
                _mono.StorageSystem.AddItemToContainer(item.CookedTechType.ToInventoryItem());
            }
        }

        private float GetCookingTime()
        {
            if (CraftData.GetCraftTime(_cookingQueue.Peek().CookedTechType, out var duration))
            {
                duration = Mathf.Max(2.7f, duration);
            }
            else
            {
                duration = 2.7f;
            }
            QuickLogger.Debug($"Cooking Time Set to: {TimeConverters.SecondsToMS(duration)}");
            return duration;
        }

        internal void Initialize(StoveController mono)
        {
            try
            {
                _mono = mono;

                if(_cookingQueue == null)
                {
                    _cookingQueue = new Queue<CookingQueue>();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error<Cooker>(e.Message);
                QuickLogger.Error<Cooker>(e.Source);
                QuickLogger.Error<Cooker>(e.StackTrace);
                _isInitialized = false;
                return;
            }

            _isInitialized = true;
        }

        internal void AddToQueue( CookingItem cookingItem)
        {
            if(!_isInitialized || _cookingQueue.Count >= MaxQueue) return;
            QuickLogger.Debug($"Adding {cookingItem.TechType} to queue",true);
            _cookingQueue.Enqueue(new CookingQueue(cookingItem));
        }

        internal struct CookingQueue
        {
            public TechType CookedTechType { get; set; }
            public TechType RawTechType { get; set; }
            public float CookingTime { get; set; }

            public CookingQueue(CookingItem cookingItem)
            {
                RawTechType = cookingItem.TechType;
                CookedTechType = cookingItem.ReturnItem;
                CraftData.craftingTimes.TryGetValue(cookingItem.ReturnItem, out float value);
                CookingTime = value;
            }
        }

        internal enum CookingPhases
        {
            StartUp = 0,
            Cooking = 1,
            CoolDown = 2
        }

        public Tuple<Queue<CookingQueue>,float> Save()
        {
            return new Tuple<Queue<CookingQueue>, float>(_cookingQueue, GenerationProgress);
        }

        public void Load(Tuple<Queue<CookingQueue>, float> saveDataQueuedItems)
        {
            if (saveDataQueuedItems != null)
            {
                _cookingQueue = saveDataQueuedItems.Item1;
                GenerationProgress = saveDataQueuedItems.Item2;
            }

            StartCookingOperation();
        }
    }
}
