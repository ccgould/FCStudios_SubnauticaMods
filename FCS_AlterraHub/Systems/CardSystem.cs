using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
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
        [JsonProperty] internal float Balance { get; set; }
        [JsonProperty] internal static float AlterraDebitPayed { get; set; }
        [JsonProperty] internal float AccountBeforeDebit { get; set; }
        [JsonProperty] internal Dictionary<string, string> KnownCardNumbers = new Dictionary<string, string>();
    }
    internal class CardSystem
    {
        private Random _random;

        internal AccountDetails AccountDetails = new AccountDetails();
        public static CardSystem main = new CardSystem();
        private const float AlterraDebit = -3000000f;
        internal Action<float> onBalanceUpdated;

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
            
            if (AccountDetails == null)
            {
                AccountDetails = new AccountDetails();
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

            if (!AccountDetails.KnownCardNumbers.ContainsValue(sb.ToString()))
            {
                AccountDetails.KnownCardNumbers.Add(prefabId,sb.ToString());
            }
        }

        /// <summary>
        /// Deletes card from the network
        /// </summary>
        /// <param name="cardNumber"></param>
        internal void DeleteCard(string cardNumber)
        {
            AccountDetails.KnownCardNumbers?.Remove(cardNumber);
        }

        /// <summary>
        /// Adds credit to the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card</param>
        /// <param name="amount">The amount of credit to add to the account.</param>
        /// <param name="callBack">Callback method to call in-case of error</param>
        /// <returns>Boolean on success</returns>
        internal bool AddFinances(string cardNumber, float amount)
        {
            try
            {
                if (AccountDetails.KnownCardNumbers.ContainsValue(cardNumber))
                {
                    AccountDetails.Balance += amount;
                    onBalanceUpdated?.Invoke(AccountDetails.Balance);
                    return true;
                }

                MessageBoxHandler.main.Show(string.Format(AlterraHub.CardNotInSystemAddingBalanceFormat(), cardNumber, amount));
                return false;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[AddFinances]: {e.Message}");
                MessageBoxHandler.main.Show(AlterraHub.ErrorHasOccured());
                return false;
            }
        }

        /// <summary>
        /// Removes credit from the account.
        /// </summary>
        /// <param name="cardNumber">The number of the card</param>
        /// <param name="amount">The amount of credit to add to the account.</param>
        /// <param name="callBack">Callback method to call in-case of error</param>
        /// <returns>Boolean on success</returns>
        internal bool RemoveFinances(float amount)
        {
            if (HasEnough(amount))
            {
                AccountDetails.Balance -= amount;
                onBalanceUpdated?.Invoke(AccountDetails.Balance);
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
        internal float GetAccountBalance(string cardNumber)
        {
            if (AccountDetails.KnownCardNumbers.ContainsValue(cardNumber)) return AccountDetails.Balance;

            MessageBoxHandler.main.Show(string.Format(AlterraHub.CardNotInSystemAddingBalanceFormat(), cardNumber, 0));
            return 0;

        }
        
        /// <summary>
        /// Saves the current accounts information.
        /// </summary>
        /// <returns></returns>
        internal AccountDetails Save()
        {
            return AccountDetails;
        }

        /// <summary>
        /// Loads saved account details
        /// </summary>
        /// <param name="accounts"></param>
        internal void Load(AccountDetails account)
        {
            if (account != null)
            {
                AccountDetails = account;
                QuickLogger.Info($"Alterra account loaded for player {account.Username}",true);
            }
        }

        /// <summary>
        /// Checks if the account has enough money to purchase the items.
        /// </summary>
        /// <param name="cost">Amount to check for</param>
        /// <returns></returns>
        internal bool HasEnough(float cost)
        {
            return AccountDetails.Balance >= cost;
        }

        /// <summary>
        /// Gets the card number from the system based off the prefabID
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal string GetCardNumber(string prefabId)
        {
            return AccountDetails.KnownCardNumbers[prefabId] ?? string.Empty;
        }
        
        /// <summary>
        /// Checks the system to see if there is a key entry for this prefab
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal bool CardExistFromPrefabID(string prefabId)
        {
            return AccountDetails.KnownCardNumbers.ContainsKey(prefabId);
        }

        /// <summary>
        /// Checks the system to see if there is a key entry for this card number
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        internal bool CardExist(string cardNumber)
        {
            return AccountDetails.KnownCardNumbers.ContainsValue(cardNumber);
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
            if (AccountDetails == null)
            {
                AccountDetails = new AccountDetails();
            }
            
            AccountDetails.FullName = fullName;
            AccountDetails.Username = userName;
            AccountDetails.Password = password;
            AccountDetails.PIN = pin;

            CalculateBalance();

            MessageBoxHandler.main.Show(AlterraHub.AccountCreated());

            var newCard = Mod.DebitCardTechType.ToInventoryItem();
            GenerateNewCard(newCard.item.gameObject.GetComponent<PrefabIdentifier>().Id);
            PlayerInteractionHelper.GivePlayerItem(newCard);

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

        internal void PayDebit(float amount)
        {
            AccountDetails.AlterraDebitPayed += amount;
            RemoveFinances(amount);
        }


        internal string GetUserName()
        {
            if (AccountDetails == null) return string.Empty;

            return AccountDetails.Username;

        }

        /// <summary>
        /// Checks is the user registered and account
        /// </summary>
        /// <returns></returns>
        internal bool HasBeenRegistered()
        {
            return AccountDetails != null && 
                   !string.IsNullOrWhiteSpace(AccountDetails.FullName) && 
                   !string.IsNullOrWhiteSpace(AccountDetails.Username) && 
                   !string.IsNullOrWhiteSpace(AccountDetails.Password) && 
                   !string.IsNullOrWhiteSpace(AccountDetails.PIN);
        }

        internal float AlterraBalance()
        {
            return Mathf.Abs(AlterraDebit) - AccountDetails.AlterraDebitPayed;
        }
    }
}
