using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services;

public static class LanguageService
{
    private const string ModKey = "AHB";

    /// <summary>
    /// Gets the requested string from the Language Handler
    /// </summary>
    /// <param name="key">The key of the language text</param>
    /// <returns><see cref="string"/> of the request text</returns>
    internal static string GetLanguage(string key)
    {
        var result = Language.main.Get(key);

        if (key.Equals(result))
        {
            var newKey = $"{ModKey}_{key}";
            return Language.main.Get(newKey);
        }

        return result; 
    }

    /// <summary>
    /// Gets the requested string from the Language Handler
    /// </summary>
    /// <param name="techType">The <see cref="TechType"/> of the object to find</param>
    /// <returns>see cref="string"/> of the request text</returns>
    internal static string GetLanguage(TechType techType)
    {
        return Language.main.Get(techType);
    }


    #region Method for mod use
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
        var message = string.Format(GetLanguage("ErrorHasOccurred"), errorCode);
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
        return string.Format(GetLanguage("AccountSetupError"), value);
    }

    public static string OreConsumerTimeLeftFormat(string ore, string timeLeft, string pending, int max)
    {
        return string.Format(GetLanguage("OreConsumerTimeLeftFormat"), ore, timeLeft, pending, max);
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

    public static string BaseOnOffMessage(string baseName, string status)
    {
        return string.Format(GetLanguage("BaseOnOff"), baseName, status);
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
        return string.Format(GetLanguage("OperationExists"), deviceId);
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
        return string.Format(GetLanguage("PDAButtonPressFormat"), Plugin.Configuration.PDAInfoKeyCode.ToString());
    }

    public static string IsDeviceOn(bool value)
    {
        return string.Format(GetLanguage("IsDeviceOn"), value);
    }

    public static string PowerPerMinute(float value)
    {
        return string.Format(GetLanguage("PowerPerMinute"), value.ToString("F2"));
    }

    public static string PowerPerMinuteDistance(float value)
    {
        return string.Format(GetLanguage("PowerPerMinuteDistance"), value.ToString("F2"));
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
        return string.Format(GetLanguage("CreditMessage"), amount, AccountService.main.GetAccountBalance());
    }

    public static string PressToInteractWith(KeyCode keyCode)
    {
        return string.Format(GetLanguage("PressToInteractWith"), keyCode);
    }

    internal static string DeviceNameStructure(FCSDevice controller)
    {
        if (controller is null) return string.Empty;
        string additional = string.Empty;   

        if (!string.IsNullOrWhiteSpace(controller.FriendlyName))
        {
            additional = $": {controller.FriendlyName}";
        }

        return string.Format(GetLanguage("DeviceNameStructure"), Language.main.Get(controller.GetTechType()), controller.UnitID, additional);
    }

    internal static string DeviceFriendlyNameStructure(FCSDevice controller)
    {
        if(controller is null || string.IsNullOrWhiteSpace(controller.FriendlyName)) return string.Empty;   
        return string.Format(GetLanguage("DeviceFriendlyNameStructure"), controller.FriendlyName);
    }

    public static string NotConnectedToBaseManager()
    {
        return GetLanguage("NotConnectedToBaseManager");
    }

    public static string StorageCountFormat(int amount, int maxStorage)
    {
        return string.Format(GetLanguage("StorageAmountFormat"), amount, maxStorage);
    }

    internal static string DebitCardDescription => "This card lets you access your Alterra Account. You must have this card with you to make Alterra Hub purchases.";
    #endregion
}
