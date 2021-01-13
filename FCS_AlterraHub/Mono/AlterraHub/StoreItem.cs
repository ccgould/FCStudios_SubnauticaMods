using System;
using System.Text;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Model;
using FCSCommon.Helpers;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class StoreItem : MonoBehaviour
    {
        private decimal _price;

        internal void Initialize(string objectName, TechType techType, TechType receiveTechType, decimal cost, Action<TechType, TechType> callback, StoreCategory category, Func<bool> toolTipPermission)
        {
            _price = cost;

            var costObj = GameObjectHelpers.FindGameObject(gameObject, "ItemAmount").GetComponent<Text>();
            costObj.text = cost.ToString("n0");

            var addToCartBTN = gameObject.GetComponentInChildren<Button>();
            addToCartBTN.onClick.AddListener(() =>
            {
                callback?.Invoke(techType,receiveTechType);
            });

            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            var toolTip = icon.AddComponent<FCSToolTip>();
            toolTip.Tooltip = ToolTipFormat(techType,objectName);
            toolTip.RequestPermission = toolTipPermission;
            var uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(techType);
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