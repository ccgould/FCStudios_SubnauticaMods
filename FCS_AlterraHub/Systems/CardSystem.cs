using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;


namespace FCS_AlterraHub.Systems
{
    /// <summary>
    /// A class that represents an account for the card system
    /// </summary>
    internal class AccountDetails
    {

        [JsonProperty] internal string FullName { get; set; }
        [JsonProperty] internal string Username { get; set; }
        [JsonProperty] internal string Password { get; set; }
        [JsonProperty] internal string PIN { get; set; }
        [JsonProperty] internal decimal Balance { get; set; }
        [JsonProperty] internal static decimal AlterraDebitPayed { get; set; }
        [JsonProperty] internal decimal AccountBeforeDebit { get; set; }
        [JsonProperty] internal Dictionary<string, string> KnownCardNumbers = new Dictionary<string, string>();

        internal AccountDetails()
        {
            
        }

        internal AccountDetails(AccountDetails info)
        {
            FullName = info.FullName;
            Username = info.Username;
            Password = info.Password;
            PIN = info.PIN;
            Balance = info.Balance;
            AccountBeforeDebit = info.AccountBeforeDebit;
            KnownCardNumbers = new Dictionary<string, string>(KnownCardNumbers);
        }
    }
    internal class CardSystem
    {
        private Random _random;

        private AccountDetails _accountDetails = new AccountDetails();
        public static CardSystem main = new CardSystem();
        private const decimal AlterraDebit = -3000000;
        internal Action<decimal> onBalanceUpdated;

        /// <summary>
        /// Generates a new card number.
        /// </summary>
        /// <returns></returns>
        internal void GenerateNewCard(string prefabId)
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
        internal void DeleteCard(string cardNumber)
        {
            _accountDetails.KnownCardNumbers?.Remove(cardNumber);
        }

        /// <summary>
        /// Adds credit to the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card</param>
        /// <param name="amount">The amount of credit to add to the account.</param>
        /// <param name="callBack">Callback method to call in-case of error</param>
        /// <returns>Boolean on success</returns>
        internal bool AddFinances(decimal amount)
        {
            try
            {
                _accountDetails.Balance += amount;
                onBalanceUpdated?.Invoke(_accountDetails.Balance);
                QuickLogger.ModMessage($"Added {amount} to account new balance is {_accountDetails.Balance}");
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[AddFinances]: {e.Message}");
                MessageBoxHandler.main.Show(AlterraHub.ErrorHasOccured());
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
        internal bool RemoveFinances(decimal amount)
        {
            if (HasEnough(amount))
            {
                _accountDetails.Balance -= amount;
                onBalanceUpdated?.Invoke(_accountDetails.Balance);
                return true;
            }
            
            MessageBoxHandler.main.Show(string.Format(AlterraHub.NotEnoughMoneyOnAccount(), 0));
            return false;
        }

        /// <summary>
        /// Gets the current balance of the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card.</param>
        /// <returns></returns>
        internal decimal GetAccountBalance()
        {
            return _accountDetails?.Balance ?? 0;
        }
        
        /// <summary>
        /// Saves the current accounts information.
        /// </summary>
        /// <returns></returns>
        internal AccountDetails Save()
        {
            return _accountDetails;
        }

        /// <summary>
        /// Loads saved account details
        /// </summary>
        /// <param name="accounts"></param>
        internal void Load(AccountDetails account)
        {
            if (account != null)
            {
                _accountDetails = account;
                QuickLogger.Info($"Alterra account loaded for player {account.Username}",true);
            }
        }

        /// <summary>
        /// Checks if the account has enough money to purchase the items.
        /// </summary>
        /// <param name="cost">Amount to check for</param>
        /// <returns></returns>
        internal bool HasEnough(decimal cost)
        {
            return _accountDetails.Balance >= cost;
        }

        /// <summary>
        /// Gets the card number from the system based off the prefabID
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal string GetCardNumber(string prefabId)
        {
            return _accountDetails.KnownCardNumbers[prefabId] ?? string.Empty;
        }
        
        /// <summary>
        /// Checks the system to see if there is a key entry for this prefab
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal bool CardExistFromPrefabID(string prefabId)
        {
            return _accountDetails.KnownCardNumbers.ContainsKey(prefabId);
        }

        /// <summary>
        /// Checks the system to see if there is a key entry for this card number
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal bool CardExist(string cardNumber)
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
        internal void CreateUserAccount(string fullName, string userName, string password, string pin)
        {
            if (_accountDetails == null)
            {
                _accountDetails = new AccountDetails();
            }
            
            _accountDetails.FullName = fullName;
            _accountDetails.Username = userName;
            _accountDetails.Password = password;
            _accountDetails.PIN = pin;

            CalculateBalance();

            if (HasBeenRegistered())
            {
                MessageBoxHandler.main.Show(AlterraHub.AccountCreated());
                var newCard = Mod.DebitCardTechType.ToInventoryItem();
                GenerateNewCard(newCard.item.gameObject.GetComponent<PrefabIdentifier>().Id);
                PlayerInteractionHelper.GivePlayerItem(newCard);
            }
            else
            {
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
                
                MessageBoxHandler.main.Show(AlterraHub.AccountSetupError(sb.ToString()));

            }
        }


        internal void CalculateBalance()
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
            return AccountDetails.AlterraDebitPayed >= AlterraDebit;
        }

        internal void PayDebit(decimal amount)
        {
            AccountDetails.AlterraDebitPayed += amount;
            RemoveFinances(amount);
        }


        internal string GetUserName()
        {
            if (_accountDetails == null) return string.Empty;

            return _accountDetails.Username;

        }

        /// <summary>
        /// Checks is the user registered and account
        /// </summary>
        /// <returns></returns>
        internal bool HasBeenRegistered()
        {
            return _accountDetails != null && 
                   !string.IsNullOrWhiteSpace(_accountDetails.FullName) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.Username) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.Password) && 
                   !string.IsNullOrWhiteSpace(_accountDetails.PIN);
        }

        internal decimal AlterraBalance()
        {
            return AlterraDebit - AccountDetails.AlterraDebitPayed;
        }

        internal bool IsAccountNameValid(string accountName)
        {
            return accountName.Equals(_accountDetails.Username, StringComparison.OrdinalIgnoreCase);
        }

        public AccountDetails SaveDetails()
        {
            return new AccountDetails(_accountDetails);
        }
    }
}
