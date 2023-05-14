using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;

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

    [SerializeField]
    private uGUI_Icon _uGUIIcon;
    [SerializeField]
    private Text _itemName;
    [SerializeField]
    private Text _itemPrice;

    private void Start()
    {
        _uGUIIcon.sprite = SpriteManager.Get(TechType);

        var name = Language.main.Get(TechType);
        _itemName.text = ReturnAmount > 1 ? $"{name} x{ReturnAmount}" : Language.main.Get(TechType);

        _itemPrice = GameObjectHelpers.FindGameObject(gameObject, "ItemPrice").GetComponent<Text>();
        _itemPrice.text = StoreInventoryService.GetPrice(TechType).ToString("n0");
    }

    public void OnRemoveBTNClicked()
    {
        onRemoveBTNClicked?.Invoke(this);
    }

    internal CartItemSaveData Save()
    {
        return new() { TechType = TechType, ReceiveTechType = ReceiveTechType, ReturnAmount = ReturnAmount, Sender = FCSAlterraHubGUISender.None };
    }
}