using FCS_AlterraHub.Helpers;
using UnityEngine;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Model
{
    public class PlayerLockInteractionController : MonoBehaviour 
    {
        private bool _isInUse;
        private GameObject _inputDummy;
        private GameObject _playerBody;
        private GameObject _cameraPosition;
        private GameObject _screenBlock;
        private bool _cursorLockCached;

        private GameObject inputDummy
        {
            get
            {
                if (this._inputDummy == null)
                {
                    this._inputDummy = new GameObject("InputDummy");
                    this._inputDummy.SetActive(false);
                }
                return this._inputDummy;
            }
        }

        public void Initialize()
        {
            _cameraPosition = GameObjectHelpers.FindGameObject(gameObject, "CameraPosition");
            _screenBlock = GameObjectHelpers.FindGameObject(gameObject, "MainBlocker");
            _playerBody = Player.main.playerController.gameObject.FindChild("body");
        }

        private void ExitDisplay()
        {
            _isInUse = false;
            SNCameraRoot.main.transform.localPosition = Vector3.zero;
            SNCameraRoot.main.transform.localRotation = Quaternion.identity;
            ExitLockedMode();
            _playerBody.SetActive(true);
        }

        private void ExitLockedMode()
        {
            InterceptInput(false);
        }

        private void InterceptInput(bool state)
        {
            if (inputDummy.activeSelf == state)
            {
                return;
            }
            if (state)
            {
                _screenBlock.SetActive(false);
                MainCameraControl.main.enabled = false;
                InputHandlerStack.main.Push(inputDummy);
                _cursorLockCached = UWE.Utils.lockCursor;
                UWE.Utils.lockCursor = false;
                return;
            }

            UWE.Utils.lockCursor = _cursorLockCached;
            InputHandlerStack.main.Pop(inputDummy);
            MainCameraControl.main.enabled = true;
            _screenBlock.SetActive(true);
        }
        
        public void OnHandClick()
        {
            if (WorldHelpers.CheckIfInRange(Player.main.gameObject,gameObject,1))
            {
                InterceptInput(true);
                _isInUse = true;
                var hudCameraPos = _cameraPosition.transform.position;
                var hudCameraRot = _cameraPosition.transform.rotation;
                Player.main.SetPosition(new Vector3(hudCameraPos.x, Player.main.transform.position.y, hudCameraPos.z), hudCameraRot);
                _playerBody.SetActive(false);
                SNCameraRoot.main.transform.position = hudCameraPos;
                SNCameraRoot.main.transform.rotation = hudCameraRot;
            }
        }
    }
}
