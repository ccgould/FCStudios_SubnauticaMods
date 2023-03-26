using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCSCommon.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services;

internal class StoreManager : MonoBehaviour
{
    public List<KeyValuePair<ShipmentInfo, List<CartItemSaveData>>> PendingItems { get; set; } = new();

    public List<Shipment> PendingShipments { get; set; } = new();

    public static StoreManager main;

    //TODO Drone System
    //public DroneDeliveryService DeliveryService;

    public void Awake()
    {
        main = this;
    }

    internal bool CompleteOrder(IStoreClient sender, ShipmentInfo shipmentInfo)
    {
        bool wasOrderSuccessfull = false;


        //TODO Drone System
        //if (string.IsNullOrWhiteSpace(shipmentInfo?.OrderNumber) || !HasShipment(shipmentInfo)) return false;
        //QuickLogger.Debug("Complete Order 1");

        //var cart = GetCartItems(shipmentInfo);

        //var totalCash = GetCartTotal(shipmentInfo);

        //shipmentInfo.TotalCash = totalCash;

        //if (FCSModsAPI.PublicAPI.IsInOreBuildMode())
        //{
        //    foreach (CartItemSaveData cartItem in GetCartItems(shipmentInfo))
        //    {
        //        if (!KnownTech.Contains(cartItem.TechType))
        //        {
        //            if (CraftData.IsAllowed(cartItem.TechType) && KnownTech.Add(cartItem.TechType, true))
        //            {
        //                ErrorMessage.AddDebug("Unlocked " + Language.main.Get(cartItem.TechType.AsString()));
        //            }
        //        }
        //    }

        //    RemovePendingItem(shipmentInfo);
        //}
        //else
        //{
        //    QuickLogger.Debug("Complete Order 2");
        //    var sizes = GetSizes(shipmentInfo);
        //    shipmentInfo.Sizes = sizes;
        //    var portManager = FCSAlterraHubService.PublicAPI.GetRegisteredBaseOfId(shipmentInfo.DestinationID)
        //        .GetPortManager();
        //    switch (sender.ClientType)
        //    {
        //        case StoreClientType.PDA:
        //        case StoreClientType.Hub:
        //            if (portManager.HasContructor)
        //            {
        //                if (portManager.SendItemsToConstructor(GetCartItems(shipmentInfo)))
        //                {
        //                    RemovePendingItem(shipmentInfo);
        //                    wasOrderSuccessfull = true;
        //                }
        //            }
        //            else if (portManager.HasDronePort)
        //            {
        //                QuickLogger.Debug("Complete Order 3");
        //                wasOrderSuccessfull = Ship(sender, shipmentInfo, portManager, cart);
        //            }
        //            break;
        //        case StoreClientType.Vehicle:
        //            wasOrderSuccessfull = true;
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }




        /*

        //Not sure if this was disabled before changes

        if (depot == null || !depot.HasRoomFor(sizes))
        {
            MessageBoxHandler.main.Show(depot == null ? AlterraHub.DepotNotFound() : AlterraHub.DepotFull(), FCSMessageButton.OK);
            return false;
        }

        if (DroneDeliveryService.Main == null)
        {
            QuickLogger.Error("FCSStation Main is null!");
            QuickLogger.ModMessage("The FCSStation cannot be found please contact FCSStudios for help with this issue. Order will be sent to your inventory");
            MakeAPurchase(cart, null, true);
            return true;
        }
            }
        */


        //AccountService.main.RemoveFinances(totalCash);
        //sender.OnOrderComplete(wasOrderSuccessfull);
        return wasOrderSuccessfull;
    }

    //TODO Drone System
    //private bool Ship(IStoreClient sender, ShipmentInfo shipmentInfo, PortManager portManager, List<CartItemSaveData> cart)
    //{
        //bool wasOrderSuccessfull = false;


        //DroneDeliveryService.Main.ShipOrder(portManager, shipmentInfo.OrderNumber, (result) =>
        //{
        //    if (!result) //If failed to ship Item give player items in inventory
        //    {
        //        if (AccountService.main.HasEnough(shipmentInfo.TotalCash) && Inventory.main.container.HasRoomFor(shipmentInfo.Sizes))
        //        {
        //            foreach (CartItemSaveData item in cart)
        //            {
        //                for (int i = 0; i < item.ReturnAmount; i++)
        //                {
        //                    QuickLogger.Debug($"{item.ReceiveTechType}", true);
        //                    PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        QuickLogger.Debug("Complete Order 5");
        //        CreateOrder(sender, shipmentInfo);
        //        RemovePendingItem(shipmentInfo);
        //        wasOrderSuccessfull = true;
        //    }
        //});
       // return wasOrderSuccessfull;
    //}

    public ShipmentInfo AddItemToCart(IStoreClient sender, ShipmentInfo shipmentInfo, CartItem cartItemComponent)
    {
        if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber))
        {
            shipmentInfo = new ShipmentInfo();
            shipmentInfo.OrderNumber = Guid.NewGuid().ToString("n").Substring(0, 8);
            cartItemComponent.ShipmentInfo = shipmentInfo;
            AddNewItem(shipmentInfo, new List<CartItemSaveData>
           {
               cartItemComponent.Save()
           });
        }
        else if (PendingItems.Any(x => x.Key.Equals(shipmentInfo)))
        {
            AddNewItemToExisting(shipmentInfo, cartItemComponent.Save());
        }
        
        sender.OnCreatedCartItem();

        cartItemComponent.onRemoveBTNClicked += (pendingItem) =>
        {
            main.RemoveCartItem(shipmentInfo, pendingItem.Save());
            sender.OnRemoveCartItem(pendingItem);
            sender.OnDeletedCartItem();
        };

        return shipmentInfo;
    }

    private void AddNewItem(ShipmentInfo shipmentInfo, List<CartItemSaveData> cartItemSaveDatas)
    {
        PendingItems.Add(new KeyValuePair<ShipmentInfo, List<CartItemSaveData>>(shipmentInfo, cartItemSaveDatas));
    }

    private void AddNewItemToExisting(ShipmentInfo shipmentInfo, CartItemSaveData cartItemSaveDatas)
    {
        PendingItems.First(x => x.Key.Equals(shipmentInfo)).Value.Add(cartItemSaveDatas);
    }

    private bool HasShipment(ShipmentInfo info)
    {
        return PendingItems.Any(x => x.Key.OrderNumber.Equals(info.OrderNumber));
    }

    private void RemovePendingItem(ShipmentInfo shipment)
    {
        var pair = PendingItems.FirstOrDefault(x => x.Key.Equals(shipment));
        PendingItems.Remove(pair);

    }

    internal List<CartItemSaveData> GetCartItems(ShipmentInfo info)
    {
        var result = PendingItems.FirstOrDefault(x => x.Key.OrderNumber.Equals(info.OrderNumber)).Value;
        QuickLogger.Debug($"GetCartItems: {result.Count}");
        return result;
    }

    public void CreateOrder(IStoreClient sender, ShipmentInfo shipmentInfo)
    {
        /*
         //TODO Drone System
        if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber) || !HasShipment(shipmentInfo)) return;

        if (PendingItems.Any() && DeliveryService is not null)
        {
            PendingShipments.Add(new Shipment
            {
                CartItems = GetCartItems(shipmentInfo),
                Info = shipmentInfo
            });

            sender?.OnOrderComplete(true);
        }
        */
    }

    public decimal GetCartTotal(ShipmentInfo shipmentInfo)
    {
        if (string.IsNullOrEmpty(shipmentInfo?.OrderNumber) || !HasShipment(shipmentInfo)) return 0;
        return GetCartItems(shipmentInfo).Sum(x => StoreInventoryService.GetPrice(x.TechType));
    }

    public int GetCartCount(ShipmentInfo shipmentInfo)
    {
        if (shipmentInfo is null || !HasShipment(shipmentInfo)) return 0;
        return GetCartItems(shipmentInfo).Count;
    }

    public void RemoveCartItem(ShipmentInfo shipmentInfo, CartItemSaveData pendingItem)
    {
        if (PendingItems.Any(x => x.Key.Equals(shipmentInfo)))
        {
            QuickLogger.Debug("GetCartItems: Found Pending Item");
            GetCartItems(shipmentInfo).Remove(pendingItem);
        }
    }

    public void RemovePendingOrder(ShipmentInfo shipmentInfo)
    {
        if (shipmentInfo == null) return;

        if (!string.IsNullOrWhiteSpace(shipmentInfo.OrderNumber) && HasShipment(shipmentInfo))
            RemovePendingItem(shipmentInfo);
    }

    private List<Vector2int> GetSizes(ShipmentInfo shipmentInfo)
    {
        var items = new List<Vector2int>();
        foreach (CartItemSaveData cartItem in GetCartItems(shipmentInfo))
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

    internal void CancelOrder(Shipment pendingOrder)
    {
        PendingShipments.Remove(pendingOrder);
    }

    public void Save()
    {
        //TODO Add to Save Manager

        //Mod.GamePlaySettings.StoreManagerSaveData = new StoreManagerSaveData
        //{
        //    PendingItems = PendingItems,
        //    PendingShipments = PendingShipments
        //};
    }

    internal void LoadSave()
    {

        //TODO Add to Save Manager

        //var saveData = Mod.GamePlaySettings.StoreManagerSaveData;

        //if (saveData?.PendingItems is not null)
        //{
        //    PendingItems = saveData.PendingItems;
        //}

        //if (saveData?.PendingShipments is not null)
        //{
        //    PendingShipments = saveData.PendingShipments;

        //    if (PendingShipments.Any())
        //    {
        //        var shipment = PendingShipments.First();
        //        Ship(null, shipment.Info, FCSAlterraHubService.PublicAPI.GetRegisteredBaseOfId(shipment.Info.DestinationID).GetPortManager(), shipment.CartItems);
        //    }
        //}

        //QuickLogger.Debug("Store Manager Save Loaded");
    }

    internal void CompleteOrder(Shipment shipment)
    {
        PendingShipments.Remove(shipment);
    }
}

[JsonObject]
internal class StoreManagerSaveData
{
    [JsonProperty]
    internal List<KeyValuePair<ShipmentInfo, List<CartItemSaveData>>> PendingItems { get; set; } = new();
    [JsonProperty]
    internal List<Shipment> PendingShipments { get; set; } = new();
}


public class ShipmentInfo
{
    [JsonProperty]
    internal string OrderNumber { get; set; }
    [JsonProperty]
    internal string DestinationID { get; set; }
    [JsonProperty]
    internal decimal TotalCash { get; set; }
    [JsonProperty]
    internal List<Vector2int> Sizes { get; set; }
    public string BaseName { get; set; }
}


internal interface IStoreClient
{
    public StoreClientType ClientType { get; }
    void OnOrderComplete(bool result);
    void OnCreatedCartItem();
    void OnDeletedCartItem();
    void OnRemoveCartItem(CartItem go);
}

public enum StoreClientType
{
    None = -1,
    PDA = 0,
    Hub = 2,
    Vehicle = 3
}
