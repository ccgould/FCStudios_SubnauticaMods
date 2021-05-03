using System;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Mono.AlterraHub
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

        internal Action<CartItem> onRemoveBTNClicked;
        private int _returnAmount;

        private void Start()
        {
            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            uGUI_Icon uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(TechType);

            var itemName = GameObjectHelpers.FindGameObject(gameObject, "ItemName").GetComponent<Text>();
            itemName.text = Language.main.Get(TechType);

            var itemPrice = GameObjectHelpers.FindGameObject(gameObject, "ItemPrice").GetComponent<Text>();
            itemPrice.text = StoreInventorySystem.GetPrice(TechType).ToString("n0");


            var removeBTN = gameObject.GetComponentInChildren<Button>();
            removeBTN.onClick.AddListener((() => { onRemoveBTNClicked?.Invoke(this); }));
        }

        internal CartItemSaveData Save()
        {
            return new CartItemSaveData { TechType = TechType, ReceiveTechType = ReceiveTechType, ReturnAmount = ReturnAmount};
        }
}

    internal struct CartItemSaveData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }
        [JsonProperty] internal int ReturnAmount { get; set; }
    }
}
