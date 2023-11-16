using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services;

/// <summary>
/// This class is deals with adding credit, removing credit, create and deleting accounts
/// </summary>
internal class AccountService : MonoBehaviour
{
    private System.Random _random;

    private AccountDetails _accountDetails = new();
    public static AccountService main;
    private const decimal AlterraDebit = -1000000000000000000;
    public Action onBalanceUpdated;
    private bool _hasBeenSaved;
    private bool _accountLoaded;
    private float _time;

    public float targetTime = 0;
    private decimal _amount;




    private void Awake()
    {
        main = this;
    }


    private void Update()
    {
        if (_amount == 0)
        {
            return;
        }

        targetTime += Time.deltaTime;

        if (targetTime >= Plugin.Configuration.CreditMessageDelay)
        {
            timerEnded();
        }

    }

    void timerEnded()
    {
        if (_amount > 0)
        {
            QuickLogger.CreditMessage(LanguageService.CreditMessage(_amount));
        }
        _amount = 0;
        targetTime = 0;
    }

    private void AddCredit(decimal amount)
    {
        _amount += amount;
    }

    public bool IsLoaded { get; private set; }
    public AccountDetails AccountDetails { get => _accountDetails; set => _accountDetails = value; }

    /// <summary>
    /// Generates a new card number.
    /// </summary>
    /// <returns></returns>
    public void GenerateNewCard(string prefabId)
    {
        if (_random == null)
        {
            _random = new System.Random();
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
            AccountDetails.KnownCardNumbers.Add(prefabId, sb.ToString());
        }
    }

    /// <summary>
    /// Deletes card from the network
    /// </summary>
    /// <param name="cardNumber"></param>
    public void DeleteCard(string cardNumber)
    {
        AccountDetails.KnownCardNumbers?.Remove(cardNumber);
    }

    /// <summary>
    /// Adds credit to the account.
    /// </summary>
    /// <param name="amount">The amount of credit to add to the account.</param>
    /// <returns>Boolean on success</returns>
    public bool AddFinances(decimal amount, FCSAlterraHubGUISender sender = FCSAlterraHubGUISender.PDA)
    {
        try
        {
            AccountDetails.Balance += amount;
            if (Plugin.Configuration.CreditMessageDelay == 0f)
            {
                QuickLogger.CreditMessage(LanguageService.CreditMessage(amount));
            }
            else
            {
                main.AddCredit(amount);
            }
            onBalanceUpdated?.Invoke();
        }
        catch (Exception e)
        {
            QuickLogger.Error($"[AddFinances]: {e.Message}");
            QuickLogger.Error($"[AddFinances]: {e.StackTrace}");
            uGUI_MessageBoxHandler.Instance.ShowMessage(LanguageService.ErrorHasOccurred("0x0002"), sender);
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
        if (!UWEHelpers.RequiresPower()) return true;

        if (HasEnough(amount))
        {
            AccountDetails.Balance -= amount;
            onBalanceUpdated?.Invoke();
            return true;
        }

        //gui.ShowMessage(string.Format(AlterraHub.NotEnoughMoneyOnAccount(), 0));
        return false;
    }

    /// <summary>
    /// Gets the current balance of the account.
    /// </summary>
    /// <param name="cardNumber">The number of the card.</param>
    /// <returns></returns>
    public decimal GetAccountBalance()
    {
        if (!UWEHelpers.RequiresPower()) return 99999999999999;
        return AccountDetails?.Balance ?? 0;
    }

    /// <summary>
    /// Saves the current accounts information.
    /// </summary>
    /// <returns></returns>
    public AccountDetails SaveDetails()
    {
        _hasBeenSaved = true;
        QuickLogger.Debug($"Attempting to save account details {AccountDetails?.FullName}", true);
        if (AccountDetails != null)
        {
            AccountDetails.AccountBalance = EncodeDecode.Encrypt(AccountDetails.Balance.ToString(CultureInfo.InvariantCulture));
        }
        return AccountDetails /*_accountDetails != null ? _accountDetails : new AccountDetails()*/ ;
    }

    /// <summary>
    /// Loads saved account details
    /// </summary>
    /// <param name="accounts"></param>
    public IEnumerator Load(AccountDetails account)
    {
        IsLoaded = true;
        WaitScreen.ManualWaitItem creditMessage = null;
        if (_accountLoaded)
        {
            QuickLogger.Debug($"Account already loaded. Canceling load operation for {account.Username}.", true);
            yield break;
        }

        if (account != null)
        {
            AccountDetails = account;
            if (account.Version.Equals("2.0"))
            {
                var enUs = new CultureInfo("en-US");
                AccountDetails.Balance = Convert.ToDecimal(EncodeDecode.Decrypt(account.AccountBalance), enUs);
            }

            QuickLogger.Debug($"Alterra account loaded for player {account.Username}", true);
            QuickLogger.Debug($"{account.Username} | Funds: {AccountDetails.Balance}", true);
            QuickLogger.Debug($"{account.Username} | Debt Payed: {AccountDetails.AlterraDebitPayed}", true);

            _accountLoaded = true;

            creditMessage = WaitScreen.Add($"Alterra account loaded for player {account.Username}");
            QuickLogger.Info("Setting wait timer Credit", true);

            yield return WaitForRealSeconds(2, creditMessage);

            QuickLogger.Info("Removing Credit", true);
            WaitScreen.Remove(creditMessage);
        }

        yield break;
    }


    IEnumerator WaitForRealSeconds(float seconds, WaitScreen.ManualWaitItem item)
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < seconds)
        {
            var time = Time.realtimeSinceStartup - startTime;
            item.SetProgress((int)time, Convert.ToInt32(seconds));
            yield return null;
        }
    }

    /// <summary>
    /// Checks if the account has enough money to purchase the items.
    /// </summary>
    /// <param name="cost">Amount to check for</param>
    /// <returns></returns>
    public bool HasEnough(decimal cost)
    {
        QuickLogger.Debug("Checking is account has enough");
        if (!UWEHelpers.RequiresPower())
        {
            QuickLogger.Debug("Is in creative mode returning true since credit isnt used");
            return true;
        }
        QuickLogger.Debug($"Result: {AccountDetails.Balance >= cost}");
        return AccountDetails.Balance >= cost;
    }

    /// <summary>
    /// Gets the card number from the system based off the prefabID
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    public string GetCardNumber(string prefabId)
    {
        return AccountDetails.KnownCardNumbers[prefabId] ?? string.Empty;
    }

    /// <summary>
    /// Checks the system to see if there is a key entry for this prefab
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    public bool CardExistFromPrefabID(string prefabId)
    {
        return AccountDetails.KnownCardNumbers.ContainsKey(prefabId);
    }

    /// <summary>
    /// Creates an account to be used by the Alterrahub
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="pin"></param>
    public bool CreateUserAccount(string fullName, string userName, string password, string pin, decimal accountBalance = 0, FCSAlterraHubGUISender sender = FCSAlterraHubGUISender.PDA)
    {
        if (AccountDetails == null)
        {
            AccountDetails = new AccountDetails();
        }

        if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(userName) &&
            !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(pin))
        {
            AccountDetails.FullName = fullName;
            AccountDetails.Username = userName;
            AccountDetails.Password = EncodeDecode.CreateMD5(password);
            AccountDetails.PIN = EncodeDecode.CreateMD5(pin);
            AccountDetails.Balance = accountBalance;
            CalculateBalance();

            if (HasBeenRegistered())
            {
                uGUI_MessageBoxHandler.Instance.ShowMessage(LanguageService.AccountCreated(GetAccountBalance().ToString("N0")), sender);
                
                if (!PlayerInteractionHelper.HasCard())
                {
                    PlayerInteractionHelper.GivePlayerItem(DebitCardSpawnable.PatchedTechType);
                }

                //VoiceNotificationService.main.Play("PDA_Account_Created_key");
                return true;
            }

            AccountDetails = null;
            return false;
        }

        var sb = new StringBuilder();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            sb.Append(LanguageService.FullName());
            sb.Append(",");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            sb.Append(LanguageService.UserName());
            sb.Append(",");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            sb.Append(LanguageService.Password());
            sb.Append(",");
        }

        if (string.IsNullOrWhiteSpace(pin))
        {
            sb.Append(LanguageService.PIN());
            sb.Append(",");
        }

        uGUI_MessageBoxHandler.Instance.ShowMessage(LanguageService.AccountSetupError(sb.ToString()), sender);

        return false;
        //Main.MissionManagerGM.NotifyDeviceAction(Mod.AlterraHubTechType,Mod.DebitCardTechType,DeviceActionType.CREATEITEM);
    }

    public void CalculateBalance()
    {
        //if (Main.Configuration.GameModeOption == FCSGameMode.HardCore)
        //{
        //    if (!IsAlterraRepaid())
        //    {
        //        AccountDetails.AccountBeforeDebit = AccountDetails.Balance;
        //        AccountDetails.Balance += AlterraDebit;
        //    }

        //}
        //else if (Main.Configuration.GameModeOption == FCSGameMode.Normal)
        //{
        //    if (AccountDetails.Balance <= AlterraDebit)
        //    {
        //        AccountDetails.Balance += Mathf.Abs(AlterraDebit);
        //    }
        //}

        //onBalanceUpdated?.Invoke(AccountDetails.Balance);
    }

    public bool PayDebit(IFCSAlterraHubGUI gui, decimal amount)
    {
        QuickLogger.Debug($"Trying to add {amount} to {LanguageService.DebtBalanceFormat(AlterraBalance())}", true);

        // Check if debit needs to be paid.
        if (IsDebitPaid())
        {
            return false;
        }

        var abs = AmountNeededForDebt();

        var amountToTake = abs >= amount ? amount : abs;

        AccountDetails.AlterraDebitPayed += amountToTake;
        RemoveFinances(amountToTake);

        QuickLogger.CreditMessage($"Deducted {amount} from the account new balance is {AccountDetails.Balance}");
        return true;
    }

    public decimal AmountNeededForDebt()
    {
        var amountNeeded = AlterraDebit + AccountDetails.AlterraDebitPayed;

        var abs = Math.Abs(amountNeeded);
        return abs;
    }

    public bool IsDebitPaid()
    {
        return AlterraDebit + AccountDetails.AlterraDebitPayed >= 0;
    }

    public string GetUserName()
    {
        if (AccountDetails == null) return string.Empty;

        return AccountDetails.Username;

    }

    /// <summary>
    /// Checks is the user registered and account
    /// </summary>
    /// <returns></returns>
    public bool HasBeenRegistered()
    {
        return AccountDetails != null &&
               !string.IsNullOrWhiteSpace(AccountDetails.FullName) &&
               !string.IsNullOrWhiteSpace(AccountDetails.Username) &&
               !string.IsNullOrWhiteSpace(AccountDetails.Password) &&
               !string.IsNullOrWhiteSpace(AccountDetails.PIN);
    }

    public decimal AlterraBalance()
    {

        return AlterraDebit + AccountDetails.AlterraDebitPayed;
    }

    /// <summary>
    /// Resets all the fields in the account to default. [Warning] user will have to create a new account
    /// </summary>
    public void ResetAccount()
    {
        AccountDetails.ResetAccount();
        QuickLogger.ModMessage("Account has been reset");
    }

    public void Purge()
    {
        AccountDetails = null;
        _accountLoaded = false;
    }

    public void Refund(FCSAlterraHubGUISender gui, TechType techType, bool checkIfKit = true)
    {
        AddFinances(StoreInventoryService.GetPrice(techType, checkIfKit));
    }
}
