using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_AlterraHub.Mono.Managers
{
    internal class StoreManager  : MonoBehaviour
    {
        public Dictionary<string, List<CartItem>> PendingItems {get;set;} = new();
        public List<Shipment> PendingShipments {get;set;} = new();

        public static StoreManager main;

        public DroneDeliveryService DeliveryService;

        public void Awake()
        {
            main = this;
        }

        internal bool CompleteOrder(IStoreClient sender, string orderNumber, IShippingDestination destination)
        {
            bool wasOrderSuccessfull = false;
            
            if (string.IsNullOrWhiteSpace(orderNumber) || !PendingItems.ContainsKey(orderNumber)) return false;
            QuickLogger.Debug("Complete Order 1");

            var cart = PendingItems[orderNumber];

            var totalCash = GetCartTotal(orderNumber);

            if (FCSAlterraHubService.PublicAPI.IsInOreBuildMode())
            {
                foreach (CartItem cartItem in GetCartItems(orderNumber))
                {
                    if (cartItem != null && !KnownTech.Contains(cartItem.TechType))
                    {
                        if (CraftData.IsAllowed(cartItem.TechType) && KnownTech.Add(cartItem.TechType, true))
                        {
                            ErrorMessage.AddDebug("Unlocked " + Language.main.Get(cartItem.TechType.AsString()));
                        }
                    }
                }

                PendingItems.Remove(orderNumber);
            }
            else
            {
                QuickLogger.Debug("Complete Order 2");
                var sizes = GetSizes(orderNumber);

                switch (sender.ClientType)
                    {
                        case StoreClientType.PDA:
                        case StoreClientType.Hub:
                            if (destination.HasContructor)
                            {
                                if (destination.SendItemsToConstructor(PendingItems[orderNumber]))
                                {
                                    PendingItems.Remove(orderNumber);
                                }
                            }
                            else if (destination.HasDronePort)
                            {
                                QuickLogger.Debug("Complete Order 3");
                                DroneDeliveryService.Main.ShipOrder(destination, orderNumber, (result) =>
                                {
                                    if (!result) //If failed to ship Item give player items in inventory
                                    {
                                        if (CardSystem.main.HasEnough(totalCash) && Inventory.main.container.HasRoomFor(sizes))
                                        {
                                            foreach (CartItem item in cart)
                                            {
                                                for (int i = 0; i < item.ReturnAmount; i++)
                                                {
                                                    QuickLogger.Debug($"{item.ReceiveTechType}", true);
                                                    PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        QuickLogger.Debug("Complete Order 5");
                                        CreateOrder(sender, orderNumber);
                                        PendingItems.Remove(orderNumber);
                                        wasOrderSuccessfull = true;
                                    }
                                });
                            }
                            break;
                        case StoreClientType.Vehicle:
                            wasOrderSuccessfull = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }



                    //if (depot == null || !depot.HasRoomFor(sizes))
                    //{
                    //    MessageBoxHandler.main.Show(depot == null ? AlterraHub.DepotNotFound() : AlterraHub.DepotFull(), FCSMessageButton.OK);
                    //    return false;
                    //}

                    //if (DroneDeliveryService.Main == null)
                    //{
                    //    QuickLogger.Error("FCSStation Main is null!");
                    //    QuickLogger.ModMessage("The FCSStation cannot be found please contact FCSStudios for help with this issue. Order will be sent to your inventory");
                    //    MakeAPurchase(cart, null, true);
                    //    return true;
                    //}

            }

            CardSystem.main.RemoveFinances(totalCash);
            sender.OnOrderComplete(wasOrderSuccessfull);
            return wasOrderSuccessfull;
        }

        public string AddItemToCart(IStoreClient sender,string orderNumber, CartItem cartItemComponent)
        {
            
            if (string.IsNullOrEmpty(orderNumber))
            {
                orderNumber = Guid.NewGuid().ToString("n").Substring(0, 8);
                PendingItems.Add(orderNumber, new List<CartItem>
                {
                    cartItemComponent
                });
            }
            else if(PendingItems.ContainsKey(orderNumber))
            {
                PendingItems[orderNumber].Add(cartItemComponent);
            }
            sender.OnCreatedCartItem();

            cartItemComponent.onRemoveBTNClicked += (pendingItem =>
            {
                PendingItems[orderNumber].Remove(pendingItem);
                sender.OnRemoveCartItem(pendingItem.gameObject);
                //Destroy(pendingItem.gameObject);
            });

            return orderNumber;
        }

        public void CreateOrder(IStoreClient sender, string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) || !PendingItems.ContainsKey(orderNumber)) return;

            if (PendingItems.Any() && DeliveryService is not null)
            {
                //DeliveryService.SetCurrentOrder();
                PendingShipments.Add(new Shipment
                {
                    CartItems = PendingItems[orderNumber].SaveAll().ToList(),
                    OrderNumber = orderNumber,
                    //Port = port,
                    //PortPrefabID = port.GetPrefabID()
                });

                sender.OnOrderComplete(true);
            }
        }

        public decimal GetCartTotal(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) || !PendingItems.ContainsKey(orderNumber)) return 0;
            return PendingItems[orderNumber].Sum(x => StoreInventorySystem.GetPrice(x.TechType));
        }

        public int GetCartCount(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) || !PendingItems.ContainsKey(orderNumber)) return 0;
            return PendingItems[orderNumber].Count;
        }

        public IEnumerable<CartItem> GetCartItems(string orderNumber)
        {
            return PendingItems[orderNumber];
        }

        public void RemoveCartItem(string orderNumber, CartItem pendingItem)
        {
            if(PendingItems.ContainsKey(orderNumber))
                PendingItems[orderNumber].Remove(pendingItem);
        }

        public void RemovePendingOrder(string orderNumber)
        {
            if (!string.IsNullOrWhiteSpace(orderNumber) && PendingItems.ContainsKey(orderNumber))
                PendingItems.Remove(orderNumber);
        }
        
        private List<Vector2int> GetSizes(string orderNumber)
        {
            var items = new List<Vector2int>();
            foreach (CartItem cartItem in GetCartItems(orderNumber))
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
#if SUBNAUTICA
                    items.Add(CraftData.GetItemSize(cartItem.TechType));
#else
                    items.Add(TechData.GetItemSize(cartItem.TechType));
#endif
                }
            }

            return items;
        }

        public void CancelOrder(Shipment pendingOrder)
        {
            PendingShipments.Remove(pendingOrder);
        }
    }

    internal interface IShippingDestination
    {
        public bool HasDronePort { get; }
        public bool HasContructor { get; }
        public bool IsVehicle { get; set; }
        bool SendItemsToConstructor(List<CartItem> pendingItem);
        string GetBaseName();
    }


    public interface IStoreClient
    {
        public StoreClientType ClientType { get; }
        void OnOrderComplete(bool result);
        void OnCreatedCartItem();
        void OnDeletedCartItem();
        void OnRemoveCartItem(GameObject go);
    }

    public enum StoreClientType
    {
        None = -1,
        PDA = 0,
        Hub = 2,
        Vehicle = 3
    }
}
