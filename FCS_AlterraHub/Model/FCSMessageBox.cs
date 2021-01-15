using System;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Model
{
    public class FCSMessageBox : MonoBehaviour
    {
        private Text _message;
        private Button _yesBTN;
        private Button _noBTN;
        private Button _okBTN;
        private Button _cancelBTN;
        private Action<FCSMessageResult> _result;
        private bool _initialize;

        private void Initialize()
        {
            if(_initialize)return;
            _message = GameObjectHelpers.FindGameObject(gameObject, "Message").GetComponent<Text>();
            _yesBTN = GameObjectHelpers.FindGameObject(gameObject, "YesBTN").GetComponent<Button>();
            _yesBTN.onClick.AddListener(() =>
            {
                _result?.Invoke(FCSMessageResult.OKYES); 
                Close();
            });
            _noBTN = GameObjectHelpers.FindGameObject(gameObject, "NoBTN").GetComponent<Button>();
            _noBTN.onClick.AddListener(() =>
            {
                _result?.Invoke(FCSMessageResult.NO);
                Close();
            });
            _okBTN = GameObjectHelpers.FindGameObject(gameObject, "OKBTN").GetComponent<Button>();
            _okBTN.onClick.AddListener(() =>
            {
                _result?.Invoke(FCSMessageResult.OKYES);
                Close();
            });
            _cancelBTN = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN")?.GetComponent<Button>();
            _cancelBTN.onClick.AddListener(() =>
            {
                _result?.Invoke(FCSMessageResult.CANCEL);
                Close();
            });
            _initialize = true;

        }

        public void Show(string message, FCSMessageButton button,Action<FCSMessageResult> result)
        {
            Initialize();
            _message.text = message;
            _result = result;
            
            switch (button)
            {
                case FCSMessageButton.NO:
                    _noBTN.gameObject.SetActive(true);
                    break;
                case FCSMessageButton.OK:
                    _okBTN.gameObject.SetActive(true);
                    break;
                case FCSMessageButton.YES:
                    _yesBTN.gameObject.SetActive(true);
                    break;
                case FCSMessageButton.CANCEL:
                    _cancelBTN.gameObject.SetActive(true);
                    break;
                case FCSMessageButton.YESNO:
                    _cancelBTN.gameObject.SetActive(true);
                    _yesBTN.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Initialize();
            _message.text = string.Empty;
            _cancelBTN.gameObject.SetActive(false);
            _yesBTN.gameObject.SetActive(false);
            _okBTN.gameObject.SetActive(false);
            _noBTN.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }
    }

    public enum FCSMessageButton
    {
        NO,
        OK,
        YES,
        CANCEL,
        YESNO
    }

    public enum FCSMessageResult
    {
        NO,
        OKYES,
        CANCEL
    }
}
