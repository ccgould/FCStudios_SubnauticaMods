using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs
{
    internal class AccountPageHandler : Page
    {
        private string _fullName;
        private string _userName;
        private string _password;
        private string _pin;
        private GameObject _paymentScreen;
        private decimal _pendingPayment;
        private Text _newBalance;
        private Text _accountBalance;
        private InputField _paymentInput;
        private Text _debtBalance;
        private Text _userNameLBL;
        private GameObject _createAccountDialog;
        private Text _requestButtonText;
        private IFCSAlterraHubGUI _mono;
        private  Text _sliderText;
        private  Slider _slider;
        private Toggle _deductionToggle;
        private FCSAlterraHubGUISender _sender;

        internal void Initialize(FCSAlterraHubGUI mono)
        {
            _mono = mono;
            _sender = mono.SenderType;

            var accountPage = GameObjectHelpers.FindGameObject(mono.gameObject, "AccountPage");

            _createAccountDialog = GameObjectHelpers.FindGameObject(mono.gameObject, "CreateAccountDialog");

            _slider = GameObjectHelpers.FindGameObject(accountPage, "Slider").GetComponent<Slider>();
            _sliderText = _slider.GetComponentInChildren<Text>();
            _slider.onValueChanged.AddListener(value =>
            {
                var rate = _slider.value * 10f;
                _sliderText.text = $"{Mathf.CeilToInt(rate)}%";
                GamePlayService.Main.SetRate(rate);
            });

            _deductionToggle = GameObjectHelpers.FindGameObject(accountPage, "AutomaticDebitDeduction").GetComponent<Toggle>();
            _deductionToggle.onValueChanged.AddListener(value =>
            {
                GamePlayService.Main.SetAutomaticDebitDeduction(value);
            });

            var createAccountDialogCloseBtn = GameObjectHelpers.FindGameObject(_createAccountDialog, "CloseBTN").GetComponent<Button>();
            createAccountDialogCloseBtn.onClick.AddListener(() =>
            {
                _createAccountDialog.SetActive(false);
            });

            _paymentScreen = GameObjectHelpers.FindGameObject(mono.gameObject, "DebtDialog");

            _newBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "NewBalance").GetComponent<Text>();

            _accountBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "AccountBalance").GetComponent<Text>();

            _debtBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "DebtBalance").GetComponent<Text>();

            _paymentInput = _paymentScreen.GetComponentInChildren<InputField>();

            _paymentInput.onEndEdit.AddListener((value) =>
            {

                if (_paymentInput.text.Contains("-"))
                {
                    return;
                }

                if (decimal.TryParse(value, out decimal result))
                {
                    _newBalance.text = LanguageService.AccountNewBalanceFormat(AccountService.main.AlterraBalance() + result);
                    _pendingPayment = result;
                }
            });

            var paymentButton = GameObjectHelpers.FindGameObject(_paymentScreen, "SubmitPaymentBTN").GetComponent<Button>();
            paymentButton.onClick.AddListener(() =>
            {
                if (_paymentInput.text.Contains("-"))
                {
                    _mono.ShowMessage(LanguageService.NegativeNumbersNotAllowed());
                    _paymentInput.text = string.Empty;
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

                HidePaymentScreen();
            });

            var closeButton = GameObjectHelpers.FindGameObject(_paymentScreen, "CloseBTN").GetComponent<Button>();
            closeButton.onClick.AddListener(() =>
            {
                ResetPaymentScreen();
                HidePaymentScreen();
            });


            SetupFullTitle(_createAccountDialog);

            SetupUserField(_createAccountDialog);

            SetupPasswordField(_createAccountDialog);

            SetupPINField(_createAccountDialog);

            var createBTN = _createAccountDialog.GetComponentInChildren<Button>();
            createBTN.onClick.AddListener(() =>
            {
                if (PlayerInteractionHelper.CanPlayerHold(DebitCardSpawnable.PatchedTechType))
                {
                    if (AccountService.main.CreateUserAccount(_fullName, _userName, _password, _pin, 0, _sender))
                    {
                        _userNameLBL.text = AccountService.main.GetUserName();
                        _createAccountDialog.SetActive(false);
                        UpdateRequestBTN(true);
                    }
                }
                else
                {
                    _mono.ShowMessage(LanguageService.NoSpaceAccountCreation());
                }
            });

            CreateWelcomePage(accountPage);
        }

        private void HidePaymentScreen()
        {
            _paymentScreen.SetActive(false);
        }

        private void ShowPaymentScreen()
        {
            ResetPaymentScreen();
            _paymentScreen.SetActive(true);
            _accountBalance.text = LanguageService.AccountBalanceFormat(AccountService.main.GetAccountBalance());
            _debtBalance.text = LanguageService.DebtBalanceFormat(AccountService.main.AlterraBalance());
        }

        private void ResetPaymentScreen()
        {
            _paymentInput.text = string.Empty;
            _newBalance.text = LanguageService.AccountNewBalanceFormat(0);
            _accountBalance.text = LanguageService.AccountBalanceFormat(0);
            _debtBalance.text = LanguageService.DebtBalanceFormat(0);
        }

        private void SetupFullTitle(GameObject dialog)
        {
            GameObjectHelpers.FindGameObject(dialog, "FullNameTitle").GetComponentInChildren<Text>().text =
                LanguageService.FullName();
            var fullNameInputField = GameObjectHelpers.FindGameObject(dialog, "FullNameInputField")
                .GetComponentInChildren<InputField>();

            fullNameInputField.onEndEdit.AddListener(value => { _fullName = value; });
            GameObjectHelpers.FindGameObject(dialog, "FullNamePlaceholder").GetComponentInChildren<Text>().text =
                LanguageService.FullNamePlaceholder();
        }

        private void SetupUserField(GameObject dialog)
        {
            var userNameInputField = GameObjectHelpers.FindGameObject(dialog, "UserNameInputField")
                .GetComponentInChildren<InputField>();
            userNameInputField.onEndEdit.AddListener(value => { _userName = value; });
            GameObjectHelpers.FindGameObject(dialog, "UserNamePlaceholder").GetComponentInChildren<Text>().text =
                LanguageService.UserNamePlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "UserNameTitle").GetComponentInChildren<Text>().text =
                LanguageService.UserName();
        }

        private void SetupPasswordField(GameObject dialog)
        {
            var passwordInputField = GameObjectHelpers.FindGameObject(dialog, "PasswordInputField")
                .GetComponentInChildren<InputField>();
            passwordInputField.onEndEdit.AddListener(value => { _password = value; });
            GameObjectHelpers.FindGameObject(dialog, "PasswordPlaceholder").GetComponentInChildren<Text>().text =
                LanguageService.PasswordPlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "PasswordTitle").GetComponentInChildren<Text>().text =
                LanguageService.Password();
        }

        private void SetupPINField(GameObject dialog)
        {
            var pinInputField = GameObjectHelpers.FindGameObject(dialog, "PINInputField")
                .GetComponentInChildren<InputField>();
            pinInputField.onEndEdit.AddListener(value => { _pin = value; });
            GameObjectHelpers.FindGameObject(dialog, "PINPlaceholder").GetComponentInChildren<Text>().text =
                LanguageService.PINPlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "PINTitle").GetComponentInChildren<Text>().text =
                LanguageService.PIN();
        }

        private void CreateWelcomePage(GameObject accountPage)
        {
            QuickLogger.Info("0");
            QuickLogger.Info($"Account Page Name: {accountPage?.transform?.parent?.gameObject?.name}");
            _userNameLBL = GameObjectHelpers.FindGameObject(accountPage.transform.parent.gameObject, "UserName").GetComponentInChildren<Text>();
            QuickLogger.Info($"1 {_userNameLBL is null}");
            _userNameLBL.text = AccountService.main.GetUserName();
            QuickLogger.Info("2");
            var requestButton = GameObjectHelpers.FindGameObject(accountPage, "CardRequestBTN").GetComponent<Button>();
            QuickLogger.Info("3");
            _requestButtonText = requestButton.GetComponentInChildren<Text>();
            QuickLogger.Info("4");
            UpdateRequestBTN(AccountService.main.HasBeenRegistered());
            QuickLogger.Info("5");
            requestButton.onClick.AddListener(() =>
            {
                if (AccountService.main.HasBeenRegistered())
                {
                    //Check if player has any FiberMesh and Magniette
                    if (!PlayerHasIngredients())
                    {
                        _mono.ShowMessage(string.Format(LanguageService.CardRequirementsMessageFormat(),
                            LanguageService.GetLanguage(TechType.FiberMesh), LanguageService.GetLanguage(TechType.Magnetite)));
                        return;
                    }

                    PlayerInteractionHelper.GivePlayerItem(DebitCardSpawnable.PatchedTechType);
                    RemoveItemsFromContainer();
                }
                else
                {
                    _createAccountDialog.SetActive(true);
                }
            });
            QuickLogger.Info("6");
            var payDebitButton = GameObjectHelpers.FindGameObject(accountPage, "RepayBTN").GetComponent<Button>();
            QuickLogger.Info("7");
            payDebitButton.onClick.AddListener(ShowPaymentScreen);
            QuickLogger.Info("8");
        }

        internal void UpdateRequestBTN(bool accountReg)
        {
            _requestButtonText.text = accountReg ? LanguageService.RequestNewCard() : LanguageService.CreateNewAccount();
        }

        private static void RemoveItemsFromContainer()
        {
            var fiberMesh = Inventory.main.container.RemoveItem(TechType.FiberMesh);
            Object.Destroy(fiberMesh.gameObject);
            var magnetite = Inventory.main.container.RemoveItem(TechType.Magnetite);
            Object.Destroy(magnetite.gameObject);
        }

        private bool PlayerHasIngredients()
        {
            var container = Inventory.main.container;
            return container.Contains(TechType.FiberMesh) && container.Contains(TechType.Magnetite);
        }

        internal void Refresh()
        {
            _slider.SetValueWithoutNotify(GamePlayService.Main.GetRate() / 10f);
            _sliderText.text = $"{Mathf.CeilToInt(GamePlayService.Main.GetRate())}%";
            _deductionToggle.SetIsOnWithoutNotify(GamePlayService.Main.GetAutomaticDebitDeduction());
        }

        public void Close()
        {
            ResetPaymentScreen();
            HidePaymentScreen();
        }

        public override void Enter(object arg = null)
        {
            base.Enter();
            UpdateRequestBTN(AccountService.main.HasBeenRegistered());
        }
    }
}