using System;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.Converters;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    /*
     * This class handles all cooking aspects of the Alien chief weither curing cooking and crafting
     *
     */
    internal class Cooker : MonoBehaviour
    {
        private Queue<CookingQueue> _cookingQueue;
        private AlienChefController _mono;
        private bool _isInitialized;
        private const int MaxQueue = 16;
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private float _cookingTime;
        private CookerMode _mode = CookerMode.Cook;
        public bool PauseUpdates { get; set; }
        public bool IsFull => _mono.StorageSystem.GetFreeSpace() == 0;
        public bool NotAllowToCook => PauseUpdates || _mono == null ||!_mono.IsConstructed || _cookingQueue.Count == 0;
        internal float GenerationProgress
        {
            get => _progress[(int)CookingPhases.Generating];
            set => _progress[(int)CookingPhases.Generating] = value;
        }


        private void Update()
        {
            Cook();
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
                QuickLogger.Debug($"[Alien Chief Cooked] {_cookingQueue.Peek()}", true);
                PauseUpdates = true;
                GenerationProgress = -1f;
                TryStartingNextFoodItem();
                PauseUpdates = false;
                AddItemToStorage();
                _mono.DisplayManager.UpdatePercentage(0);
                _mono.DisplayManager.UpdateCookingTime(0);
            }
            else if (GenerationProgress >= 0f)
            {
                // Is currently generating clone
                GenerationProgress = Mathf.Min(_cookingTime, GenerationProgress + energyToConsume);
                _mono.DisplayManager.UpdatePercentage(GenerationProgress/_cookingTime);
                _mono.DisplayManager.UpdateCookingTime(_cookingTime - GenerationProgress);

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
            TryStartingNextFoodItem();
            QuickLogger.ModMessage($"Alien Chef {_mono.UnitID} is cooking.");
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


        internal void Initialize(AlienChefController mono)
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
            Generating = 1,
            CoolDown = 2
        }

        public void ChangeMode(CookerMode mode)
        {
            _mode = mode;
        }

        public CookerMode GetMode()
        {
            return _mode;
        }
    }
}
