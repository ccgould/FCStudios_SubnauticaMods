using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
public class CreateAccountDialogPage : MonoBehaviour
{
    [Header("Username Field Settings")]
    [SerializeField]
    private Text _userNamePlaceholderText;
    [SerializeField]
    private Text _userNameTitleText;

    [Header("Pin Field Settings")]
    [SerializeField]
    private Text _pinPlaceHolderText;
    [SerializeField]
    private Text _pinTitleText;

    [Header("Password Field Settings")]
    [SerializeField]
    private Text _passwordPlaceHolderText;
    [SerializeField]
    private Text _passwordTitleText;

    [Header("Full Name Field Settings")]
    [SerializeField]
    private Text _fullNameTitleText;
    [SerializeField]
    private Text _fullNamePlaceHolderText;

    private string _fullName;
    private string _userName;
    private string _password;
    private string _pin;

    [Header("Additional Required Components")]
    [SerializeField]
    private AccountPageHandler _accountPageHandler;
    [SerializeField]
    private FCSAlterraHubGUI _mono;
    [SerializeField]
    private Text _userNameLBL;

    private FCSAlterraHubGUISender _sender;


    private void Awake()
    {
        _sender = _mono.SenderType;

        SetupFullTitle();

        SetupUserField();

        SetupPasswordField();

        SetupPINField();
    }

    public void OnCreateButtonClicked()
    {
        if (PlayerInteractionHelper.CanPlayerHold(DebitCardSpawnable.PatchedTechType))
        {
            if (AccountService.main.CreateUserAccount(_fullName, _userName, _password, _pin, 0, _sender))
            {
                _userNameLBL.text = AccountService.main.GetUserName();
                Hide();
                _accountPageHandler.UpdateRequestBTN(true);
            }
        }
        else
        {
            _mono.ShowMessage(LanguageService.NoSpaceAccountCreation());
        }
    }

    private void SetupFullTitle()
    {
        _fullNameTitleText.text = LanguageService.FullName();
        _fullNamePlaceHolderText.text = LanguageService.FullNamePlaceholder();
    }

    public void OnFullNameInputFieldEdited(string value)
    {
        _fullName = value;
    }

    private void SetupUserField()
    {
        _userNamePlaceholderText.text = LanguageService.UserNamePlaceholder();
        _userNameTitleText.text = LanguageService.UserName();
    }

    public void OnUsernameChanged(string value)
    {
        _userName = value;
    }

    private void SetupPasswordField()
    {
        _passwordPlaceHolderText.text = LanguageService.PasswordPlaceholder();
        _passwordTitleText.text = LanguageService.Password();
    }

    public void OnPasswordChanged(string value)
    {
        _password = value;
    }

    private void SetupPINField()
    {
        _pinPlaceHolderText.text = LanguageService.PINPlaceholder();
        _pinTitleText.text = LanguageService.PIN();
    }

    public void OnPinValueChanged(string value)
    {
        _pin = value;
    }

    internal void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
