
using AE.SeaCooker.Enumerators;
using AE.SeaCooker.Mono;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System;
using System.Collections;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class FoodManager : MonoBehaviour
    {
        internal Action<TechType, TechType> OnFoodCooked;
        internal Action<TechType, TechType> OnCookingStart;
        private bool _startCookingProcess;
        private float _passedTime;
        private TechType _rawTechType;
        private SeaCookerController _mono;
        private bool _killCooking;

        internal void Initialize(SeaCookerController mono)
        {
            _mono = mono;
        }

        private void Update()
        {
            Timer();
        }

        internal void CookFood(InventoryItem item)
        {
            if (_mono.PowerManager.GetPowerState() != FCSPowerStates.Powered
                || _mono.GasManager.CurrentFuel == FuelType.None
                || !_mono.StorageManager.HasRoomFor(item.item))
            {
                KillCooking();
                return;
            }

            _rawTechType = item.item.GetTechType();
            _startCookingProcess = true;
            StartCoroutine(Cook());
            QuickLogger.Debug($"Cooking food {_rawTechType}", true);
        }

        internal void KillCooking()
        {
            QuickLogger.Debug("Kill Cooking");
            _killCooking = true;
            StopCoroutine(Cook());
            _mono.UpdateIsRunning(false);
            _mono.DisplayManager.ToggleProcessDisplay(false);
            Reset();
            _mono.DisplayManager.ResetProgressBar();
        }

        private IEnumerator Cook()
        {
            OnCookingStart?.Invoke(_rawTechType, GetCookedFood(_rawTechType));
            yield return new WaitForSeconds(QPatch.Configuration.Config.CookTime);
            QuickLogger.Debug($"StartProcess : {_startCookingProcess}", true);

            if (_killCooking)
            {
                _killCooking = false;
                yield break;
            }

            OnFoodCooked?.Invoke(_rawTechType, GetCookedFood(_rawTechType));
            _mono.GasManager.RemoveGas(QPatch.Configuration.Config.UsagePerItem);
            QuickLogger.Debug($"Cooked food {_rawTechType}", true);
        }

        private void Reset()
        {
            _startCookingProcess = false;
            _passedTime = 0;
        }

        private TechType GetCookedFood(TechType techType)
        {
            TechType cookedFood = TechType.None;

            switch (techType)
            {
                case TechType.Peeper:
                    cookedFood = TechType.CookedPeeper;
                    break;
                case TechType.HoleFish:
                    cookedFood = TechType.CookedHoleFish;
                    break;
                case TechType.GarryFish:
                    cookedFood = TechType.CookedGarryFish;
                    break;
                case TechType.Reginald:
                    cookedFood = TechType.CookedReginald;
                    break;
                case TechType.Bladderfish:
                    cookedFood = TechType.CookedBladderfish;
                    break;
                case TechType.Hoverfish:
                    cookedFood = TechType.CookedHoverfish;
                    break;
                case TechType.Spadefish:
                    cookedFood = TechType.CookedSpadefish;
                    break;
                case TechType.Boomerang:
                    cookedFood = TechType.CookedBoomerang;
                    break;
                case TechType.Eyeye:
                    cookedFood = TechType.CookedEyeye;
                    break;
                case TechType.Oculus:
                    cookedFood = TechType.CookedOculus;
                    break;
                case TechType.Hoopfish:
                    cookedFood = TechType.CookedHoopfish;
                    break;
                case TechType.Spinefish:
                    cookedFood = TechType.CookedSpinefish;
                    break;
                case TechType.LavaEyeye:
                    cookedFood = TechType.CookedLavaEyeye;
                    break;
                case TechType.LavaBoomerang:
                    cookedFood = TechType.CookedLavaBoomerang;
                    break;
            }

            return cookedFood;
        }

        private void Timer()
        {
            if (!_startCookingProcess) return;

            _passedTime += DayNightCycle.main.deltaTime;

            var percent = _passedTime / QPatch.Configuration.Config.CookTime;

            _mono.DisplayManager.UpdatePercentage(percent);

            if (!(_passedTime >= QPatch.Configuration.Config.CookTime)) return;

            Reset();
        }

        internal bool IsCooking()
        {
            return _startCookingProcess;
        }
    }
}
