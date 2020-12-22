using FCSCommon.Components;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    /// <summary>
    /// Class the handles the order list button to show and hide the order list and update
    /// the list count number.
    /// </summary>
    internal class FoodQueueList : InterfaceButton
    {
        private OrderWindowDialog _dialog;
        private Text _amountLBL;

        internal void UpdateCount(int amount)
        {
            _amountLBL.text = amount.ToString();
        }

        internal void Initialize(OrderWindowDialog dialog)
        {
            dialog.ListButton = this;
            _dialog = dialog;
            _amountLBL = gameObject.GetComponentInChildren<Text>();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            _dialog.Show();
        }
    }
}