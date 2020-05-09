using FCSCommon.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Display
{
    internal class DSSUIButton : InterfaceButton
    {
        float _timeLeft = 2.0f;
        public Vehicle Vehicle { get; set; }
        public Text Label { get; set; }

        private new void Update()
        {
            if (Vehicle == null || Label == null) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                UpdateLabel();
                _timeLeft = 2.0f;
            }
        }

        private void UpdateLabel()
        {
            Label.text = Vehicle.GetName();
        }
    }
}
