using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.Mono.Tools;
using FCSCommon.Utilities;
using SMLHelper.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems
{
    internal class StoreItem : MonoBehaviour
    {
        private decimal _price;
        private int _returnAmount;
        private bool _forceUnlock;

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
            _forceUnlock = storeEntry.ForcedUnlock;
            IsVisibleForced = storeEntry.ForcedUnlock;
            _price = storeEntry.Cost;

            TechType = storeEntry.TechType;

            _returnAmount = storeEntry.ReturnAmount;

            var costObj = GameObjectHelpers.FindGameObject(gameObject, "ItemAmount").GetComponent<Text>();
            costObj.text = _price.ToString("n0");

            var addToCartBTN = gameObject.GetComponentInChildren<Button>();
            addToCartBTN.onClick.AddListener(() =>
            {
                NotificationService.CSVLog(addToCartBTN);
                callback?.Invoke(TechType, storeEntry.ReceiveTechType, _returnAmount);
            });

            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");

            var returnAmountObj = GameObjectHelpers.FindGameObject(gameObject, "ReturnAmount");
            var returnToolTip = returnAmountObj.AddComponent<FCSToolTip>();
            returnToolTip.Tooltip = $"{LanguageService.Bulk()}: {_returnAmount} {Language.main.Get(storeEntry.ReceiveTechType)}";
            returnToolTip.RequestPermission = toolTipPermission;


            if (_returnAmount > 1)
            {
                returnAmountObj.SetActive(true);
            }

            var returnAmountText = returnAmountObj.GetComponentInChildren<Text>();
            if (returnAmountText != null)
            {
                returnAmountText.text = $"x{_returnAmount}";
            }

            var toolTip = icon.AddComponent<FCSToolTip>();
            toolTip.Tooltip = ToolTipFormat(TechType, LanguageService.GetLanguage(TechType));
            toolTip.RequestPermission = toolTipPermission;

            var uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(TechType);

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
}
