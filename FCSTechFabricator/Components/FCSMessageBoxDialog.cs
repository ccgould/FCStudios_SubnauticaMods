using System;
using FCSCommon.Helpers;
using FCSTechFabricator.Enums;
using FMOD;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Components
{
    public class FCSMessageBoxDialog : MonoBehaviour
    {
        public Action<string> OnCancelButtonClick { get; set; }
        public Action<string> OnConfirmButtonClick { get; set; }
        
        private string _messageId;
        private Text _messageText;
        private bool _initialized;
        private Action<FCSDialogResult> _callback;
        private GameObject _cancelBTNObject;
        private GameObject _noBTNObject;
        private GameObject _yesBTNObject;
        private GameObject _okBTNObject;

        private void Initialize()
        {
            if(_initialized) return;
            _messageText = GameObjectHelpers.FindGameObject(gameObject, "Message")?.GetComponent<Text>();
            
            _cancelBTNObject = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN");
            var cancelBTN = _cancelBTNObject?.GetComponent<Button>();
            _cancelBTNObject?.GetComponentInChildren<Text>(true);
            cancelBTN?.onClick.AddListener(() =>
            {
                OnCancelButtonClick?.Invoke(_messageId);
                _callback?.Invoke(FCSDialogResult.Cancel);
                HideMessageBox();
            });

            _noBTNObject = GameObjectHelpers.FindGameObject(gameObject, "NoBTN");
            var noBTN = _noBTNObject?.GetComponent<Button>();
            _noBTNObject?.GetComponentInChildren<Text>(true);
            noBTN?.onClick.AddListener(() =>
            {
                _callback?.Invoke(FCSDialogResult.No);
                HideMessageBox();
            });

            _yesBTNObject = GameObjectHelpers.FindGameObject(gameObject, "YesBTN");
            var yesBTN = _yesBTNObject?.GetComponent<Button>();
            _yesBTNObject?.GetComponentInChildren<Text>(true);
            yesBTN?.onClick.AddListener(() =>
            {
                _callback?.Invoke(FCSDialogResult.Yes);
                HideMessageBox();
            });

            _okBTNObject = GameObjectHelpers.FindGameObject(gameObject, "OkBTN");
            var okBTN = _okBTNObject?.GetComponent<Button>();
            _okBTNObject?.GetComponentInChildren<Text>(true);
            okBTN?.onClick.AddListener(() =>
            {
                _callback?.Invoke(FCSDialogResult.OK);
                HideMessageBox();
            });
            
            _initialized = true;
        }

        private void ChangeMessage(string message)
        {
            if(_messageText != null)
                _messageText.text = message;
        }


        private void HideMessageBox()
        {
            gameObject.SetActive(false);
        }

        private void HideCancelBTN()
        {
            _cancelBTNObject?.SetActive(false);
        }
        
        public virtual void ShowMessageBox(string message, string id, Action<FCSDialogResult> callback, FCSMessageBox buttons = FCSMessageBox.YesNo)
        {
            Initialize();
            ChangeMessage(message);
            _messageId = id;
            _callback = callback;
            RefreshButtons(buttons);
            gameObject.SetActive(true);
        }

        private void RefreshButtons(FCSMessageBox buttons)
        {
            switch (buttons)
            {
                case FCSMessageBox.OK:
                    _cancelBTNObject.SetActive(false);
                    _noBTNObject.SetActive(false);
                    _okBTNObject.SetActive(true);
                    _yesBTNObject.SetActive(false);
                    HideCancelBTN();
                    break;
                case FCSMessageBox.OKCancel:
                    _cancelBTNObject.SetActive(true);
                    _noBTNObject.SetActive(false);
                    _okBTNObject.SetActive(true);
                    _yesBTNObject.SetActive(false);
                    break;
                case FCSMessageBox.YesNo:
                    _cancelBTNObject.SetActive(false);
                    _noBTNObject.SetActive(true);
                    _okBTNObject.SetActive(false);
                    _yesBTNObject.SetActive(true);
                    break;
            }
        }
    }
}
