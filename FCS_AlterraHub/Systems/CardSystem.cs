using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
using Random = System.Random;
#if SUBNAUTICA_STABLE

#else
#endif


namespace FCS_AlterraHub.Systems
{
    public class CardSystem
    {
        private Random _random;

        private AccountDetails _accountDetails = new();
        public static CardSystem main = new();
        private const decimal AlterraDebit = -1000000000000000000;
        public Action onBalanceUpdated;
        private bool _hasBeenSaved;
        private bool _accountLoaded;

        public TechType CardTechType { get; set; }
        
        /// <summary>
        /// Generates a new card number.
        /// </summary>
        /// <returns></returns>
        public void GenerateNewCard(string prefabId)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            
            if (_accountDetails == null)
            {
                _accountDetails = new AccountDetails();
            }
            
            int[] checkArray = new int[15];

            var cardNum = new int[16];

            for (int d = 14; d >= 0; d--)
            {
                cardNum[d] = _random.Next(0, 9);
                checkArray[d] = (cardNum[d] * (((d + 1) % 2) + 1)) % 9;
            }

            cardNum[15] = (checkArray.Sum() * 9) % 10;

            var sb = new StringBuilder();

            for (int d = 0; d < 16; d++)
            {
                sb.Append(cardNum[d].ToString());
                if (d == 3 || d == 7 || d == 11)
                {
                    sb.Append(" ");
                }
            }

            if (!_accountDetails.KnownCardNumbers.ContainsValue(sb.ToString()))
            {
                _accountDetails.KnownCardNumbers.Add(prefabId,sb.ToString());
            }
        }

        /// <summary>
        /// Deletes card from the network
        /// </summary>
        /// <param name="cardNumber"></param>
        public void DeleteCard(string cardNumber)
        {
            _accountDetails.KnownCardNumbers?.Remove(cardNumber);
        }

        /// <summary>
        /// Adds credit to the account.
        /// </summary>
        /// <param name="amount">The amount of credit to add to the account.</param>
        /// <returns>Boolean on success</returns>
        public bool AddFinances(decimal amount)
        {
            try
            {
                _accountDetails.Balance += amount;
                if (QPatch.Configuration.CreditMessageDelay == 0f)
                {
                    QuickLogger.CreditMessage(AlterraHub.CreditMessage(amount));
                }
                else
                {
                    CreditNotificationSystem.main.AddCredit(amount);
                }
                onBalanceUpdated?.Invoke();
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[AddFinances]: {e.Message}");
                QuickLogger.Error($"[AddFinances]: {e.StackTrace}");
                MessageBoxHandler.main.Show(AlterraHub.ErrorHasOccurred("0x0002"),FCSMessageButton.OK);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Removes credit from the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card</param>
        /// <param name="amount">The amount of credit to add to the account.</param>
        /// <param name="callBack">Callback method to call in-case of error</param>
        /// <returns>Boolean on success</returns>
        public bool RemoveFinances(decimal amount)
        {
            if (!GameModeUtils.RequiresPower()) return true;

            if (HasEnough(amount))
            {
                _accountDetails.Balance -= amount;
                onBalanceUpdated?.Invoke();
                return true;
            }
            
            MessageBoxHandler.main.Show(string.Format(AlterraHub.NotEnoughMoneyOnAccount(), 0), FCSMessageButton.OK);
            return false;
        }

        /// <summary>
        /// Gets the current balance of the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card.</param>
        /// <returns></returns>
        public decimal GetAccountBalance()
        {
            if (!GameModeUtils.RequiresPower()) return 99999999999999;
            return _accountDetails?.Balance ?? 0;
        }
        
        /// <summary>
        /// Saves the current accounts information.
        /// </summary>
        /// <returns></returns>
        public AccountDetails SaveDetails()
        {
            _hasBeenSaved = true;
            QuickLogger.Debug($"Attempting to save account details {_accountDetails?.FullName}", true);
            if (_accountDetails != null)
            {
                _accountDetails.AccountBalance = EncodeDecode.Encrypt(_accountDetails.Balance.ToString(CultureInfo.InvariantCulture));
            }
            return _accountDetails /*_accountDetails != null ? _accountDetails : new AccountDetails()*/ ;
        }

        /// <summary>
        /// Loads saved account details
        /// </summary>
        /// <param name="accounts"></param>
        public void Load(AccountDetails account)
        {
            try
            {

                if (_accountLoaded)
                {
                    QuickLogger.Debug($"Account already loaded. Canceling load operation for {account.Username}.", true);
                    return;
                }

                if (account != null)
                {
                    _accountDetails = account;
                    if (account.Version.Equals("2.0"))
                    {
                        _accountDetails.Balance = Convert.ToDecimal(EncodeDecode.Decrypt(account.AccountBalance));
                    }

                    QuickLogger.Debug($"Alterra account loaded for player {account.Username}",true);
                    QuickLogger.Debug($"{account.Username} | Funds: {_accountDetails.Balance}",true);
                    QuickLogger.Debug($"{account.Username} | Debt Payed: {_accountDetails.AlterraDebitPayed}",true);

                    _accountLoaded = true;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"StackTrace: {e.StackTrace}");
                QuickLogger.Error($"Message: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if the account has enough money to purchase the items.
        /// </summary>
        /// <param name="cost">Amount to check for</param>
        /// <returns></returns>
        public bool HasEnough(decimal cost)
        {
            if (!GameModeUtils.RequiresPower()) return true;
            return _accountDetails.Balance >= cost;
        }

        /// <summary>
        /// Gets the card number from the system based off the prefabID
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public string GetCardNumber(string prefabId)
        {
            return _accountDetails.KnownCardNumbers[prefabId] ?? string.Empty;
        }
        
        /// <summary>
        /// Checks the system to see if there is a key entry for this prefab
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public bool CardExistFromPrefabID(string prefabId)
        {
            return _accountDetails.KnownCardNumbers.ContainsKey(prefabId);
        }

        /// <summary>
        /// Checks the system to see if there is a key entry for this card number
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public bool CardExist(string cardNumber)
        {
            return _accountDetails.KnownCardNumbers.ContainsValue(cardNumber);
        }

        /// <summary>
        /// Creates an account to be used by the Alterrahub
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="pin"></param>
        public bool CreateUserAccount(string fullName, string userName, string password, string pin,decimal accountBalance = 0)
        {
            if (_accountDetails == null)
            {
                _accountDetails = new AccountDetails();
            }

            if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(userName) &&
                !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(pin))
            {
                _accountDetails.FullName = fullName;
                _accountDetails.Username = userName;
                _accountDetails.Password = EncodeDecode.CreateMD5(password);
                _accountDetails.PIN = EncodeDecode.CreateMD5(pin);
                _accountDetails.Balance = accountBalance;
                CalculateBalance();

                if (HasBeenRegistered())
                {
                    MessageBoxHandler.main.Show(AlterraHub.AccountCreated(GetAccountBalance().ToString("N0")), FCSMessageButton.OK);
                    if (!PlayerInteractionHelper.HasCard())
                    {
                        var newCard = PlayerInteractionHelper.GivePlayerItem(Mod.DebitCardTechType);
                        //GenerateNewCard(newCard.gameObject.GetComponent<PrefabIdentifier>().Id);
                    }

                    VoiceNotificationSystem.main.Play("PDA_Account_Created_key");
                    return true;
                }

                _accountDetails = null;
                return false;
            }

            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                sb.Append(AlterraHub.FullName());
                sb.Append(",");
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                sb.Append(AlterraHub.UserName());
                sb.Append(",");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                sb.Append(AlterraHub.Password());
                sb.Append(",");
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                sb.Append(AlterraHub.PIN());
                sb.Append(",");
            }

            MessageBoxHandler.main.Show(AlterraHub.AccountSetupError(sb.ToString()), FCSMessageButton.OK);

            return false;
            //QPatch.MissionManagerGM.NotifyDeviceAction(Mod.AlterraHubTechType,Mod.DebitCardTechType,DeviceActionType.CREATEITEM);
        }

        public void CalculateBalance()
        {
            //if (QPatch.Configuration.GameModeOption == FCSGameMode.HardCore)
            //{
            //    if (!IsAlterraRepaid())
            //    {
            //        AccountDetails.AccountBeforeDebit = AccountDetails.Balance;
            //        AccountDetails.Balance += AlterraDebit;
            //    }

            //}
            //else if (QPatch.Configuration.GameModeOption == FCSGameMode.Normal)
            //{
            //    if (AccountDetails.Balance <= AlterraDebit)
            //    {
            //        AccountDetails.Balance += Mathf.Abs(AlterraDebit);
            //    }
            //}

            //onBalanceUpdated?.Invoke(AccountDetails.Balance);
        }

        private bool IsAlterraRepaid()
        {
            return _accountDetails.AlterraDebitPayed >= AlterraDebit;
        }

        public bool PayDebit(decimal amount)
        {
            QuickLogger.Debug($"Trying to add {amount} to {AlterraHub.DebtBalanceFormat(AlterraBalance())}", true);

            // Check if debit needs to be paid.
            if (IsDebitPaid())
            {
                return false;
            }

            var abs = AmountNeededForDebt();

            var amountToTake = abs >= amount ? amount : abs;

            _accountDetails.AlterraDebitPayed += amountToTake;
            RemoveFinances(amountToTake);

            QuickLogger.CreditMessage($"Deducted {amount} from the account new balance is {_accountDetails.Balance}");
            return true;
        }

        public decimal AmountNeededForDebt()
        {
            var amountNeeded = AlterraDebit + _accountDetails.AlterraDebitPayed;

            var abs = Math.Abs(amountNeeded);
            return abs;
        }

        public bool IsDebitPaid()
        {
            return AlterraDebit + _accountDetails.AlterraDebitPayed >= 0;
        }

        public string GetUserName()
        {
            if (_accountDetails == null) return string.Empty;

            return _accountDetails.Username;

        }

        /// <summary>
        /// Checks is the user registered and account
        /// </summary>
        /// <returns></returns>
        public bool HasBeenRegistered()
        {
            return _accountDetails != null && 
                   !string.IsNullOrWhiteSpace(_accountDetails.FullName) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.Username) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.Password) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.PIN);
        }

        public decimal AlterraBalance()
        {

            return AlterraDebit + _accountDetails.AlterraDebitPayed;
        }

        public bool IsAccountNameValid(string accountName)
        {
            return accountName.Equals(_accountDetails.Username, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resets all the fields in the account to default. [Warning] user will have to create a new account
        /// </summary>
        public void ResetAccount()
        {
            _accountDetails.ResetAccount();
            QuickLogger.ModMessage("Account has been reset");
        }

        public void Purge()
        {
            _accountDetails = null;
            _accountLoaded = false;
        }

        public bool HasAccountBeenSaved()
        {
            return _hasBeenSaved;
        }

        public void ResetHasBeenSaved()
        {
            _hasBeenSaved = false;
        }

        public AccountDetails GetAccount()
        {
            return _accountDetails;
        }

        public void Refund(TechType techType, bool checkIfKit = true)
        {
            AddFinances( StoreInventorySystem.GetPrice(techType, checkIfKit));
        }

        public bool HasPaymentBeenMadeToDebit()
        {
            return _accountDetails.AlterraDebitPayed > 0;
        }
    }
}
