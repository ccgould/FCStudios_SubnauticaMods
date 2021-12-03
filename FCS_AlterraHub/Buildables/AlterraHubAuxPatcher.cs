﻿using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Buildables
{
    public partial class AlterraHub
    {
        private const string ModKey = "AHB";

        internal static Dictionary<string,string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_RequestNewCard","Request New Card."},
            { $"{ModKey}_CreateNewAccount","Create New Account."},
            { $"{ModKey}_CardNotInSystemSettingBalanceFormat", "This card number ({0}) wasn't found in the system. Setting balance to {1}." },
            { $"{ModKey}_ErrorHasOccured", "An error has occurred please let FCStudios in the Alterra Corp. know about this error. Thank you and sorry for the inconvenience." },
            { $"{ModKey}_NotEnoughMoneyOnAccount", "There is not enough money on card to perform this transaction." },
            { $"{ModKey}_AccountNotFoundFormat", "Unable to locate your AlterraHub Account. Please consult your FCStudios PDA." },
            { $"{ModKey}_CardReader", "Card Reader" },
            { $"{ModKey}_AccountBalanceFormat", "Account Balance: {0}" },
            { $"{ModKey}_DebtBalanceFormat", "Debt Balance: {0}" },
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
            { $"{ModKey}_NoCardInventory", "Error: No debit card detected. There is no card in your inventory. Your account card is needed for validation.Please get you card or request a new on from the account page."},
            { $"{ModKey}_AccountSetupError", "[Error] Please refill the following fields and press enter in the field to continue: {0}"},
            { $"{ModKey}_OreConsumerTimeLeftFormat", "Time left till {0} ore processed {1} | Pending {2} (Container Limit: {3})."},
            { $"{ModKey}_NoOresToProcess", "No ores to process."},
            { $"{ModKey}_PleaseBuildOnPlatForm", "Please Build on a platform to operate."},
            { $"{ModKey}_BaseOnOff","{0} is now {1}."},
            { $"{ModKey}_Online","online"},
            { $"{ModKey}_Offline","offline"},
            { $"{ModKey}_ChangeBaseName","Change Base Name"},
            { $"{ModKey}_ClickToSearchForItems","Click to search for items..."},
            { $"{ModKey}_BlackListItemFormat","{0} item is in the blacklist an will not be pulled from the vehicle."},
            { $"{ModKey}_NoVehicles","No vehicles are docked at this base."},
            { $"{ModKey}_NegativeNumbersNotAllowed","Negative numbers not allowed"},
            { $"{ModKey}_PressToTurnDeviceOnOff","Press {0} to turn device on/off"},
            { $"{ModKey}_DeviceOn","Device ON"},
            { $"{ModKey}_DeviceOff","Device OFF"},
            { $"{ModKey}_Bulk","Bulk"},
            { $"{ModKey}_FoodItemsNotAllowed","Cooked food items are not allowed in Data Storage Solutions."},
            { $"{ModKey}_PDAButtonPressFormat","Press ({0}) to open Alterra Hub PDA"},
            { $"{ModKey}_OperationExists","Similar operation already exists for device {0}"},
            { $"{ModKey}_DepotFull","Depot is full or doesn't have enough space for your purchase."},
            { $"{ModKey}_DepotNotFound","Depot cannot be located."},
            { $"{ModKey}_PurchaseSuccessful","Purchase was successful. Your items will be shipped to your selected base."},
            { $"{ModKey}_ErrorLoadingAccountDetails","There was an error loading your account details. Please contact FCStudio about this issue. Please provide your game log located in the Subnautica root folder. \nFileName:\"qmodmanager_log-Subnautica.txt\""},
            { $"{ModKey}_NoSpaceAccountCreation","To complete your account creation, you need at least one slot to receive your debit card. Please try again once one inventory slot is available in your inventory."},
            { $"{ModKey}_NoDestinationFound","Please select a destination for your items to be transferred to. You must have an AlterraHub Depot at a base."},
            { $"{ModKey}_IsDeviceOn","Device On: {0}"},
            { $"{ModKey}_PowerPerMinute","Power Per Minute: {0}"},
            { $"{ModKey}_Waiting","WAITING"},
            { $"{ModKey}_DoorInstructions","Put in the correct 4 digit pin to unlock the door"},
            { $"{ModKey}_CannotRemovePowercell","Cannot remove powercell from generator."},
            { $"{ModKey}_AntennaPinNeededMessage","Valid 4 digit pin required to activate antenna."},
            { $"{ModKey}_ProcessingItem","Processing Item"},
            { $"{ModKey}_OreValue","Ore Value"},
            { $"{ModKey}_TimeLeft","Time Left"},
            { $"{ModKey}_PleaseClearHands","Please clear hands (E) to use."},
            { $"{ModKey}_DebitHasBeenPaid","Your balance has been paid off."},
            { $"{ModKey}_MustBeBuiltOnBasePlatform","Must be built on Base Platform"},
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

        public static string ErrorHasOccured()
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

        internal static string DebtBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage($"{ModKey}_DebtBalanceFormat"), amount.ToString("n0"));
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

        public static string OreConsumerTimeLeftFormat(string ore, string timeLeft,string pending,int max)
        {
            return string.Format(GetLanguage($"{ModKey}_OreConsumerTimeLeftFormat"), ore,timeLeft,pending,max);
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

        public static string SearchForItemsMessage()
        {
            return GetLanguage($"{ModKey}_ClickToSearchForItems");
        }

        public static string BlackListFormat(string name)
        {
            return string.Format(GetLanguage($"{ModKey}_BlackListItemFormat"), name);
        }

        public static string NoVehiclesDocked()
        {
            return GetLanguage($"{ModKey}_NoVehicles");
        }

        public static string InventoryFull()
        {
            return Language.main.Get("InventoryFull");
        }

        public static string NegativeNumbersNotAllowed()
        {
            return GetLanguage($"{ModKey}_NegativeNumbersNotAllowed");
        }

        public static string NoSpaceAccountCreation()
        {
            return GetLanguage($"{ModKey}_NoSpaceAccountCreation");
        }

        public static string PressToTurnDeviceOnOff(string key)
        {
            return string.Format(GetLanguage($"{ModKey}_PressToTurnDeviceOnOff"), key);
        }

        public static string DeviceOn()
        {
            return GetLanguage($"{ModKey}_DeviceOn");
        }

        public static string DeviceOff()
        {
            return GetLanguage($"{ModKey}_DeviceOff");
        }

        public static string MissionButtonPressFormat(KeyCode key)
        {
            return string.Format(GetLanguage($"{ModKey}_PDAButtonPressFormat"), key.ToString());
        }

        public static string OperationExistsFormat(string deviceId)
        {
            return string.Format(GetLanguage($"{ModKey}_OperationExists"),deviceId);
        }

        public static string ErrorLoadingAccount()
        {
            return GetLanguage($"{ModKey}_ErrorLoadingAccountDetails");
        }

        public static string Bulk()
        {
            return GetLanguage($"{ModKey}_Bulk");
        }

        public static string FoodItemsNotAllowed()
        {
            return GetLanguage($"{ModKey}_FoodItemsNotAllowed");
        }

        public static string NoDestinationFound()
        {
            return GetLanguage($"{ModKey}_NoDestinationFound");
        }

        public static string NotEmpty()
        {
            return Language.main.Get("DeconstructNonEmptyStorageContainerError");
        }

        public static string PurchaseSuccessful()
        {
            return GetLanguage($"{ModKey}_PurchaseSuccessful");
        }

        public static string DepotFull()
        {
            return GetLanguage($"{ModKey}_DepotFull");
        }        
        
        public static string DepotNotFound()
        {
            return GetLanguage($"{ModKey}_DepotNotFound");
        }

        public static string ViewInPDA()
        {
            return string.Format(GetLanguage($"{ModKey}_PDAButtonPressFormat"), QPatch.Configuration.PDAInfoKeyCode.ToString());
        }

        public static string IsDeviceOn(bool value)
        {
            return string.Format(GetLanguage($"{ModKey}_IsDeviceOn"), value);
        }

        public static string PowerPerMinute(float value)
        {
            return string.Format(GetLanguage($"{ModKey}_PowerPerMinute"), value.ToString("F2"));
        }

        public static string Waiting()
        {
            return GetLanguage($"{ModKey}_Waiting");
        }
        public static string DoorInstructions()
        {
            return GetLanguage($"{ModKey}_DoorInstructions");
        }

        public static string CannotRemovePowercell()
        {
            return GetLanguage($"{ModKey}_CannotRemovePowercell");
        }

        public static string AntennaPinNeededMessage()
        {
            return GetLanguage($"{ModKey}_AntennaPinNeededMessage");
        }

        public static string ProcessingItem()
        {
            return GetLanguage($"{ModKey}_ProcessingItem");
        }

        public static string OreValue()
        {
            return GetLanguage($"{ModKey}_OreValue");
        }

        public static string TimeLeft()
        {
            return GetLanguage($"{ModKey}_TimeLeft");
        }

        public static string PleaseClearHands()
        {
            return GetLanguage($"{ModKey}_PleaseClearHands");
        }

        public static string DebitHasBeenPaid()
        {
            return GetLanguage($"{ModKey}_DebitHasBeenPaid");
        }

        public static string MustBeBuiltOnBasePlatform()
        {
            return GetLanguage($"{ModKey}_MustBeBuiltOnBasePlatform");
        }
    }
}
