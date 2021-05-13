using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class ScreenTrigger : HandTarget,IHandTarget
    {
        private bool _cursorLockCached;
        private GameObject _inputDummy;
        private bool _isLocked;
        private BoxCollider _boxCollider;

        private void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                InterceptInput(false);
            }
        }

        private GameObject inputDummy
        {
            get
            {
                if (_inputDummy == null)
                {
                    _inputDummy = new GameObject("InputDummy");
                    _inputDummy.SetActive(false);
                }
                return _inputDummy;
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText("Use Turbine Control");
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        }

        public void OnHandClick(GUIHand hand)
        {
            if(_isLocked) return;
            InterceptInput(true);
        }

        internal void InterceptInput(bool state)
        {
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.GetComponentInChildren<BoxCollider>();
            }

            if (inputDummy.activeSelf == state)
            {
                return;
            }

            if (state)
            {
                Player.main.EnterLockedMode(null);
                MainCameraControl.main.enabled = false;
                InputHandlerStack.main.Push(inputDummy);
                _cursorLockCached = UWE.Utils.lockCursor;
                UWE.Utils.lockCursor = false;
                _boxCollider.isTrigger = true;
                _isLocked = true;
                return;
            }

            Player.main.ExitLockedMode(false, false);
            UWE.Utils.lockCursor = _cursorLockCached;
            InputHandlerStack.main.Pop(inputDummy);
            MainCameraControl.main.enabled = true;
            _boxCollider.isTrigger = false;
            _isLocked = false;
        }


    }
}