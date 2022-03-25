using UnityEngine;

namespace FCS_HomeSolutions.Mods.Shower.Mono
{
    internal class DoorController : HandTarget, IHandTarget
    {
        private Transform _transform;
        public float ClosePos { get; set; }
        public float OpenPos { get; set; }
        private float _targetPos;
        public bool IsOpen => Mathf.Approximately(_targetPos, OpenPos);
        private const float  Speed = 2.5f;

        private void Update()
        {
            //MoveDoor();
            RotateDoor();
        }

        public override void Awake()
        {
            base.Awake();
            _targetPos = ClosePos;
            _transform = gameObject.transform;
        }

        public void ForceOpen()
        {
            OpenDoor();
        }

        private void OpenDoor()
        {
            _targetPos = OpenPos;
        }

        private void CloseDoor()
        {
            _targetPos = ClosePos;
        }

        private void MoveDoor()
        {
            // remember, 10 - 5 is 5, so target - position is always your direction.
            Vector3 dir = new Vector3(_targetPos, _transform.localPosition.y, _transform.localPosition.z) - _transform.localPosition;

            // magnitude is the total length of a vector.
            // getting the magnitude of the direction gives us the amount left to move
            float dist = dir.magnitude;

            // this makes the length of dir 1 so that you can multiply by it.
            dir = dir.normalized;

            // the amount we can move this frame
            float move = Speed * DayNightCycle.main.deltaTime;

            // limit our move to what we can travel.
            if (move > dist) move = dist;

            // apply the movement to the object.
            _transform.Translate(dir * move);
        }

        private void RotateDoor()
        {
            _transform.localRotation = Quaternion.Slerp(_transform.localRotation,Quaternion.Euler(0,_targetPos,0),Speed * Time.deltaTime);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            if (IsOpen)
            {
#if SUBNAUTICA
                main.SetInteractText(
#else
                main.SetTextRaw(HandReticle.TextType.Use,
#endif
                "Close Door");
            }
            else
            {
                
#if SUBNAUTICA
                main.SetInteractText(
#else
                main.SetTextRaw(HandReticle.TextType.Use,
#endif
                "Open Door");
            }

            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (IsOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }
}