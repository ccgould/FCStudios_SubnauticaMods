using System;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    [RequireComponent(typeof(Button))]
    public class FCSMultiClickButton : OnScreenButton,IPointerEnterHandler,IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Button _button;
        bool mouseClicksStarted = false; 
        bool wasLongPressed = false; 
        int mouseClicks = 0; 
        float mouseTimerLimit = .25f;
        private float holdDuration = 0.5f;
        public Action onLongPress;
        public Action onDoubleClick;
        public Action onSingleClick;

        private bool isPointerDown = false;
        private bool isLongPressed = false;
        private float elapsedTime = 0f;


        private void Start()
        {
            FindButton();
        }

        private void FindButton()
        {
            if (_button == null)
            {
                _button = gameObject.GetComponentInChildren<Button>();
                if (_button != null)
                {
                    _button.onClick.AddListener((() =>
                    {
                        mouseClicks++;
                        if (mouseClicksStarted)
                        {
                            return;
                        }
                        mouseClicksStarted = true;
                        Invoke(nameof(checkMouseDoubleClick), mouseTimerLimit);
                    }));
                }
            }

        }

        private void checkMouseDoubleClick()
        {
            if (mouseClicks > 1)
            {
                QuickLogger.Debug("Double Clicked");
                onDoubleClick?.Invoke();
            }
            else if(!wasLongPressed)
            {
                QuickLogger.Debug("Single Clicked");
                onSingleClick.Invoke();
            }

            if (wasLongPressed)
            {
                wasLongPressed = false;
            }
            mouseClicksStarted = false;
            mouseClicks = 0;
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
            isLongPressed = false;
            elapsedTime = 0f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
        }

        public override void Update()
        {
            base.Update();
            if (isPointerDown && !isLongPressed)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= holdDuration)
                {
                    QuickLogger.Debug("Long Clicked");
                    isLongPressed = true;
                    wasLongPressed = true;
                    elapsedTime = 0f;
                    if (_button.interactable && !object.ReferenceEquals(onLongPress, null))
                    {
                        onLongPress.Invoke();
                    }
                }
            }
        }
    }
}
