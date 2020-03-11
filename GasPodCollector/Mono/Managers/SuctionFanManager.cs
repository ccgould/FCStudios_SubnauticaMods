using UnityEngine;

namespace GasPodCollector.Mono.Managers
{
    internal class SuctionFanManager : MonoBehaviour
    {
        private GameObject _blades1;
        private GameObject _blades2;
        private GaspodCollectorController _mono;
        private bool _increasing;
        private float IncreaseRate { get; set; } = 5f;
        private float _currentSpeed;

        private void Update()
        {
            if (_mono != null)
            {
                if (_mono.PowerManager.HasPower())
                {
                    Spin();
                }
                else
                {
                    Stop();
                }

                _currentSpeed = Mathf.Clamp(_currentSpeed + DayNightCycle.main.deltaTime * IncreaseRate * (_increasing ? 1 : -1), 0, 250);
                _blades1.transform.Rotate(Vector3.up, _currentSpeed * DayNightCycle.main.deltaTime);
                _blades2.transform.Rotate(Vector3.up, _currentSpeed * DayNightCycle.main.deltaTime);
            }
        }

        internal void Initialize(GaspodCollectorController mono)
        {
            _mono = mono;

            _blades1 = mono.gameObject.FindChild("model").FindChild("gaspod_container").FindChild("Floater_Top")
                .FindChild("Tube_2").FindChild("TopBladesController_2");

            _blades2 = mono.gameObject.FindChild("model").FindChild("gaspod_container").FindChild("Floater_Bottom")
                .FindChild("Tube").FindChild("TopBladesController");


        }

        internal void Spin()
        {
            _increasing = true;
        }

        internal void Stop()
        {
            _increasing = false;
        }
    }
}
