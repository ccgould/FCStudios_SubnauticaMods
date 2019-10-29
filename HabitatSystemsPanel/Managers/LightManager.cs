using AE.HabitatSystemsPanel.Mono;
using FCSCommon.Utilities;
using System.Reflection;
using UnityEngine;

namespace AE.HabitatSystemsPanel.Managers
{
    internal class LightManager
    {
        private HSPController _mono;
        private SubRoot _currentSub;
        private FieldInfo _subLights;
        private static readonly MethodInfo SetLightsActiveMethod = typeof(TechLight).GetMethod("SetLightsActive", BindingFlags.Instance | BindingFlags.NonPublic);
        internal void Initialized(HSPController mono)
        {
            _mono = mono;
            _currentSub = Player.main.GetCurrentSub();

            if (_currentSub != null)
            {
                _subLights = typeof(SubRoot).GetField("subLightsOn", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            this.spotLight = base.GetComponent<BaseSpotLight>();
        }

        internal void ToggleFloodLight(TechLight light)
        {
            if (light != null)
            {
                bool constructed = light.constructable.constructed;
                if (constructed)
                {
                    this.isOn = !this.isOn;
                    FloodlightToggle.SetLightsActiveMethod.Invoke(this.techLight, new object[]
                    {
                        this.isOn
                    });
                }
                else
                {
                    QuickLogger.Debug("Flood Light not constructed.");
                }
            }
            else
            {
                QuickLogger.Debug("Flood Light returned null");
            }
        }





        internal void ToggleHabitatLights()
        {
            if (_currentSub != null)
            {
                if (_mono.IsConstructed)
                {
                    bool result = (bool)_subLights.GetValue(_currentSub);
                    _currentSub.ForceLightingState(!result);
                }
            }
        }

        internal void ToggleFloodLights()
        {
            if (_currentSub != null)
            {
                foreach (Transform go in _currentSub.gameObject.transform)
                {

                }
            }
        }
    }
}
