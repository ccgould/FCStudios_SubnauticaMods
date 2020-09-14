using System;
using FCSCommon.Helpers;
using FCSTechFabricator.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCSTechFabricator.Components
{
    public class FCSMessageBoxDialog : MonoBehaviour
    {
        public Action<string> OnCancelButtonClick { get; set; }
        public Action<string> OnConfirmButtonClick { get; set; }
        
        private string _messageId;
        private Button _cancelBTN;
        private Button _confirmBTN;
        private Text _messageText;
        private Text _confirmBTNText;
        private Text _cancelBTNText;
        private GameObject _confirmButtonObject;
        private GameObject _cancelBTNObject;
        private bool _initialized;

        private void Initialize()
        {
            if(_initialized) return;
            _messageText = GameObjectHelpers.FindGameObject(gameObject, "Message")?.GetComponent<Text>();
            
            _cancelBTNObject = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN");
            _cancelBTN = _cancelBTNObject?.GetComponent<Button>();
            _cancelBTNText = _cancelBTNObject?.GetComponentInChildren<Text>(true);
            _cancelBTN?.onClick.AddListener(() =>
            {
                OnCancelButtonClick?.Invoke(_messageId); 
                HideMessageBox();
            });

            _confirmButtonObject = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN");
            _confirmBTNText = _confirmButtonObject?.GetComponentInChildren<Text>(true);
            _confirmBTN = _confirmButtonObject?.GetComponent<Button>();
            _confirmBTN?.onClick.AddListener(() =>
            {
                OnConfirmButtonClick?.Invoke(_messageId); 
                HideMessageBox();
            });

            _initialized = true;
        }

        private void ChangeMessage(string message)
        {
            if(_messageText != null)
                _messageText.text = message;
        }

        private void ChangeConfirmButtonText(string text)
        {
            if (_confirmBTNText != null)
                _confirmBTNText.text = text;
        }

        private void ChangeCancelButtonText(string text)
        {
            if (_cancelBTNText != null)
                _cancelBTNText.text = text;
        }

        private void HideMessageBox()
        {
            gameObject.SetActive(false);
        }

        private void HideCancelBTN()
        {
            _cancelBTNObject?.SetActive(false);
        }
        
        public void ShowMessageBox(string message, string id, FCSMessageBox buttons = FCSMessageBox.YesNo)
        {
            Initialize();
            ChangeMessage(message);
            _messageId = id;
            RefreshButtons(buttons);
            gameObject.SetActive(true);
        }

        private void RefreshButtons(FCSMessageBox buttons)
        {
            switch (buttons)
            {
                case FCSMessageBox.OK:
                    ChangeConfirmButtonText("OK");
                    HideCancelBTN();
                    break;
                case FCSMessageBox.OKCancel:
                    ChangeConfirmButtonText("OK");
                    ChangeCancelButtonText("CANCEL");
                    break;
                case FCSMessageBox.RetryCancel:
                    ChangeConfirmButtonText("RETRY");
                    ChangeCancelButtonText("CANCEL");
                    break;
                case FCSMessageBox.YesNo:
                    ChangeConfirmButtonText("YES");
                    ChangeCancelButtonText("NO");
                    break;
            }
        }
    }
}
