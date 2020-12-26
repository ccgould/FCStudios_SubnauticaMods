using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_AlterraHub.Buildables
{
    internal partial class AlterraHub
    {
        private const string ModKey = "AHB";

        internal static Dictionary<string,string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_RequestNewCard","Request New Card."},
            { $"{ModKey}_CreateNewAccount","Create New Account."},
            { $"{ModKey}_CardNotInSystemSettingBalanceFormat", "This card number ({0}) wasn't found in the system. Setting balance to {1}." },
            { $"{ModKey}_ErrorHasOccured", "An error has occurred please let FCStudios in the Alterra Corp. know about this error. Thank you and sorry for the inconvenience." },
            { $"{ModKey}_NotEnoughMoneyOnAccount", "There is not enough money on card to perform this transaction." },
            { $"{ModKey}_AccountNotFoundFormat", "There is no account found for you please register an account in the Alterra Hub" },
            { $"{ModKey}_CardReader", "Card Reader" },
            { $"{ModKey}_AccountBalanceFormat", "Account Balance: {0}" },
            { $"{ModKey}_DebitBalanceFormat", "Debit Balance: {0}" },
            { $"{ModKey}_CheckOutTotalFormat", "Cart Total: {0}" },
            { $"{ModKey}_AccountNewBalanceFormat", "New Balance: {0}" },
            { $"{ModKey}_NoValidCardForPurchase", "Please select a valid card with enough finances to complete transaction." },
            { $"{ModKey}_NoItemsInCart", "No items in the card to purchase!" },
            { $"{ModKey}_CartMaxItems", "No more items can be added to the cart!. Please remove items to add more to the cart." },
            { $"{ModKey}_OreConsumerReceptacle", "Add ores to cash in." },
            { $"{ModKey}_AccountTotal", "Account Total" },
            { $"{ModKey}_TransferMoney", "Transfer Money" },
            { $"{ModKey}_RemoveAllCreditFromDevice", "Remove all credit to card before destroying." },
            { $"{ModKey}_Account", "ACCOUNT" },
            { $"{ModKey}_WelcomeBack", "WELCOME BACK!" },
            { $"{ModKey}_CardRequirementsMessage", "You need x1 {0} and x1 {1} to get a new card."},
            { $"{ModKey}_RegisterWelcomeMessage", "Create an account and get your free card!"},
            { $"{ModKey}_FullName", "Full Name"},
            { $"{ModKey}_UserName", "User Name"},
            { $"{ModKey}_Password", "Password"},
            { $"{ModKey}_PIN", "PIN (4 Digits)"},
            { $"{ModKey}_FullNamePlaceHolder", "Enter Full Name..."},
            { $"{ModKey}_UserNamePlaceHolder", "Enter username..."},
            { $"{ModKey}_PasswordPlaceHolder", "Enter password..."},
            { $"{ModKey}_PINPlaceHolder", "Enter pin number..."},
            { $"{ModKey}_AccountCreated", "Thank you for registering for an Alterra Bank Account your current balance is {0}"},
            { $"{ModKey}_NoCardInventory", "Error: No debit card detected. You are to far away or there is no card in your inventory"},
            { $"{ModKey}_AccountSetupError", "[Error] Please refill the following fields and press enter in the field to continue: {0}"},
            { $"{ModKey}_OreConsumerTimeLeftFormat", "Time left till {0} ore processed {1} | Pending {2}."},
            { $"{ModKey}_NoOresToProcess", "No ores to process."},
            { $"{ModKey}_PleaseBuildOnPlatForm", "Please Build on a platform to operate."},
            { $"{ModKey}_BaseOnOff","{0} is now {1}."},
            { $"{ModKey}_Online","online"},
            { $"{ModKey}_Offline","offline"},
            { $"{ModKey}_ChangeBaseName","Change Base Name"},

        };
        
        private void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(LanguageDictionary[key]) : string.Empty;
        }

        internal static string RequestNewCard()
        {
            return GetLanguage($"{ModKey}_RequestNewCard");
        }

        internal static string CreateNewAccount()
        {
            return GetLanguage($"{ModKey}_CreateNewAccount");
        }

        internal static string CardNotInSystemAddingBalanceFormat()
        {
            return GetLanguage($"{ModKey}_CardNotInSystemSettingBalanceFormat");
        }

        internal static string ErrorHasOccured()
        {
            return GetLanguage($"{ModKey}_ErrorHasOccured");
        }

        internal static string NotEnoughMoneyOnAccount()
        {
            return GetLanguage($"{ModKey}_NotEnoughMoneyOnAccount");
        }

        internal static string AccountNotFoundFormat()
        {
            return GetLanguage($"{ModKey}_AccountNotFoundFormat");
        }

        internal static string CardReader()
        {
            return GetLanguage($"{ModKey}_CardReader");
        }

        internal static string AccountBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_AccountBalanceFormat"), amount.ToString("n0"));
        }

        internal static string CheckOutTotalFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_CheckOutTotalFormat"), amount.ToString("n0"));
        }

        internal static string AccountNewBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_AccountNewBalanceFormat"), amount.ToString("n0"));
        }

        internal static string DebitBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_DebitBalanceFormat"), amount.ToString("n0"));
        }

        internal static string NoValidCardForPurchase()
        {
            return GetLanguage($"{ModKey}_NoValidCardForPurchase");
        }

        internal static string NoItemsInCart()
        {
            return GetLanguage($"{ModKey}_NoItemsInCart");
        }

        internal static string CannotAddAnyMoreItemsToCart()
        {
            return GetLanguage($"{ModKey}_CartMaxItems");
        }

        internal static string OreConsumerReceptacle()
        {
            return GetLanguage($"{ModKey}_OreConsumerReceptacle");
        }

        internal static string AccountTotal()
        {
            return GetLanguage($"{ModKey}_AccountTotal");
        }

        internal static string TransferMoney()
        {
            return GetLanguage($"{ModKey}_TransferMoney");
        }

        internal static string RemoveAllCreditFromDevice()
        {
            return GetLanguage($"{ModKey}_RemoveAllCreditFromDevice");
        }

        internal static string Account()
        {
            return GetLanguage($"{ModKey}_Account");
        }

        public static string WelcomeBack()
        {
            return GetLanguage($"{ModKey}_WelcomeBack");
        }

        public static string CardRequirementsMessageFormat()
        {
            return GetLanguage($"{ModKey}_CardRequirementsMessage");
        }

        public static string RegisterWelcomeMessage()
        {
            return GetLanguage($"{ModKey}_RegisterWelcomeMessage");
        }

        public static string FullName()
        {
            return GetLanguage($"{ModKey}_FullName");
        }

        public static string UserName()
        {
            return GetLanguage($"{ModKey}_UserName");
        }

        public static string Password()
        {
            return GetLanguage($"{ModKey}_Password");
        }
        
        public static string PIN()
        {
            return GetLanguage($"{ModKey}_PIN");
        }

        public static string FullNamePlaceholder()
        {
            return GetLanguage($"{ModKey}_FullNamePlaceHolder");
        }

        public static string UserNamePlaceholder()
        {
            return GetLanguage($"{ModKey}_UserNamePlaceHolder");
        }

        public static string PasswordPlaceholder()
        {
            return GetLanguage($"{ModKey}_PasswordPlaceHolder");
        }

        public static string PINPlaceholder()
        {
            return GetLanguage($"{ModKey}_PINPlaceHolder");
        }

        public static string AccountCreated(string amount)
        {
            return string.Format(GetLanguage($"{ModKey}_AccountCreated"), amount);
        }

        public static string CardNotDetected()
        {
            return GetLanguage($"{ModKey}_NoCardInventory");
        }

        public static string AccountSetupError(string value)
        {
            return string.Format(GetLanguage($"{ModKey}_AccountSetupError"),value);
        }

        public static string OreConsumerTimeLeftFormat(string ore, string timeLeft,string pending)
        {
            return string.Format(GetLanguage($"{ModKey}_OreConsumerTimeLeftFormat"), ore,timeLeft,pending);
        }

        public static string NoOresToProcess()
        {
            return GetLanguage($"{ModKey}_NoOresToProcess");
        }

        public static string PleaseBuildOnPlatForm()
        {
            return GetLanguage($"{ModKey}_PleaseBuildOnPlatForm");
        }

        public static string Offline()
        {
            return GetLanguage($"{ModKey}_Offline");
        }        
        
        public static string Online()
        {
            return GetLanguage($"{ModKey}_Online");
        }        
        
        public static string BaseOnOffMessage(string baseName,string status)
        {
            return string.Format(GetLanguage($"{ModKey}_BaseOnOff"),baseName,status);
        }


        public static string Submit()
        {
            return Language.main.Get("Submit");
        }

        public static string ChangeBaseName()
        {
            return GetLanguage($"{ModKey}_ChangeBaseName");
        }
    }
}
