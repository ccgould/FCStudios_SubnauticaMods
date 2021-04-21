﻿using System;
using System.Text;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Structs;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class StoreItem : MonoBehaviour
    {
        private decimal _price;
        private int _returnAmount;
        
        internal void CheckIsUnlocked()
        {
            if (TechType != TechType.None && !gameObject.activeSelf)
            {
                if (IsVisibleForced)
                {
                    gameObject.SetActive(true);
                    return;
                }
                
                bool isUnlocked = CrafterLogic.IsCraftRecipeUnlocked(TechType);
                QuickLogger.Debug($"Checking if {Language.main.Get(TechType)} is unlocked: {isUnlocked}");
                gameObject.SetActive(isUnlocked);
            }
        }

        public TechType TechType { get; set; }
        public bool IsVisibleForced { get; set; }

        internal void Initialize(FCSStoreEntry storeEntry, Action<TechType, TechType,int> callback, StoreCategory category, Func<bool> toolTipPermission)
        {
            IsVisibleForced = storeEntry.ForcedUnlock;

            _price = storeEntry.Cost;

            TechType = storeEntry.TechType;

            _returnAmount = storeEntry.ReturnAmount;
            
            var costObj = GameObjectHelpers.FindGameObject(gameObject, "ItemAmount").GetComponent<Text>();
            costObj.text = _price.ToString("n0");

            var addToCartBTN = gameObject.GetComponentInChildren<Button>();
            addToCartBTN.onClick.AddListener(() =>
            {
                callback?.Invoke(TechType, storeEntry.ReceiveTechType, _returnAmount);
            });

            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            
            var returnAmountObj = GameObjectHelpers.FindGameObject(gameObject, "ReturnAmount");
            var returnToolTip = returnAmountObj.AddComponent<FCSToolTip>();
            returnToolTip.Tooltip = $"{Buildables.AlterraHub.Bulk()}: {_returnAmount} {Language.main.Get(storeEntry.ReceiveTechType)}";
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
            toolTip.Tooltip = ToolTipFormat(TechType, LanguageHelpers.GetLanguage(TechType));
            toolTip.RequestPermission = toolTipPermission;
            var uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(TechType);

        }

        private string ToolTipFormat(TechType techType,string objectName)
        {
            StringBuilder sb = new StringBuilder();
            TooltipFactory.WriteTitle(sb, objectName);
            TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
            return sb.ToString();
        }


    }
}