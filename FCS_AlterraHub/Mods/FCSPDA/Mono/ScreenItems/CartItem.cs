using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Systems;
using UnityEngine;
using UnityEngine.UI;

#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems
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
            var name = Language.main.Get(TechType);
            itemName.text = ReturnAmount > 1 ? $"{name} x{ReturnAmount}" : Language.main.Get(TechType);

            var itemPrice = GameObjectHelpers.FindGameObject(gameObject, "ItemPrice").GetComponent<Text>();
            itemPrice.text = StoreInventorySystem.GetPrice(TechType).ToString("n0");


            var removeBTN = gameObject.GetComponentInChildren<Button>();
            removeBTN.onClick.AddListener((() => { onRemoveBTNClicked?.Invoke(this); }));
        }

        internal CartItemSaveData Save()
        {
            return new() { TechType = TechType, ReceiveTechType = ReceiveTechType, ReturnAmount = ReturnAmount};
        }

        public void Refund()
        {
            for (int i = 0; i < ReturnAmount; i++)
            {
                CardSystem.main.Refund(ReceiveTechType);
            }
        }
    }

    internal struct CartItemSaveData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReceiveTechType { get; set; }
        [JsonProperty] internal int ReturnAmount { get; set; }
        internal AlterraDronePortController Port { get; set; }
        public void Refund()
        {
            for (int i = 0; i < ReturnAmount; i++)
            {
                CardSystem.main.Refund(ReceiveTechType);
            }
        }
    }
}