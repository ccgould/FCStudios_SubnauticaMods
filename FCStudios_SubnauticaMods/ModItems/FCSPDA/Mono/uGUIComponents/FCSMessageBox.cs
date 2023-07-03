using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;

public class FCSMessageBox : Page
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

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        var data = arg as Tuple<string,FCSMessageButton,Action<FCSMessageResult>>;

        if(data is not null)
        {
            Show(data.Item1, data.Item2, data.Item3);
        }
    }

    public override void Exit()
    {
        base.Exit();

        _message.text = string.Empty;
        _cancelBTN.gameObject.SetActive(false);
        _yesBTN.gameObject.SetActive(false);
        _okBTN.gameObject.SetActive(false);
        _noBTN.gameObject.SetActive(false);
    }

    //Called by Unity Button
    public void SendResponse(MessageBoxResultSO messageBoxResultSO)
    {
        _result?.Invoke(messageBoxResultSO.Result);
    }

    private void Show(string message, FCSMessageButton button, Action<FCSMessageResult> result)
    {
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

[Serializable]
public enum FCSMessageResult
{
    NO = -1,
    OKYES = 0,
    CANCEL = 1
}

