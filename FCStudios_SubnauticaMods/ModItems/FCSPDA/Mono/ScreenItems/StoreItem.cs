using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.Mono.Tools;
using FCSCommon.Utilities;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;

internal class StoreItem : MonoBehaviour
{
    private decimal _price;
    private int _returnAmount;
    private Action<TechType, TechType, int> _callBack;
    private FCSStoreEntry _storeEntry;
    private bool _forceUnlock;

    [SerializeField]
    private Text _itemAmount;

    [SerializeField]
    private uGUI_Icon _icon;
    [SerializeField]
    private FCSToolTip _returnToolTip;
    [SerializeField]
    private Text _returnAmountText;



    internal bool CheckIsUnlocked()
    {
        if (TechType == TechType.None) return false;
        bool isUnlocked = CrafterLogic.IsCraftRecipeUnlocked(TechType);
        QuickLogger.Debug($"Checking if {Language.main.Get(TechType)} is unlocked: {isUnlocked}");
        return isUnlocked;
    }

    public TechType TechType { get; set; }
    public bool IsVisibleForced { get; set; }

    internal void Initialize(FCSStoreEntry storeEntry, Action<TechType, TechType, int> callback, StoreCategory category, Func<bool> toolTipPermission)
    {
        _callBack = callback;
        _storeEntry = storeEntry;
        _forceUnlock = storeEntry.ForcedUnlock;
        IsVisibleForced = storeEntry.ForcedUnlock;
        _price = storeEntry.Cost;

        TechType = storeEntry.TechType;

        _returnAmount = storeEntry.ReturnAmount;

        _itemAmount.text = _price.ToString("n0");

        _returnToolTip.Tooltip = $"{LanguageService.Bulk()}: {_returnAmount} {Language.main.Get(storeEntry.ReceiveTechType)}";
        _returnToolTip.RequestPermission = toolTipPermission;


        if (_returnAmount > 1)
        {
            _returnToolTip.gameObject.SetActive(true);
        }

        _returnAmountText.text = $"x{_returnAmount}";

        var toolTip = _icon.GetComponent<FCSToolTip>();
        toolTip.Tooltip = ToolTipFormat(TechType, LanguageService.GetLanguage(TechType));
        toolTip.RequestPermission = toolTipPermission;

        _icon.sprite = SpriteManager.Get(TechType);

    }

    public void OnAddToCartButtonClicked()
    {
        _callBack?.Invoke(TechType, _storeEntry.ReceiveTechType, _returnAmount);
    }

    private string ToolTipFormat(TechType techType, string objectName)
    {
        StringBuilder sb = new StringBuilder();
        TooltipFactory.WriteTitle(sb, objectName);
        TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
        return sb.ToString();
    }

    public override string ToString()
    {
        return $"Store Item: {Language.main.Get(TechType)}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        if (CheckIsUnlocked() || _forceUnlock || FCSModsAPI.PublicAPI.IsInOreBuildMode())
        {
            gameObject.SetActive(true);
        }
        else
        {
            Hide();
        }
    }
}
