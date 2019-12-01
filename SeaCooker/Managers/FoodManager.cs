
using AE.SeaCooker.Configuration;
using AE.SeaCooker.Enumerators;
using AE.SeaCooker.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using AE.SeaCooker.Buildable;
using FCSCommon.Enums;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class FoodManager : MonoBehaviour
    {
        internal Action<TechType, List<TechType>> OnFoodCookedAll;
        internal Action<TechType, TechType> OnFoodCooked;
        internal Action<TechType, TechType> OnCookingStart;
        private float _passedTime;
        private TechType _rawTechType;
        private SeaCookerController _mono;
        private float _targetTime;
        private bool _isCooking;
        private Coroutine _cookall;
        private bool _fromSave;
        private bool _continue;

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
            StartCoroutine(Cook());
            QuickLogger.Debug($"Cooking food {_rawTechType}", true);
        }

        internal void KillCooking()
        {
            QuickLogger.Debug("Kill Cooking");
            StopCoroutine(_cookall);
            StopCoroutine(nameof(Cook));
            _mono.UpdateIsRunning(false);
            _mono.DisplayManager.ToggleProcessDisplay(false);
            _mono.DisplayManager.ResetProgressBar();
            Reset();
        }

        private IEnumerator Cook()
        {
            OnCookingStart?.Invoke(_rawTechType, GetCookedFood(_rawTechType));
            _isCooking = true;

            for (float timer = QPatch.Configuration.Config.CookTime; timer >= 0; timer -= DayNightCycle.main.deltaTime)
            {

                yield return null;
            }

            OnFoodCooked?.Invoke(_rawTechType, GetCookedFood(_rawTechType));
            _mono.GasManager.RemoveGas(QPatch.Configuration.Config.UsagePerItem);
            QuickLogger.Debug($"Cooked food {_rawTechType}", true);
        }

        private IEnumerator CookAll(ItemsContainer container)
        {
            var list = new List<TechType>();
            _isCooking = true;
            _targetTime = QPatch.Configuration.Config.CookTime * container.count;

            if (!_fromSave)
            {
                _mono.GasManager.RemoveGas(QPatch.Configuration.Config.UsagePerItem);
            }

            OnCookingStart?.Invoke(_rawTechType, GetCookedFood(_rawTechType));

            //for (float timer = _targetTime; timer >= 0; timer -= DayNightCycle.main.deltaTime)
            //{
            //    yield return null;
            //}

            while (!_continue)
            {
                yield return null;
            }

            QuickLogger.Debug("Transferring Cooked Items", true);

            foreach (InventoryItem item in container)
            {
                _rawTechType = item.item.GetTechType();
                list.Add(GetCookedFood(_rawTechType));
                QuickLogger.Debug($"Cooked food {_rawTechType}", true);
            }

            Reset();

            OnFoodCookedAll?.Invoke(_rawTechType, list);
        }

        private void Reset()
        {
            _isCooking = false;
            _passedTime = 0;
            _fromSave = false;
            _continue = false;
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
            if (!_isCooking) return;

            _passedTime += DayNightCycle.main.deltaTime;

            var percent = _passedTime / _targetTime;

            _mono.DisplayManager.UpdatePercentage(percent);

            if (!(_passedTime >= _targetTime)) return;

            _continue = true;
        }

        internal bool IsCooking()
        {
            return _isCooking;
        }

        internal void CookAllFood(ItemsContainer container)
        {
            QuickLogger.Debug($"Get Powered State {_mono.PowerManager.GetPowerState()} || Current Fuel {_mono.GasManager.CurrentFuel} || Has Room for All {_mono.StorageManager.HasRoomForAll()}");

            if(_mono.PowerManager.GetPowerState() == FCSPowerStates.Unpowered) { QuickLogger.Message(SeaCookerBuildable.NoPowerAvailable(),true);}

            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Unpowered
                || _mono.GasManager.CurrentFuel == FuelType.None
                || !_mono.StorageManager.HasRoomForAll())
            {
                KillCooking();
                return;
            }

            _cookall = StartCoroutine(CookAll(container));
            QuickLogger.Debug("Cooking All Food", true);
        }

        internal void LoadRunningState(SaveDataEntry data)
        {
            if (!data.IsCooking) return;

            _passedTime = data.PassedTime;
            _fromSave = true;
            CookAllFood(_mono.StorageManager.GetContainer());

            //_targetTime = data.TargetTime;
            //_isCooking = data.IsCooking;
        }

        internal void SaveRunningState(SaveDataEntry data)
        {
            data.PassedTime = _passedTime;
            data.TargetTime = _targetTime;
            data.IsCooking = _isCooking;
        }
    }
}
