using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class ScreenController : MonoBehaviour, IPointerHoverHandler, IPointerClickHandler
    {
        private FCSElevatorController _controller;
        private GameObject _waitingBackground;
        private GameObject _movingToBackground;
        private GameObject _currentlyAtBackground;
        private GameObject _information;
        private Text _statusMessage;
        private Text _floorNumber;
        private Text _meterAmount;
        private Text _metersText;
        private Text _floorName;
        private Text _speedText;
        private Text _speedAmount;

        internal void Initialize(FCSElevatorController controller)
        {
            _controller = controller;
            _waitingBackground = GameObjectHelpers.FindGameObject(gameObject, "WaitingBackground");
            _movingToBackground = GameObjectHelpers.FindGameObject(gameObject, "MovingToBackground");
            _currentlyAtBackground = GameObjectHelpers.FindGameObject(gameObject, "CurrentlyAtBackground");
            _information = GameObjectHelpers.FindGameObject(gameObject, "Information");
            _statusMessage = GameObjectHelpers.FindGameObject(gameObject, "StatusMessage").GetComponent<Text>();
            _floorNumber = GameObjectHelpers.FindGameObject(gameObject, "FloorNumber").GetComponent<Text>();
            _meterAmount = GameObjectHelpers.FindGameObject(gameObject, "meterAmount").GetComponent<Text>();
            _metersText = GameObjectHelpers.FindGameObject(gameObject, "metersText").GetComponent<Text>();            
            _speedAmount = GameObjectHelpers.FindGameObject(gameObject, "speedAmount").GetComponent<Text>();
            _speedText = GameObjectHelpers.FindGameObject(gameObject, "speedText").GetComponent<Text>();
            _floorName = GameObjectHelpers.FindGameObject(gameObject, "FloorName").GetComponent<Text>();
            InvokeRepeating(nameof(UpdateScreen),.2f,.2f);
        }
        
        private void UpdateScreen()
        {
            if (_controller.PlatformTrigger == null) return;

            if (!_controller.Manager.HasEnoughPower(FCSElevatorController.POWERUSAGE))
            {
                _information.SetActive(false);
                _waitingBackground.SetActive(true);
                _movingToBackground.SetActive(false);
                _currentlyAtBackground.SetActive(false);
                _floorName.text = "NoPower";
            }

            if (!_controller.PlatformTrigger.IsPlayerInside)
            {
                _information.SetActive(false);
                _waitingBackground.SetActive(true);
                _movingToBackground.SetActive(false);
                _currentlyAtBackground.SetActive(false);
                _floorName.text = "WAITING";
            }
            else if (_controller.IsMoving())
            {
                _information.SetActive(true);
                _waitingBackground.SetActive(false);
                _movingToBackground.SetActive(true);
                _currentlyAtBackground.SetActive(false);
                _statusMessage.text = "MOVING TO";
            }
            else
            {
                _information.SetActive(true);
                _waitingBackground.SetActive(false);
                _movingToBackground.SetActive(false);
                _currentlyAtBackground.SetActive(true);
                _statusMessage.text = "CURRENTLY AT";
            }

            _floorName.text = _controller.GetCurrentFloorData().FloorName;
            _floorNumber.text = _controller.GetCurrentFloorIndex().ToString();
            _meterAmount.text = _controller.GetPlatformCurrentHeightRounded().ToString();
            _speedAmount.text = _controller.GetSpeedValue().ToString("0.00");

        }

        public void OnPointerHover(PointerEventData eventData)
        {
            if(!WorldHelpers.CheckIfInRange(gameObject,Player.main.gameObject,1)) return;
            var main = HandReticle.main;
            main.SetInteractText("Open Control Panel");
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 1)) return;
            ElevatorHUD.Main.Show(_controller);
        }
    }
}