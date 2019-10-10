using AE.HabitatSystemsPanel.Mono;
using System.Reflection;

namespace AE.HabitatSystemsPanel.Managers
{
    internal class LightManager
    {
        private HSPController _mono;
        private SubRoot _currentSub;
        private FieldInfo _subLights;

        internal void Initialized(HSPController mono)
        {
            _mono = mono;
            _currentSub = Player.main.GetCurrentSub();

            if (_currentSub != null)
            {
                _subLights = typeof(SubRoot).GetField("subLightsOn", BindingFlags.Instance | BindingFlags.NonPublic);
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
    }
}
