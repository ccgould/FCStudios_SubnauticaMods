using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubPod.Spawnable;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
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
        public Dictionary<ShipmentInfo, List<CartItem>> PendingItems {get;set;} = new();
        public List<Shipment> PendingShipments {get;set;} = new();

        public static StoreManager main;

        public DroneDeliveryService DeliveryService;

        public void Awake()
        {
            main = this;
        }

        internal bool CompleteOrder(IStoreClient sender, ShipmentInfo shipmentInfo)
        {
            bool wasOrderSuccessfull = false;
            
            if (string.IsNullOrWhiteSpace(shipmentInfo?.OrderNumber) || !PendingItems.ContainsKey(shipmentInfo)) return false;
            QuickLogger.Debug("Complete Order 1");

            var cart = PendingItems[shipmentInfo];

            var totalCash = GetCartTotal(shipmentInfo);

            if (FCSAlterraHubService.PublicAPI.IsInOreBuildMode())
            {
                foreach (CartItem cartItem in GetCartItems(shipmentInfo))
                {
                    if (cartItem != null && !KnownTech.Contains(cartItem.TechType))
                    {
                        if (CraftData.IsAllowed(cartItem.TechType) && KnownTech.Add(cartItem.TechType, true))
                        {
                            ErrorMessage.AddDebug("Unlocked " + Language.main.Get(cartItem.TechType.AsString()));
                        }
                    }
                }

                PendingItems.Remove(shipmentInfo);
            }
            else
            {
                QuickLogger.Debug("Complete Order 2");
                var sizes = GetSizes(shipmentInfo);

                switch (sender.ClientType)
                    {
                        case StoreClientType.PDA:
                        case StoreClientType.Hub:
                            if (shipmentInfo.Destination.HasContructor)
                            {
                                if (shipmentInfo.Destination.SendItemsToConstructor(PendingItems[shipmentInfo]))
                                {
                                    PendingItems.Remove(shipmentInfo);
                                    wasOrderSuccessfull = true;
                                }
                            }
                            else if (shipmentInfo.Destination.HasDronePort)
                            {
                                QuickLogger.Debug("Complete Order 3");
                                DroneDeliveryService.Main.ShipOrder(shipmentInfo.Destination, shipmentInfo.OrderNumber, (result) =>
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
                                        CreateOrder(sender, shipmentInfo);
                                        PendingItems.Remove(shipmentInfo);
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

        public ShipmentInfo AddItemToCart(IStoreClient sender,ShipmentInfo shipmentInfo, CartItem cartItemComponent)
        {


            if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber))
            {
                shipmentInfo = new ShipmentInfo();
                shipmentInfo.OrderNumber = Guid.NewGuid().ToString("n").Substring(0, 8);
                PendingItems.Add(shipmentInfo, new List<CartItem>
                {
                    cartItemComponent
                });
            }
            else if(PendingItems.ContainsKey(shipmentInfo))
            {
                PendingItems[shipmentInfo].Add(cartItemComponent);
            }
            sender.OnCreatedCartItem();

            cartItemComponent.onRemoveBTNClicked += (pendingItem =>
            {
                PendingItems[shipmentInfo].Remove(pendingItem);
                sender.OnRemoveCartItem(pendingItem.gameObject);
                //Destroy(pendingItem.gameObject);
            });

            return shipmentInfo;
        }

        public void CreateOrder(IStoreClient sender, ShipmentInfo shipmentInfo)
        {
            if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber) || !PendingItems.ContainsKey(shipmentInfo)) return;

            if (PendingItems.Any() && DeliveryService is not null)
            {
                //DeliveryService.SetCurrentOrder();
                PendingShipments.Add(new Shipment
                {
                    CartItems = PendingItems[shipmentInfo].SaveAll().ToList(),
                    OrderNumber = shipmentInfo.OrderNumber,
                    //Port = port,
                    //PortPrefabID = port.GetPrefabID()
                });

                sender.OnOrderComplete(true);
            }
        }

        public decimal GetCartTotal(ShipmentInfo shipmentInfo)
        {
            if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber) || !PendingItems.ContainsKey(shipmentInfo)) return 0;
            return PendingItems[shipmentInfo].Sum(x => StoreInventorySystem.GetPrice(x.TechType));
        }

        public int GetCartCount(ShipmentInfo shipmentInfo)
        {
            if (shipmentInfo is null || !PendingItems.ContainsKey(shipmentInfo)) return 0;
            return PendingItems[shipmentInfo].Count;
        }

        public IEnumerable<CartItem> GetCartItems(ShipmentInfo shipmentInfo)
        {
            return PendingItems[shipmentInfo];
        }

        public void RemoveCartItem(ShipmentInfo shipmentInfo, CartItem pendingItem)
        {
            if(PendingItems.ContainsKey(shipmentInfo))
                PendingItems[shipmentInfo].Remove(pendingItem);
        }

        public void RemovePendingOrder(ShipmentInfo shipmentInfo)
        {
            if (shipmentInfo == null) return;

            if (!string.IsNullOrWhiteSpace(shipmentInfo.OrderNumber) && PendingItems.ContainsKey(shipmentInfo))
                PendingItems.Remove(shipmentInfo);
        }
        
        private List<Vector2int> GetSizes(ShipmentInfo shipmentInfo)
        {
            var items = new List<Vector2int>();
            foreach (CartItem cartItem in GetCartItems(shipmentInfo))
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

    internal class ShipmentInfo
    {
        public string OrderNumber { get; set; }
        public IShippingDestination Destination { get; set; }
    }

    internal interface IShippingDestination
    {
        public bool HasDronePort { get; }
        public bool HasContructor { get; }
        public bool IsVehicle { get; set; }
        bool SendItemsToConstructor(List<CartItem> pendingItem);
        string GetBaseName();
        void SetInboundDrone(DroneController droneController);
        IDroneDestination ActivePort();
        string GetPreFabID();
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
