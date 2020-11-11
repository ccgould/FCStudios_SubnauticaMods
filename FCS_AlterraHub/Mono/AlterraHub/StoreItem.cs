using System;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class StoreItem : MonoBehaviour
    {
        private decimal _price;

        internal void Initialize(string objectName,TechType techType, TechType receiveTechType, decimal cost, Action<TechType,TechType> callback,StoreCategory category)
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
            var uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(techType);
        }
    }
}