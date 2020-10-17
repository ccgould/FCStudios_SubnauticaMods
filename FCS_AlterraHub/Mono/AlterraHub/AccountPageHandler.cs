using System;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class AccountPageHandler
    {
        private AlterraHubController _mono;
        private readonly GameObject _accountCreation;
        private GameObject _welcomeScreen;
        private GameObject _noCardScreen;
        private string _fullName;
        private string _userName;
        private string _password;
        private string _pin;
        private GameObject _paymentScreen;
        private float _pendingPayment;
        private Text _newBalance;
        private Text _accountBalance;
        private InputField _paymentInput;
        private Text _debitBalance;

        internal AccountPageHandler(AlterraHubController mono)
        {
            _mono = mono;

            var accountPage = GameObjectHelpers.FindGameObject(mono.gameObject, "AccountPage");
            
            GameObjectHelpers.FindGameObject(accountPage, "PageTitle").GetComponentInChildren<Text>().text = Buildables.AlterraHub.Account();
            
            _accountCreation = GameObjectHelpers.FindGameObject(accountPage, "AccountCreation");

            _noCardScreen = GameObjectHelpers.FindGameObject(accountPage, "NoCardScreen");

            GameObjectHelpers.FindGameObject(_noCardScreen, "MessageLBL").GetComponent<Text>().text =
                Buildables.AlterraHub.CardNotDetected();

            _paymentScreen = GameObjectHelpers.FindGameObject(mono.gameObject, "DebitDialog");
            
            _newBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "NewBalance").GetComponent<Text>();

            _accountBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "AccountBalance").GetComponent<Text>();
            
            _debitBalance = GameObjectHelpers.FindGameObject(_paymentScreen, "DebitBalance").GetComponent<Text>();

            _paymentInput = _paymentScreen.GetComponentInChildren<InputField>();
            _paymentInput.onEndEdit.AddListener((value) =>
            {
                if(float.TryParse(value,out float result))
                {
                    _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(CardSystem.main.AlterraBalance() - result);
                    _pendingPayment = result;
                }
            });

            var paymentButton = GameObjectHelpers.FindGameObject(_paymentScreen, "SubmitPaymentBTN").GetComponent<Button>();
            paymentButton.onClick.AddListener((() =>
            {
                if (CardSystem.main.HasEnough(_pendingPayment))
                {
                    CardSystem.main.PayDebit(_pendingPayment);
                }
                else
                {
                    MessageBoxHandler.main.Show(Buildables.AlterraHub.NotEnoughMoneyOnAccount());
                }

                HidePaymentScreen();
            }));

            var closeButton = GameObjectHelpers.FindGameObject(_paymentScreen, "CloseBTN").GetComponent<Button>();
            closeButton.onClick.AddListener((() =>
            {
                ResetPaymentScreen();
                HidePaymentScreen();
            }));

            mono.AlterraHubTrigger.onTriggered += value =>
            {
                SwitchToCorrectPage();
            };

            SetupFullTitle(accountPage);

            SetupUserField(accountPage);

            SetupPasswordField(accountPage);

            SetupPINField(accountPage);

            SetupSubmitButton();

            CreateWelcomePage(accountPage);
            
            SwitchToCorrectPage();
        }

        private void HidePaymentScreen()
        {
            _paymentScreen.SetActive(false);
        }

        private void ShowPaymentScreen()
        {
            _paymentScreen.SetActive(true);
        }

        private void ResetPaymentScreen()
        {
            _paymentInput.text = string.Empty;
            _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            _accountBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            _debitBalance.text = Buildables.AlterraHub.DebitBalanceFormat(0);
        }

        private void SetupSubmitButton()
        {
            var button = _accountCreation.GetComponentInChildren<Button>();
            button.onClick.AddListener((() =>
            {
                CardSystem.main.CreateUserAccount(_fullName, _userName, _password, _pin);
                SwitchToCorrectPage();
            }));
        }

        private void SetupFullTitle(GameObject accountPage)
        {
            GameObjectHelpers.FindGameObject(accountPage, "FullNameTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.FullName();
            var fullNameInputField = GameObjectHelpers.FindGameObject(accountPage, "FullNameInputField")
                .GetComponentInChildren<InputField>();
            fullNameInputField.onEndEdit.AddListener((value => { _fullName = value; }));
            GameObjectHelpers.FindGameObject(accountPage, "FullNamePlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.FullNamePlaceholder();
        }

        private void SetupUserField(GameObject accountPage)
        {
            var userNameInputField = GameObjectHelpers.FindGameObject(accountPage, "UserNameInputField")
                .GetComponentInChildren<InputField>();
            userNameInputField.onEndEdit.AddListener((value => { _userName = value; }));
            GameObjectHelpers.FindGameObject(accountPage, "UserNamePlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.UserNamePlaceholder();
            GameObjectHelpers.FindGameObject(accountPage, "UserNameTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.UserName();
        }

        private void SetupPasswordField(GameObject accountPage)
        {
            var passwordInputField = GameObjectHelpers.FindGameObject(accountPage, "PasswordInputField")
                .GetComponentInChildren<InputField>();
            passwordInputField.onEndEdit.AddListener((value => { _password = value; }));
            GameObjectHelpers.FindGameObject(accountPage, "PasswordPlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PasswordPlaceholder();
            GameObjectHelpers.FindGameObject(accountPage, "PasswordTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.Password();
        }

        private void SetupPINField(GameObject accountPage)
        {
            var pinInputField = GameObjectHelpers.FindGameObject(accountPage, "PINInputField")
                .GetComponentInChildren<InputField>();
            pinInputField.onEndEdit.AddListener((value => { _pin = value; }));
            GameObjectHelpers.FindGameObject(accountPage, "PINPlaceholder").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PINPlaceholder();
            GameObjectHelpers.FindGameObject(accountPage, "PINTitle").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.PIN();
        }

        private void SwitchToCorrectPage()
        {
            QuickLogger.Debug("Switch to correct page");
            if (!CardSystem.main.HasBeenRegistered())
            {
                QuickLogger.Debug("Switched to account creation page");
                _accountCreation.SetActive(true);
                _welcomeScreen.SetActive(false);
                _noCardScreen.SetActive(false);
            }
            else
            {
                if (_mono.IsPlayerInRange() && PlayerInteractionHelper.HasCard())
                {
                    QuickLogger.Debug("Switched to welcome page");
                    _noCardScreen.SetActive(false);
                    _welcomeScreen.SetActive(true);
                }
                else
                {
                    QuickLogger.Debug("Switched to no card page");
                    _noCardScreen.SetActive(true);
                    _welcomeScreen.SetActive(false);
                }

                _accountCreation.SetActive(false);
            }
        }

        private void CreateWelcomePage(GameObject accountPage)
        {
            _welcomeScreen = GameObjectHelpers.FindGameObject(accountPage, "Welcome Screen");
            GameObjectHelpers.FindGameObject(_welcomeScreen, "MessageLBL").GetComponentInChildren<Text>().text =
                Buildables.AlterraHub.WelcomeBack();
            
            

            GameObjectHelpers.FindGameObject(_welcomeScreen, "UserName").GetComponentInChildren<Text>().text = CardSystem.main.GetUserName();

            var requestButton = _noCardScreen.GetComponentInChildren<Button>();

            requestButton.onClick.AddListener(() =>
            {
                //Check if player has any FiberMesh and Magniette
                if (!PlayerHasIngredients())
                {
                    MessageBoxHandler.main.Show(string.Format(Buildables.AlterraHub.CardRequirementsMessageFormat(),
                        LanguageHelpers.GetLanguage(TechType.FiberMesh), LanguageHelpers.GetLanguage(TechType.Magnetite)));
                    return;
                }

                PlayerInteractionHelper.GivePlayerItem(Mod.DebitCardTechType);
                RemoveItemsFromContainer();
            });


            var repayButton = _welcomeScreen.GetComponentInChildren<Button>();

            repayButton.onClick.AddListener(() =>
            {
                ResetPaymentScreen();
                _debitBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.AlterraBalance());
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.AccountDetails.Balance);
                ShowPaymentScreen();
            });

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
    }
}