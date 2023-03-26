using System;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems
{
    internal class CartItem : MonoBehaviour
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }

        [JsonProperty]
        internal int ReturnAmount
        {
            get
            {
                if (_returnAmount <= 0)
                {
                    _returnAmount = 1;
                }
                return _returnAmount;
            }
            set => _returnAmount = value;
        }

        public ShipmentInfo ShipmentInfo { get; internal set; }

        internal Action<CartItem> onRemoveBTNClicked;
        private int _returnAmount;

        private void Start()
        {
            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            uGUI_Icon uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(TechType);

            var itemName = GameObjectHelpers.FindGameObject(gameObject, "ItemName").GetComponent<Text>();
            var name = Language.main.Get(TechType);
            itemName.text = ReturnAmount > 1 ? $"{name} x{ReturnAmount}" : Language.main.Get(TechType);

            var itemPrice = GameObjectHelpers.FindGameObject(gameObject, "ItemPrice").GetComponent<Text>();
            itemPrice.text = StoreInventoryService.GetPrice(TechType).ToString("n0");


            var removeBTN = gameObject.GetComponentInChildren<Button>();
            removeBTN.onClick.AddListener(() => { onRemoveBTNClicked?.Invoke(this); });
        }

        internal CartItemSaveData Save()
        {
            return new() { TechType = TechType, ReceiveTechType = ReceiveTechType, ReturnAmount = ReturnAmount, Sender = FCSAlterraHubGUISender.None };
        }
    }
}