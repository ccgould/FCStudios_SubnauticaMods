using System.Collections.Generic;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Components;
using FCSCommon.Helpers;
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
                    var item = _orderQueue[i];
                    var result = mono.TryGetItem(item.Dialog, item.Dialog.GetCookerItemDialog().Amount);
                    if(!result)
                    {
                        break;
                    }
                    RemoveItemFromList(_orderQueue[i]);
                }
                ListButton.UpdateCount(_orderQueue.Count);
                Hide();
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

        internal void AddItemToList(CookerItemController dialog)
        {
            var itemPrefab = GameObject.Instantiate(ModelPrefab.CookerOrderItemPrefab);
            var item = itemPrefab.EnsureComponent<OrderListItem>();
            var foodTechType = dialog.Mode == CookerMode.Cook || dialog.Mode == CookerMode.Custom
                ? dialog.CookedTechType
                : dialog.CuredTechType;

            var category = dialog.Mode == CookerMode.Cook || dialog.Mode == CookerMode.Custom
                ? AuxPatchers.CookedFoods()
                : AuxPatchers.CuredFoods();

            item.Initialize(foodTechType,dialog.GetCookerItemDialog()?.Amount ?? 0, category,dialog,this);
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