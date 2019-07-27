using FCS_DeepDriller.Managers;
using FCSCommon.Utilities.Enums;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerLavaPitHandler : MonoBehaviour
    {
        private FCSDeepDrillerController _mono;
        private FCSPowerStates _powerState;
        private float _passedTime;
        private int _lavaTempState;
        private int _lavaState;
        private const float HeatLavaInSec = 5f;
        private const float CoolLavaInSec = 300f;
        private const bool CooledLava = false;
        private const bool HotLava = true;

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _powerState = mono.PowerManager.GetPowerState();
            _lavaTempState = Animator.StringToHash("LavaTempState");
            _lavaState = Animator.StringToHash("LavaState");
            //InvokeRepeating("LavaStateUpdater", 1, 1);
        }

        internal void CoolLava()
        {
            _mono.AnimationHandler.SetBoolHash(_lavaTempState, CooledLava);
        }

        internal void HeatLava()
        {
            _mono.AnimationHandler.SetBoolHash(_lavaTempState, HotLava);
        }

        internal void RaiseLava()
        {
            _mono.AnimationHandler.SetBoolHash(_lavaState, true);
        }

        private void Update()
        {
            if (DayNightCycle.main == null) return;

            if (_powerState == FCSPowerStates.Powered && !DeepDrillerComponentManager.IsLavaHot())
            {
                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= HeatLavaInSec)
                {
                    HeatLava();

                    if (_mono.AnimationHandler.GetBoolHash(_lavaState) != true)
                    {
                        RaiseLava();
                    }

                    _passedTime = 0.0f;
                }
            }
            else if (_powerState == FCSPowerStates.Tripped || _powerState == FCSPowerStates.Unpowered && DeepDrillerComponentManager.IsLavaHot())
            {
                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= CoolLavaInSec)
                {
                    CoolLava();
                    _passedTime = 0.0f;
                }
            }
        }

        internal void RestTime()
        {
            _passedTime = 0.0f;
        }
    }
}
