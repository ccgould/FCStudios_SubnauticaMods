using System.Collections.Generic;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_AlterraHub.Buildables
{
    public partial class AlterraHub
    {
        private const string ModKey = "AHB";

        internal static Dictionary<string,string> LanguageDictionary = new()
        {
            { $"{ModKey}_RequestNewCard","Request New Card."},
            { $"{ModKey}_CreateNewAccount","Create New Account."},
            { $"{ModKey}_CardNotInSystemSettingBalanceFormat", "This card number ({0}) wasn't found in the system. Setting balance to {1}." },
            { $"{ModKey}_ErrorHasOccurred", "An error has occurred please let FCStudios in the Alterra Corp. know about this error. Thank you and sorry for the inconvenience. Error Code: {0}"},
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
            { $"{ModKey}_AccountCreated", "Thank you for registering for a temporary Alterra Account. Your current account balance is {0}"},
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
            { $"{ModKey}_PowerPerMinute", "Distance Related Power Loss: {0} epm" },
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
            { $"{ModKey}_HolsterPaintTool","To change color and pattern/image use the Paint Tool."},
            { $"{ModKey}_Error404", "Connection to station failed! Please visit the AlterraHub Fabrication Facility for assistance." },
            { $"{ModKey}_OrderBeingShipped", "Your order is now being shipped to base" },
            { $"{ModKey}_OrderHasBeenShipped", "Your order has been shipped to base" },
            { $"{ModKey}_NoPowerEmergencyMode", "NO POWER SYSTEM IN EMERGENCY MODE" },
            { $"{ModKey}_BaseIDFormat", "Base ID: {0}" },
            { $"{ModKey}_CreditMessage", "Added {0} to account new balance is {1}" },
            { $"{ModKey}_PressToInteractWith", "Press {0} to interact with {1}" }
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            var newKey = $"{ModKey}_{key}";
            return LanguageDictionary.ContainsKey(newKey) ? Language.main.Get(newKey) : string.Empty;
        }

        internal static string RequestNewCard()
        {
            return GetLanguage("RequestNewCard");
        }

        internal static string CreateNewAccount()
        {
            return GetLanguage("CreateNewAccount");
        }

        internal static string CardNotInSystemAddingBalanceFormat()
        {
            return GetLanguage("CardNotInSystemSettingBalanceFormat");
        }

        public static string ErrorHasOccurred(string errorCode)
        {
            var message =  string.Format(GetLanguage("ErrorHasOccurred"),errorCode);
            QuickLogger.Error($"Error Occurred: {errorCode}");
            return message;
        }

        internal static string NotEnoughMoneyOnAccount()
        {
            return GetLanguage("NotEnoughMoneyOnAccount");
        }

        internal static string AccountNotFoundFormat()
        {
            return GetLanguage("AccountNotFoundFormat");
        }

        internal static string CardReader()
        {
            return GetLanguage("CardReader");
        }

        internal static string AccountBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage("AccountBalanceFormat"), amount.ToString("n0"));
        }

        internal static string CheckOutTotalFormat(decimal amount)
        {
            return string.Format(GetLanguage("CheckOutTotalFormat"), amount.ToString("n0"));
        }

        internal static string AccountNewBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage("AccountNewBalanceFormat"), amount.ToString("n0"));
        }

        internal static string DebtBalanceFormat(decimal amount)
        {
            return string.Format(GetLanguage("DebtBalanceFormat"), amount.ToString("n0"));
        }

        internal static string NoValidCardForPurchase()
        {
            return GetLanguage("NoValidCardForPurchase");
        }

        internal static string NoItemsInCart()
        {
            return GetLanguage("NoItemsInCart");
        }

        internal static string CannotAddAnyMoreItemsToCart()
        {
            return GetLanguage("CartMaxItems");
        }

        internal static string OreConsumerReceptacle()
        {
            return GetLanguage("OreConsumerReceptacle");
        }

        internal static string AccountTotal()
        {
            return GetLanguage("AccountTotal");
        }

        internal static string TransferMoney()
        {
            return GetLanguage("TransferMoney");
        }

        internal static string RemoveAllCreditFromDevice()
        {
            return GetLanguage("RemoveAllCreditFromDevice");
        }

        internal static string Account()
        {
            return GetLanguage("Account");
        }

        public static string WelcomeBack()
        {
            return GetLanguage("WelcomeBack");
        }

        public static string CardRequirementsMessageFormat()
        {
            return GetLanguage("CardRequirementsMessage");
        }

        public static string RegisterWelcomeMessage()
        {
            return GetLanguage("RegisterWelcomeMessage");
        }

        public static string FullName()
        {
            return GetLanguage("FullName");
        }

        public static string UserName()
        {
            return GetLanguage("UserName");
        }

        public static string Password()
        {
            return GetLanguage("Password");
        }
        
        public static string PIN()
        {
            return GetLanguage("PIN");
        }

        public static string FullNamePlaceholder()
        {
            return GetLanguage("FullNamePlaceHolder");
        }

        public static string UserNamePlaceholder()
        {
            return GetLanguage("UserNamePlaceHolder");
        }

        public static string PasswordPlaceholder()
        {
            return GetLanguage("PasswordPlaceHolder");
        }

        public static string PINPlaceholder()
        {
            return GetLanguage("PINPlaceHolder");
        }

        public static string AccountCreated(string amount)
        {
            return string.Format(GetLanguage("AccountCreated"), amount);
        }

        public static string CardNotDetected()
        {
            return GetLanguage("NoCardInventory");
        }

        public static string AccountSetupError(string value)
        {
            return string.Format(GetLanguage("AccountSetupError"),value);
        }

        public static string OreConsumerTimeLeftFormat(string ore, string timeLeft,string pending,int max)
        {
            return string.Format(GetLanguage("OreConsumerTimeLeftFormat"), ore,timeLeft,pending,max);
        }

        public static string NoOresToProcess()
        {
            return GetLanguage("NoOresToProcess");
        }

        public static string PleaseBuildOnPlatForm()
        {
            return GetLanguage("PleaseBuildOnPlatForm");
        }

        public static string Offline()
        {
            return GetLanguage("Offline");
        }        
        
        public static string Online()
        {
            return GetLanguage("Online");
        }        
        
        public static string BaseOnOffMessage(string baseName,string status)
        {
            return string.Format(GetLanguage("BaseOnOff"),baseName,status);
        }

        public static string Submit()
        {
            return Language.main.Get("Submit");
        }

        public static string ChangeBaseName()
        {
            return GetLanguage("ChangeBaseName");
        }

        public static string SearchForItemsMessage()
        {
            return GetLanguage("ClickToSearchForItems");
        }

        public static string BlackListFormat(string name)
        {
            return string.Format(GetLanguage("BlackListItemFormat"), name);
        }

        public static string NoVehiclesDocked()
        {
            return GetLanguage("NoVehicles");
        }

        public static string InventoryFull()
        {
            return Language.main.Get("InventoryFull");
        }

        public static string NegativeNumbersNotAllowed()
        {
            return GetLanguage("NegativeNumbersNotAllowed");
        }

        public static string NoSpaceAccountCreation()
        {
            return GetLanguage("NoSpaceAccountCreation");
        }

        public static string PressToTurnDeviceOnOff(string key)
        {
            return string.Format(GetLanguage("PressToTurnDeviceOnOff"), key);
        }

        public static string DeviceOn()
        {
            return GetLanguage("DeviceOn");
        }

        public static string DeviceOff()
        {
            return GetLanguage("DeviceOff");
        }

        public static string MissionButtonPressFormat(KeyCode key)
        {
            return string.Format(GetLanguage("PDAButtonPressFormat"), key.ToString());
        }

        public static string OperationExistsFormat(string deviceId)
        {
            return string.Format(GetLanguage("OperationExists"),deviceId);
        }

        public static string ErrorLoadingAccount()
        {
            return GetLanguage("ErrorLoadingAccountDetails");
        }

        public static string Bulk()
        {
            return GetLanguage("Bulk");
        }

        public static string FoodItemsNotAllowed()
        {
            return GetLanguage("FoodItemsNotAllowed");
        }

        public static string NoDestinationFound()
        {
            return GetLanguage("NoDestinationFound");
        }

        public static string NotEmpty()
        {
            return Language.main.Get("DeconstructNonEmptyStorageContainerError");
        }

        public static string PurchaseSuccessful()
        {
            return GetLanguage("PurchaseSuccessful");
        }

        public static string DepotFull()
        {
            return GetLanguage("DepotFull");
        }        
        
        public static string DepotNotFound()
        {
            return GetLanguage("DepotNotFound");
        }

        public static string ViewInPDA()
        {
            return string.Format(GetLanguage("PDAButtonPressFormat"), Main.Configuration.PDAInfoKeyCode.ToString());
        }

        public static string IsDeviceOn(bool value)
        {
            return string.Format(GetLanguage("IsDeviceOn"), value);
        }

        public static string PowerPerMinute(float value)
        {
            return string.Format(GetLanguage("PowerPerMinute"), value.ToString("F2"));
        }

        public static string Waiting()
        {
            return GetLanguage("Waiting");
        }
        public static string DoorInstructions()
        {
            return GetLanguage("DoorInstructions");
        }

        public static string CannotRemovePowercell()
        {
            return GetLanguage("CannotRemovePowercell");
        }

        public static string AntennaPinNeededMessage()
        {
            return GetLanguage("AntennaPinNeededMessage");
        }

        public static string ProcessingItem()
        {
            return GetLanguage("ProcessingItem");
        }

        public static string OreValue()
        {
            return GetLanguage("OreValue");
        }

        public static string TimeLeft()
        {
            return GetLanguage("TimeLeft");
        }

        public static string PleaseClearHands()
        {
            return GetLanguage("PleaseClearHands");
        }



        public static string DebitHasBeenPaid()
        {
            return GetLanguage("DebitHasBeenPaid");
        }

        public static string MustBeBuiltOnBasePlatform()
        {
            return GetLanguage("MustBeBuiltOnBasePlatform");
        }

        public static string HolsterPaintTool()
        {
            return GetLanguage("HolsterPaintTool");
        }

        public static string Error404()
        {
            return GetLanguage("Error404");
        }

        public static string OrderBeingShipped()
        {
            return GetLanguage("OrderBeingShipped");
        }

        public static string OrderHasBeenShipped()
        {
            return GetLanguage("OrderHasBeenShipped");
        }

        public static string NoPowerEmergencyMode()
        {
            return GetLanguage("NoPowerEmergencyMode");
        }

        public static string BaseIDFormat(string baseId)
        {
            return string.Format(GetLanguage("BaseIDFormat"), baseId);
        }

        public static string CreditMessage(decimal amount)
        {
            return string.Format(GetLanguage("CreditMessage"), amount, CardSystem.main.GetAccountBalance());
        }

        public static string PressToInteractWith(KeyCode keyCode, string unitName)
        {
            return string.Format(GetLanguage("PressToInteractWith"), keyCode, unitName);
        }
    }
}
