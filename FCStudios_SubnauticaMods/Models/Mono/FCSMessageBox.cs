using FCS_AlterraHub.Core.Helpers;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Models.Mono;

public class FCSMessageBox : MonoBehaviour
{
    [SerializeField]
    private Text _message;
    [SerializeField]
    private Button _yesBTN;
    [SerializeField]
    private Button _noBTN;
    [SerializeField]
    private Button _okBTN;
    [SerializeField]
    private Button _cancelBTN;
    private Action<FCSMessageResult> _result;
    private bool _initialize;

    private void Initialize()
    {
        if (_initialize) return;
        _initialize = true;
    }

    public void SendResponse(int result)
    {
        _result?.Invoke((FCSMessageResult)result);
        Close();
    }

    public void Show(string message, FCSMessageButton button, Action<FCSMessageResult> result)
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

        QuickLogger.Debug($"MessageBox Message: {message}");
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

[Serializable]
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
    NO = -1,
    OKYES = 0,
    CANCEL = 1
}

