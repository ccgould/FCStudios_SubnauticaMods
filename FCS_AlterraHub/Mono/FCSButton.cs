using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    //[RequireComponent(typeof(Button))]
    public class FCSButton : OnScreenButton,IPointerEnterHandler,IPointerExitHandler
    {
        private Button _button;
        private Toggle _toggle;
        
        private void Start()
        {
            FindButton();
        }

        private void FindButton()
        {
            if (_button == null)
            {
                _button = gameObject.GetComponentInChildren<Button>();
                if(_button != null) return;
            }

            if (_toggle == null)
            {
                _toggle = gameObject.GetComponentInChildren<Toggle>();
            }

            if (_button == null && _toggle == null)
            {
                QuickLogger.DebugError($"Failed to find Button/Toggle on object {gameObject.name} for FCSButton.",true);
            }
        }

        public override void Update()
        {
            base.Update();
            if (_button == null && _toggle == null) return;

            if (_button != null)
            {
                //_button.interactable = IsHovered;
            }

            if (_toggle != null)
            {
                //_toggle.interactable = IsHovered;
            }
        }

        public void Subscribe(UnityAction call)
        {
            FindButton();
            _button?.onClick.AddListener(call);
        }        
        
        public void Subscribe(UnityAction<bool> call)
        {
            FindButton();
            _toggle?.onValueChanged.AddListener(call);
        }

        public void UnSubscribe(UnityAction call)
        {
            _button?.onClick.RemoveListener(call);
        }

        public void UnSubscribe(UnityAction<bool> call)
        {
            _toggle?.onValueChanged.RemoveListener(call);
        }

        public void Check(bool notify=false)
        {
            if (notify)
            {
                _toggle.isOn = true;
            }
            else
            {
                _toggle?.SetIsOnWithoutNotify(true);
            }
        }

        public void UnCheck(bool notify = false)
        {
            if (notify)
            {
                _toggle.isOn = false;
            }
            else
            {
                _toggle?.SetIsOnWithoutNotify(false);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool IsSelected()
        {
            if (_toggle != null)
            {
                return _toggle.isOn;
            }

            return false;
        }
    }
}
