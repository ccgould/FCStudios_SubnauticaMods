using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    [RequireComponent(typeof(Button))]
    public class FCSButton : OnScreenButton,IPointerEnterHandler,IPointerExitHandler
    {
        private Button _button;
        
        private void Start()
        {
            FindButton();
        }

        private void FindButton()
        {
            if (_button == null)
            {
                _button = gameObject.GetComponent<Button>();
            }

            if (_button == null)
            {
                QuickLogger.DebugError($"Failed to find Button on object {gameObject.name} for FCSButton.",true);
            }
        }

        public override void Update()
        {
            base.Update();
            if (_button == null) return;
            _button.interactable = IsHovered;
        }

        public void Subscribe(UnityAction call)
        {
            FindButton();
            _button?.onClick.AddListener(call);
        }

        public void UnSubscribe(UnityAction call)
        {
            _button?.onClick.RemoveListener(call);
        }
    }
}
