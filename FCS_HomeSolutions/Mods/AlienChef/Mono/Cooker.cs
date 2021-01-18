using System;
using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Converters;
using FCSCommon.Extensions;
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
            if (_mode == CookerMode.Cook)
            {
                Cook();
                return;
            }

            if (_mode == CookerMode.Cure)
            {
                Cure();
                return;
            }

            if (_mode == CookerMode.Custom)
            {
                return;
            }
        }

        private void Cure()
        {
            if (NotAllowToCook)
                return;

            var energyToConsume = DayNightCycle.main.deltaTime;


            if (!_mono.HasPowerToConsume())
                return;

            if (GenerationProgress >= _cookingTime)
            {
                QuickLogger.Debug($"[Alien Chief Cured] {_cookingQueue.Peek()}", true);
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
                _mono.DisplayManager.UpdatePercentage(GenerationProgress / _cookingTime);
                _mono.DisplayManager.UpdateCookingTime(_cookingTime - GenerationProgress);

            }
        }

        private void Cook()
        {
            if (NotAllowToCook)
                return;

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
        }

        private void TryStartingNextFoodItem()
        {
            QuickLogger.Debug("Trying to cook another item", true);

            //if (CurrentSpeedMode == SpeedModes.Off)
            //    return;// Powered off, can't start a new clone

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
        }


        private void AddItemToStorage()
        {
            var item  = _cookingQueue.Dequeue();
            //TODO Add to container for testing give to player
            if (_mode == CookerMode.Cook)
            {
                _mono.StorageSystem.AddItem(item.CookedTechType.ToInventoryItem());
                //PlayerInteractionHelper.GivePlayerItem(item.CookedTechType);
            }

            if (_mode == CookerMode.Cure)
            {
                _mono.StorageSystem.AddItem(item.CuredTechType.ToInventoryItem());
                //PlayerInteractionHelper.GivePlayerItem(item.CuredTechType);
            }

            if (_mode == CookerMode.Custom)
            {

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

        internal void AddToQueue(TechType rawTechType, TechType cookedTechType, TechType curedTechType)
        {
            if(!_isInitialized || _cookingQueue.Count >= MaxQueue) return;
            QuickLogger.Debug($"Adding {rawTechType} to queue",true);
            _cookingQueue.Enqueue(new CookingQueue(rawTechType, cookedTechType, curedTechType));
        }

        internal struct CookingQueue
        {
            public TechType RawTechType { get; set; }
            public TechType CookedTechType { get; set; }
            public float CookingTime { get; set; }
            public TechType CuredTechType { get; set; }

            public CookingQueue(TechType rawTechType, TechType cookedTechType, TechType curedTechType)
            {
                RawTechType = rawTechType;
                CookedTechType = cookedTechType;
                CuredTechType = curedTechType;
                CraftData.craftingTimes.TryGetValue(cookedTechType, out float value);
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
