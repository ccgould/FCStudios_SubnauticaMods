using System;
using System.Collections.Generic;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Systems
{
    /// <summary>
    /// A class that represents an account for the card system
    /// </summary>
    public class AccountDetails
    {
        [JsonProperty] public string Version { get; set; } = "2.0";
        [JsonProperty] public string FullName { get; set; }
        [JsonProperty] public string Username { get; set; }
        [JsonProperty] public string Password { get; set; }
        [JsonProperty] public string PIN { get; set; }
        public decimal Balance { internal get; set; }
        [JsonProperty] public string AccountBalance { get; set; }
        [JsonProperty] public decimal AlterraDebitPayed { get; set; }
        [JsonProperty] public decimal AccountBeforeDebit { get; set; }
        [JsonProperty] public Dictionary<string, string> KnownCardNumbers = new Dictionary<string, string>();

        public AccountDetails(AccountDetails accountDetails)
        {
            if (accountDetails == null) return;
            FullName = accountDetails.FullName;
            Username = accountDetails.Username;
            Password = accountDetails.Password;
            PIN = accountDetails.PIN;
            Balance = accountDetails.Balance;
            KnownCardNumbers = accountDetails.KnownCardNumbers;
            AccountBalance = accountDetails.AccountBalance;
        }

        public AccountDetails()
        {
            
        }
        public void ResetAccount()
        {
            FullName = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            PIN = string.Empty;
            Balance = 0;
            AlterraDebitPayed = 0;
            KnownCardNumbers.Clear();
        }

        public void GetDebit()
        {
            throw new NotImplementedException();
        }
    }
}