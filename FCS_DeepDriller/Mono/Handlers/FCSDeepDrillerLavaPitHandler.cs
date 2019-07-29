using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System.Collections;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerLavaPitHandler : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private int _lavaTempState;
        private int _lavaState;
        private bool _lavaRaised;
        private FCSPowerStates _prevState;
        private float _timeCoolingStart;
        private bool _wasActive;
        private FCSPowerStates _powerState;
        private const float CoolingDelay = 30f;
        private const float HeatLavaInSec = 20f;
        private const bool CooledLava = false;
        private const bool HotLava = true;
        private const int LavaRaiseWaitInSec = 11;
        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _mono.PowerManager.OnPowerUpdate += OnPowerUpdate;
            _lavaTempState = Animator.StringToHash("LavaTempState");
            _lavaState = Animator.StringToHash("LavaState");
        }

        private void OnPowerUpdate(FCSPowerStates powerState)
        {
            _powerState = powerState;

            if (powerState == FCSPowerStates.Powered && _prevState != FCSPowerStates.Powered)
            {
                StartCoroutine(HeatLava());

                if (_lavaRaised) return;
                StartCoroutine(RaiseLava());
                _prevState = FCSPowerStates.Powered;
                _wasActive = false;
            }

            if (powerState == FCSPowerStates.Unpowered || powerState == FCSPowerStates.Tripped)
            {
                StartCoolingInvoke();
                _prevState = powerState;
            }
        }

        private void StartCoolingInvoke()
        {
            float time = Random.Range(0f, 5f);
            InvokeRepeating("IterateCooling", time, 5f);
        }

        private void IterateCooling()
        {
            if (_powerState == FCSPowerStates.Powered)
            {
                _wasActive = false;
                return;
            }

            if (!_wasActive)
            {
                _timeCoolingStart = DayNightCycle.main.timePassedAsFloat;
            }
            this._wasActive = true;

            if (DayNightCycle.main.timePassedAsFloat - _timeCoolingStart > CoolingDelay)
            {
                CancelInvoke();
                CoolLava();
            }
        }

        internal void CoolLava()
        {
            QuickLogger.Debug("Switching to cool lava");
            _mono.AnimationHandler.SetBoolHash(_lavaTempState, CooledLava);
        }

        internal IEnumerator HeatLava()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(HeatLavaInSec);
            QuickLogger.Debug("Switching to lava");
            _mono.AnimationHandler.SetBoolHash(_lavaTempState, HotLava);
        }

        private IEnumerator RaiseLava()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(LavaRaiseWaitInSec);
            QuickLogger.Debug("Raising lava");
            _mono.AnimationHandler.SetBoolHash(_lavaState, true);
            _lavaRaised = true;
        }
    }
}
