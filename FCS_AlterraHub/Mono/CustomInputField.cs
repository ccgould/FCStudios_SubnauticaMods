﻿using System;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    public class CustomInputField : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;
        private InputField _inputField;
        internal Action<string> OnSearchValueChanged;
        internal Action<string> OnEnterPressed;
        private bool _cursorLockCached;
        private GameObject _inputDummy;
        internal string HoverMessage;


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


        private void Update()
        {
            if (!hover) return;
            HandReticle.main.SetIcon(HandReticle.IconType.Rename);
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, HoverMessage);
        }

        private void Awake()
        {
            _inputField = GetComponentInChildren<InputField>();
            _inputField.onValueChanged.AddListener(OnValueChanged);
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string text)
        {
            QuickLogger.Debug("End Edit",true);
            LockMovement(false);
            InterceptInput(false);
            //OnEnterPressed?.Invoke(text);
        }
        private void LockMovement(bool state)
        {
            FPSInputModule.current.lockMovement = state;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            QuickLogger.Debug("Searching",true);
            InterceptInput(true);
            LockMovement(true);
            //Player.main.playerController.SetEnabled(false);
        }

        private void InterceptInput(bool state)
        {
            if (inputDummy.activeSelf == state)
            {
                return;
            }
            if (state)
            {
                InputHandlerStack.main.Push(inputDummy);
                _cursorLockCached = UWE.Utils.lockCursor;
                UWE.Utils.lockCursor = false;
                return;
            }
            UWE.Utils.lockCursor = _cursorLockCached;
            InputHandlerStack.main.Pop(inputDummy);
        }

        private void OnValueChanged(string newSearch)
        {
            OnSearchValueChanged?.Invoke(newSearch);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }
    }
}
