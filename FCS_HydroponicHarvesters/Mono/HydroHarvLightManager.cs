using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvLightManager : MonoBehaviour
    {
        private Light _light;
        private HydroHarvController _mono;
        private bool _lightSwitchedOff;

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
            _light = gameObject.GetComponentInChildren<Light>();
            _light.gameObject.SetActive(false);
            InvokeRepeating(nameof(UpdateLightState),0.5f,0.5f);
        }

        private void TurnOn()
        {
            if (_light != null && !_light.isActiveAndEnabled)
            {
                _light.gameObject.SetActive(true);
            }
        }

        private void TurnOff()
        {
            if (_light != null && _light.isActiveAndEnabled)
            {
                _light.gameObject.SetActive(false);
            }
        }

        private void UpdateLightState()
        {
            if (_mono.PowerManager.HasPowerToConsume())
            {
                if (_lightSwitchedOff)
                {
                    TurnOff();
                }
                else
                {
                    TurnOn();
                }
            }
            else
            {
                TurnOff();
            }
        }

        internal bool GetLightSwitchedOff()
        {
            return _lightSwitchedOff;
        }

        internal void SetLightSwitchedOff(bool state)
        {
            _lightSwitchedOff = state;
        }

        internal void Load(bool state)
        {
            _lightSwitchedOff = state;
            _light.gameObject.SetActive(!state);
        }
        
        internal void ToggleLight()
        {
            _lightSwitchedOff = !_lightSwitchedOff;
        }
    }
}
