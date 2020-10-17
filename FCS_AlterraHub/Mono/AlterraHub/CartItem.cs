using System;
using System.Globalization;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class CartItem : MonoBehaviour
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }
        internal Action<CartItem> onRemoveBTNClicked;

        private void Start()
        {
            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            uGUI_Icon uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(TechType);

            var itemName = GameObjectHelpers.FindGameObject(gameObject, "ItemName").GetComponent<Text>();
            itemName.text = Language.main.Get(TechType);

            var itemPrice = GameObjectHelpers.FindGameObject(gameObject, "ItemPrice").GetComponent<Text>();
            itemPrice.text = StoreInventorySystem.GetPrice(TechType).ToString(CultureInfo.InvariantCulture);

            var removeBTN = gameObject.GetComponentInChildren<Button>();
            removeBTN.onClick.AddListener((() => { onRemoveBTNClicked?.Invoke(this); }));
        }

        internal CartItemSaveData Save()
        {
            return new CartItemSaveData { TechType = TechType, ReceiveTechType = ReceiveTechType};
        }
}

    internal struct CartItemSaveData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }
    }
}
