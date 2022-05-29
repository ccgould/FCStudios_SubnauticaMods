using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class DoorSensor : MonoBehaviour
    {
        private GameObject _door;
        private bool _isTriggered;
        private float ClosePos { get; } = -2.431577f;
        private float OpenPos { get; } = -1.359f;
        private const float Speed = 3f;
        

        private void Update()
        {
            if (_door == null) return;

            if (_isTriggered)
            {
                if (_door.transform.localPosition.x < OpenPos)
                {
                    _door.transform.Translate(Vector3.right * Speed * DayNightCycle.main.deltaTime);
                }

                if (_door.transform.localPosition.x > OpenPos)
                {
                    _door.transform.localPosition = new Vector3(OpenPos, _door.transform.localPosition.y, _door.transform.localPosition.z);
                }
            }
            else
            {
                if (_door.transform.localPosition.x > ClosePos)
                {
                    _door.transform.Translate(-Vector3.right * Speed * DayNightCycle.main.deltaTime);
                }

                if (_door.transform.localPosition.x < ClosePos)
                {
                    _door.transform.localPosition = new Vector3(ClosePos, _door.transform.localPosition.y, _door.transform.localPosition.z);
                }
            }
        }

        internal void Initialize(GameObject door)
        {
            _door = door;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            _isTriggered = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            _isTriggered = false;
        }
    }
}