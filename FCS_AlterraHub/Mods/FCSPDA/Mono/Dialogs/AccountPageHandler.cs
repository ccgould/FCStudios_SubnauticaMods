using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs
{
    internal class AccountPageHandler
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
        private FCSAlterraHubGUI _mono;
        private readonly Text _sliderText;
        private readonly Slider _slider;
        private readonly Toggle _deductionToggle;

        internal AccountPageHandler(FCSAlterraHubGUI mono)
        {
            _mono = mono;

            var accountPage = GameObjectHelpers.FindGameObject(mono.gameObject, "AccountPage");

            _createAccountDialog = GameObjectHelpers.FindGameObject(mono.gameObject, "CreateAccountDialog");

            _slider = GameObjectHelpers.FindGameObject(accountPage, "Slider").GetComponent<Slider>();
            _sliderText = _slider.GetComponentInChildren<Text>();
            _slider.onValueChanged.AddListener((value =>
            {
                _sliderText.text = $"{Mathf.CeilToInt(GetRate())}%";
            }));

            _deductionToggle = GameObjectHelpers.FindGameObject(accountPage, "AutomaticDebitDeduction").GetComponent<Toggle>();


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

                if (decimal.TryParse(value,out decimal result))
                {
                    _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(CardSystem.main.AlterraBalance() + result);
                    _pendingPayment = result;
                }
            });

            var paymentButton = GameObjectHelpers.FindGameObject(_paymentScreen, "SubmitPaymentBTN").GetComponent<Button>();
            paymentButton.onClick.AddListener((() =>
            {
                if (_paymentInput.text.Contains("-"))
                {
                    MessageBoxHandler.main.Show(Buildables.AlterraHub.NegativeNumbersNotAllowed(), FCSMessageButton.OK);
                    _paymentInput.text = string.Empty;
                    return;
                }
                if (CardSystem.main.HasEnough(_pendingPayment))
                {
                    if (CardSystem.main.IsDebitPaid())
                    {
                        QuickLogger.ModMessage(AlterraHub.DebitHasBeenPaid());
                        return;
                    }
                    CardSystem.main.PayDebit(_pendingPayment);
                }
                else
                {
                    MessageBoxHandler.main.Show(Buildables.AlterraHub.NotEnoughMoneyOnAccount(),FCSMessageButton.OK,null);
                }

                HidePaymentScreen();
            }));

            var closeButton = GameObjectHelpers.FindGameObject(_paymentScreen, "CloseBTN").GetComponent<Button>();
            closeButton.onClick.AddListener((() =>
            {
                ResetPaymentScreen();
                HidePaymentScreen();
            }));


            SetupFullTitle(_createAccountDialog);

            SetupUserField(_createAccountDialog);

            SetupPasswordField(_createAccountDialog);

            SetupPINField(_createAccountDialog);
            
            var createBTN = _createAccountDialog.GetComponentInChildren<Button>();
            createBTN.onClick.AddListener(() =>
            {
                if (PlayerInteractionHelper.CanPlayerHold(Mod.DebitCardTechType))
                {
                    if (CardSystem.main.CreateUserAccount(_fullName, _userName, _password, _pin))
                    {
                        _userNameLBL.text = CardSystem.main.GetUserName();
                        _createAccountDialog.SetActive(false);
                        UpdateRequestBTN(true);
                    }
                }
                else
                {
                    MessageBoxHandler.main.Show(Buildables.AlterraHub.NoSpaceAccountCreation(),FCSMessageButton.OK,null);
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
            _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.GetAccountBalance());
            _debtBalance.text = Buildables.AlterraHub.DebtBalanceFormat(CardSystem.main.AlterraBalance());
        }

        private void ResetPaymentScreen()
        {
            _paymentInput.text = string.Empty;
            _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
            _debtBalance.text = Buildables.AlterraHub.DebtBalanceFormat(0);
        }

        private void SetupFullTitle(GameObject dialog)
        {
            GameObjectHelpers.FindGameObject(dialog, "FullNameTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.FullName();
            var fullNameInputField = GameObjectHelpers.FindGameObject(dialog, "FullNameInputField")
                .GetComponentInChildren<InputField>();

            fullNameInputField.onEndEdit.AddListener((value => { _fullName = value; }));
            GameObjectHelpers.FindGameObject(dialog, "FullNamePlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.FullNamePlaceholder();
        }

        private void SetupUserField(GameObject dialog)
        {
            var userNameInputField = GameObjectHelpers.FindGameObject(dialog, "UserNameInputField")
                .GetComponentInChildren<InputField>();
            userNameInputField.onEndEdit.AddListener((value => { _userName = value; }));
            GameObjectHelpers.FindGameObject(dialog, "UserNamePlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.UserNamePlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "UserNameTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.UserName();
        }

        private void SetupPasswordField(GameObject dialog)
        {
            var passwordInputField = GameObjectHelpers.FindGameObject(dialog, "PasswordInputField")
                .GetComponentInChildren<InputField>();
            passwordInputField.onEndEdit.AddListener((value => { _password = value; }));
            GameObjectHelpers.FindGameObject(dialog, "PasswordPlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PasswordPlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "PasswordTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.Password();
        }

        private void SetupPINField(GameObject dialog)
        {
            var pinInputField = GameObjectHelpers.FindGameObject(dialog, "PINInputField")
                .GetComponentInChildren<InputField>();
            pinInputField.onEndEdit.AddListener((value => { _pin = value; }));
            GameObjectHelpers.FindGameObject(dialog, "PINPlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PINPlaceholder();
            GameObjectHelpers.FindGameObject(dialog, "PINTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PIN();
        }
        
        private void CreateWelcomePage(GameObject accountPage)
        {
            _userNameLBL = GameObjectHelpers.FindGameObject(_mono.gameObject, "UserName").GetComponentInChildren<Text>();
            _userNameLBL.text = CardSystem.main.GetUserName();
            
            var requestButton = GameObjectHelpers.FindGameObject( accountPage, "CardRequestBTN").GetComponent<Button>();
            _requestButtonText = requestButton.GetComponentInChildren<Text>();
            UpdateRequestBTN(CardSystem.main.HasBeenRegistered());

            requestButton.onClick.AddListener(() =>
            {
                if (CardSystem.main.HasBeenRegistered())
                {
                    //Check if player has any FiberMesh and Magniette
                    if (!PlayerHasIngredients())
                    {
                        MessageBoxHandler.main.Show(string.Format(Buildables.AlterraHub.CardRequirementsMessageFormat(),
                            LanguageHelpers.GetLanguage(TechType.FiberMesh), LanguageHelpers.GetLanguage(TechType.Magnetite)),FCSMessageButton.OK,null);
                        return;
                    }

                    PlayerInteractionHelper.GivePlayerItem(Mod.DebitCardTechType);
                    RemoveItemsFromContainer();
                }
                else
                {
                    _createAccountDialog.SetActive(true);
                }
            });

            var payDebitButton = GameObjectHelpers.FindGameObject(accountPage, "RepayBTN").GetComponent<Button>();

            payDebitButton.onClick.AddListener(ShowPaymentScreen);
        }

        internal void UpdateRequestBTN(bool accountReg)
        {
            _requestButtonText.text = accountReg ? Buildables.AlterraHub.RequestNewCard() : Buildables.AlterraHub.CreateNewAccount();
        }

        private static void RemoveItemsFromContainer()
        {
            var fiberMesh = Inventory.main.container.RemoveItem(TechType.FiberMesh);
            GameObject.Destroy(fiberMesh.gameObject);
            var magnetite = Inventory.main.container.RemoveItem(TechType.Magnetite);
            GameObject.Destroy(magnetite.gameObject);
        }

        private bool PlayerHasIngredients()
        {
            var container = Inventory.main.container;
            return container.Contains(TechType.FiberMesh) && container.Contains(TechType.Magnetite);
        }

        internal void Refresh()
        {
            _slider.SetValueWithoutNotify(Mod.GamePlaySettings.Rate / 10f);
            _sliderText.text = $"{Mathf.CeilToInt(Mod.GamePlaySettings.Rate)}%";

            _deductionToggle.SetIsOnWithoutNotify(Mod.GamePlaySettings.AutomaticDebitDeduction);
        }

        public void Close()
        {
            ResetPaymentScreen();
            HidePaymentScreen();
        }

        public float GetRate()
        {
           return _slider.value * 10f;
        }

        public bool GetAutomaticDebitDeduction()
        {
            return _deductionToggle.isOn;
        }
    }
}