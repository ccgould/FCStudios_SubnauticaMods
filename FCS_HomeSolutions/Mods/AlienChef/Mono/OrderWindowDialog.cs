using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    /// <summary>
    /// A class that handles the order list functions adding and remove of <see cref="OrderListItem"/>
    /// showing and hiding of the order list.
    /// </summary>
    internal class OrderWindowDialog : MonoBehaviour
    {
        private GameObject _grid;
        private List<OrderListItem> _orderQueue = new List<OrderListItem>();
        private CookerItemController _dialog;
        public FoodQueueList ListButton { get; set; }

        internal void Initialize(AlienChefController mono)
        {
            _grid = gameObject.FindChild("Grid");
            
            var deleteBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").EnsureComponent<InterfaceButton>();
            deleteBTN.OnButtonClick += (s, o) =>
            {
                Hide();
            }; 

            var nameLbl = GameObjectHelpers.FindGameObject(gameObject, "Name").GetComponent<Text>();
            nameLbl.text = AuxPatchers.Order();

            var cookBTN = GameObjectHelpers.FindGameObject(gameObject, "Button").GetComponent<Button>();
            cookBTN.onClick.AddListener(() =>
            {
                for (int i = _orderQueue.Count - 1; i > -1; i--)
                {
                    RemoveItemFromList(_orderQueue[i]);
                }
                ListButton.UpdateCount(_orderQueue.Count);
                Hide();
                _dialog.GetCookerItemDialog().GetController().Cooker.StartCooking();
            });
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void AddItemToList(CookerItemController dialog, int amount)
        {
            _dialog = dialog;
            var itemPrefab = GameObject.Instantiate(ModelPrefab.CookerOrderItemPrefab);
            var item = itemPrefab.EnsureComponent<OrderListItem>();
            var foodTechType = dialog.CookingItem.ReturnItem;

            var category = dialog.Mode == CookerMode.Cook || dialog.Mode == CookerMode.Custom
                ? AuxPatchers.CookedFoods()
                : AuxPatchers.CuredFoods();

            item.Initialize(foodTechType,amount, category,dialog,this);
            itemPrefab.transform.SetParent(_grid.transform,false);
            _orderQueue.Add(item);
            ListButton.UpdateCount(_orderQueue.Count);
        }

        internal void RemoveItemFromList(OrderListItem orderListItem)
        {
            _orderQueue.Remove(orderListItem);
            Destroy(orderListItem.gameObject);
            ListButton.UpdateCount(_orderQueue.Count);
        }
    }
}