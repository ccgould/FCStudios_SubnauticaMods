﻿using FCS_AlterraHub.Core.Services;
using FCSCommon.Utilities;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
public class DebtDialogController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _newBalance;
    [SerializeField]
    private TMP_Text _accountBalance;    
    [SerializeField]
    private TMP_Text _paymentInputText;
    [SerializeField]
    private TMP_Text _debtBalance;
    [SerializeField]
    private FCSAlterraHubGUI _mono;
    [SerializeField]
    private TMP_InputField _inputField;
    private decimal _pendingPayment;

    public void OnPaymentInputChanged(string value)
    {
        if (_paymentInputText.text.Contains("-"))
        {
            return;
        }

        if (decimal.TryParse(value, out decimal result))
        {
            _newBalance.text = LanguageService.AccountNewBalanceFormat(AccountService.main.AlterraBalance() + result);
            _pendingPayment = result;
        }
    }

    public void OnPaymentButtonClicked()
    {
        if (_paymentInputText.text.Contains("-"))
        {

            _mono.ShowMessage(LanguageService.NegativeNumbersNotAllowed());
            _paymentInputText.text = string.Empty;
            return;
        }
        if (AccountService.main.HasEnough(_pendingPayment))
        {
            if (AccountService.main.IsDebitPaid())
            {
                QuickLogger.ModMessage(LanguageService.DebitHasBeenPaid());
                return;
            }
            AccountService.main.PayDebit(_mono, _pendingPayment);
        }
        else
        {
            _mono.ShowMessage(LanguageService.NotEnoughMoneyOnAccount());
        }

        Close();
    }

    public void Show()
    {
        ResetPaymentScreen();
        gameObject.SetActive(true);
        _accountBalance.text = LanguageService.AccountBalanceFormat(AccountService.main.GetAccountBalance());
        _debtBalance.text = LanguageService.DebtBalanceFormat(AccountService.main.AlterraBalance());
    }

    public void Close()
    {
        ResetPaymentScreen();
        gameObject.SetActive(false);
    }

    private void ResetPaymentScreen()
    {
        _paymentInputText.text = string.Empty;
        _newBalance.text = LanguageService.AccountNewBalanceFormat(0);
        _accountBalance.text = LanguageService.AccountBalanceFormat(0);
        _debtBalance.text = LanguageService.DebtBalanceFormat(0);
        _inputField.SetTextWithoutNotify(string.Empty);
    }
}
