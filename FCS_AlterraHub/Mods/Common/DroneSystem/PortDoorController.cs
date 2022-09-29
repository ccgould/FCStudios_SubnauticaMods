using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem
{
    internal class PortDoorController : MonoBehaviour
    {
        private Vector3 closedPos;
        private Vector3 openPos;
        private bool doorOpen;
        private FMOD_CustomEmitter _openSound;

        private void Start()
        {
            closedPos = transform.position;
            openPos = transform.TransformPoint(new Vector3(OpenPosX, 0f, 0f));

            _openSound = FModHelpers.CreateCustomEmitter(gameObject, "keypad_door_open", "event:/env/keypad_door_open");

            if (StartDoorOpen || doorOpen)
            {
                doorOpen = true;
                transform.position = openPos;
            }
        }

        public float OpenPosX { get; set; }

        public bool StartDoorOpen { get; set; }

        private void Update()
        {
            Vector3 vector = transform.position;
            vector = Vector3.Lerp(vector, doorOpen ? openPos : closedPos, Time.deltaTime * 2f);
            transform.position = vector;
        }

        public void Open()
        {
            doorOpen = true;
            PlaySound();
        }

        private void PlaySound()
        {
            if (_openSound)
            {
                _openSound.Play();
            }
        }

        public void Close()
        {
            doorOpen = false;
            PlaySound();
        }
    }
}