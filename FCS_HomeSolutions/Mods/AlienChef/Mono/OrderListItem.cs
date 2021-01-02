using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    /// <summary>
    /// A class that defined a order in the order list it handles deleting and updating an order icon name and category.
    /// </summary>
    internal class OrderListItem : MonoBehaviour
    {
        internal void Initialize(TechType techType, int amount, string category, CookerItemController cookerItemDialog,
            OrderWindowDialog orderWindowDialog)
        {
            Dialog = cookerItemDialog;
            var deleteBTN = InterfaceHelpers.FindGameObject(gameObject, "CloseBTN").AddComponent<InterfaceButton>();
            deleteBTN.OnButtonClick += (s, o) =>
            {
                orderWindowDialog.RemoveItemFromList(this);
            };

            var icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();
            icon.sprite = SpriteManager.Get(techType);

            var itemNameAndAmount = InterfaceHelpers.FindGameObject(gameObject,"Name").GetComponent<Text>();
            itemNameAndAmount.text = $"{Language.main.Get(techType)} X{amount}";

            var foodCategory = InterfaceHelpers.FindGameObject(gameObject, "Type").GetComponent<Text>();
            foodCategory.text = category;
        }

        public CookerItemController Dialog { get; set; }
    }
}