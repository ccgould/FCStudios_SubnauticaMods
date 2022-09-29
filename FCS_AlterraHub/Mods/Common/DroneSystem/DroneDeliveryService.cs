using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem
{
    internal class DroneDeliveryService : MonoBehaviour
    {
        private Dictionary<string, IDroneDestination> _ports = new();
        private Dictionary<string, Shipment> _pendingPurchase = new();
        private HashSet<DroneController> _drones = new();
        private Shipment _currentOrder;
        private PortManager _portManager;
        private int MAXDRONECOUNT = 1;
        private bool _completePendingOrder;

        private void Awake()
        {
            QuickLogger.Debug("FCS Station Awake", true);

            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        public static DroneDeliveryService Main;


        private void Start()
        {
            InvokeRepeating(nameof(TryShip), 1f, 1f);
            CreatePorts();
        }

        private Dictionary<string, Shipment> ConvertPendingPurchase(Dictionary<string, Shipment> pendingPurchases)
        {
            if (pendingPurchases == null) return null;

            var result = new Dictionary<string, Shipment>();

            foreach (var purchase in pendingPurchases)
            {
                var station = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(purchase.Value.PortPrefabID);
                result.Add(purchase.Key, new Shipment
                {
                    PortPrefabID = purchase.Value.PortPrefabID,
                    Port = (AlterraDronePortController)station.Value,
                    CartItems = purchase.Value.CartItems.ToList(),
                    OrderNumber = purchase.Key
                });
            }

            return result;
        }

        public void PendAPurchase(AlterraDronePortController port, CartDropDownHandler cartItem)
        {
            PendAPurchase(port, cartItem.Save().ToList());
        }

        public void PendAPurchase(AlterraDronePortController port, List<CartItemSaveData> cartItem)
        {
            var orderNumber = Guid.NewGuid().ToString("n").Substring(0, 8);
            var shipment = new Shipment
            {
                CartItems = cartItem,
                OrderNumber = orderNumber,
                Port = port,
                PortPrefabID = port.GetPrefabID()
            };
            _pendingPurchase.Add(orderNumber, shipment);

            FCSPDAController.Main.AddShipment(shipment);

            SavePendingPurchase();
        }

        private void SavePendingPurchase()
        {
            Mod.GamePlaySettings.PendingPurchases = _pendingPurchase;
        }

        internal IDroneDestination GetAssignedPort(string prefabID)
        {
            return Mod.GamePlaySettings.DronePortAssigns.ContainsKey(prefabID)
                ? _ports.ElementAt(Mod.GamePlaySettings.DronePortAssigns[prefabID]).Value
                : null;
        }

        public void TryShip()
        {
            SpawnMissingDrones();

            if (_completePendingOrder && _drones.Any())
            {
                _drones.ElementAt(0).LoadData();
                _completePendingOrder = false;
            }


            if (!LargeWorldStreamer.main.IsWorldSettled() || _pendingPurchase.Count <= 0) return;

            for (int i = _pendingPurchase.Count - 1; i >= 0; i--)
            {
                foreach (DroneController drone in _drones)
                {
                    var purchase = _pendingPurchase.ElementAt(i);
                    if (purchase.Key is null)
                    {
                        foreach (CartItemSaveData cartItemSaveData in purchase.Value.CartItems)
                        {
                            cartItemSaveData.Refund();
                        }

                        _pendingPurchase.Remove(purchase.Key);
                        return;
                    }

                    if (drone is null)
                    { 
                        continue;
                    }

                    if (!drone.AvailableForTransport()) continue;

                    if (drone.ShipOrder(purchase.Value.Port))
                    {
                        _currentOrder = purchase.Value;
                        _pendingPurchase.Remove(purchase.Key);

                        VoiceNotificationSystem.main.ShowSubtitle($"{AlterraHub.OrderBeingShipped()} {purchase.Value.Port.GetBaseName()}");

                        Mod.GamePlaySettings.CurrentOrder = _currentOrder;
                    }

                    if (_pendingPurchase.Count <= 0) break;
                }
            }

            SavePendingPurchase();
        }

        private void SpawnMissingDrones(bool clearDrones = false)
        {
            QuickLogger.Info("SpawnMissingDrone");
            if (clearDrones)
            {
                ResetDrones();
                _drones?.Clear();
                _drones ??= new HashSet<DroneController>();
            }

            if (_drones.Count < MAXDRONECOUNT)
            {
                QuickLogger.Info("Drone Count is less than MaxCount");
                if (MAXDRONECOUNT > _ports.Count)
                {
                    QuickLogger.Info("Drone Count is Greater than Port Count");
                    MAXDRONECOUNT = _ports.Count;
                }

                for (int i = 0; i < MAXDRONECOUNT; i++)
                {
#if SUBNAUTICA_STABLE
                        var drone = _ports.ElementAt(i).Value.SpawnDrone();
                        _drones.Add(drone);
                        drone.LoadData();
#else
                    QuickLogger.Info($"Create Drone {_ports.ElementAt(i).Key}");

                    StartCoroutine(_ports.ElementAt(i).Value.SpawnDrone(drone =>
                    {
                        QuickLogger.Info("Add Drone");
                        _drones.Add(drone);
                        drone.LoadData();
                    }));
#endif
                }
                //if (!Mod.GamePlaySettings.TransDroneSpawned)
                //{

                //}
                //else
                //{
                //    var drone = GameObject.FindObjectOfType<DroneController>();
                //    _drones.Add(drone);
                //    drone.LoadData();
                //}
            }
        }

        internal void ResetDrones()
        {
            var drones = GameObject.FindObjectsOfType<DroneController>();

            foreach (DroneController controller in drones)
            {
                DestroyImmediate(controller.gameObject);
            }

            ClearDronesList();

            foreach (KeyValuePair<string, Shipment> shipment in _pendingPurchase)
            {
                FCSPDAController.Main.RemoveShipment(shipment.Value);
                foreach (CartItemSaveData cartItem in shipment.Value.CartItems)
                {
                    cartItem.Refund();
                }
            }

            Mod.GamePlaySettings.TransDroneSpawned = false;

            SpawnMissingDrones();
        }

        internal void ClearShipmentData()
        {
            _pendingPurchase.Clear();

            if (_currentOrder != null)
            {
                foreach (CartItemSaveData cartItem in _currentOrder.CartItems)
                {
                    cartItem.Refund();
                }
                FCSPDAController.Main.RemoveShipment(_currentOrder);
                _currentOrder = new Shipment();
            }


            Mod.GamePlaySettings.CurrentOrder = null;
            Mod.GamePlaySettings.PendingPurchases = new Dictionary<string, Shipment>();
        }

        private void ClearDronesList()
        {
            _drones ??= new HashSet<DroneController>();
            _drones.Clear();
        }

        public IEnumerable<AlterraTransportDroneEntry> SaveDrones()
        {
            var drones = GameObject.FindObjectsOfType<DroneController>();

            foreach (DroneController drone in drones)
            {
                yield return drone.Save();
            }
        }

        private void CreatePorts()
        {
            if (_ports?.Count > 0) return;
            var ports = GameObjectHelpers.FindGameObjects(gameObject, "DronePortPad_HubWreck", SearchOption.StartsWith)
                .ToArray();
            for (int i = 0; i < 1; i++) // forcing to only make one port
            {
                var portController = ports.ElementAt(i).AddComponent<AlterraDronePortController>();
                portController.SetPortID(i);
                _ports.Add($"Port_{i}", portController);
                portController.Initialize();
            }
        }

        private void FindPortManager()
        {
            if (_portManager == null)
            {
                _portManager = gameObject.GetComponent<PortManager>();
            }
        }

        public IDroneDestination FindPort(int port)
        {
            if (_portManager == null)
            {
                FindPortManager();
            }

            CreatePorts();

            return _portManager.FindPort(port);
        }

        public Shipment GetCurrentOrder()
        {
            return _currentOrder;
        }

        public AlterraDronePortController GetOpenPort()
        {
            return _portManager?.GetOpenPort();
        }

        public void ClearCurrentOrder()
        {
            FCSPDAController.Main.RemoveShipment(_currentOrder);
            _currentOrder = new Shipment();
            Mod.GamePlaySettings.CurrentOrder = new Shipment();
        }

        public void CancelOrder(Shipment pendingOrder)
        {
            if (_pendingPurchase.ContainsKey(pendingOrder.OrderNumber))
            {
                _pendingPurchase.Remove(pendingOrder.OrderNumber);
            }
        }

        public float GetOrderCompletionPercentage(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(_currentOrder?.OrderNumber)) return 0f;
            return _currentOrder.OrderNumber.Equals(orderNumber) ? _drones.ElementAt(0).GetCompletionPercentage() : 0f;
        }

        public bool IsCurrentOrder(string orderNumber)
        {

            if (_currentOrder == null || string.IsNullOrWhiteSpace(_currentOrder?.OrderNumber) || string.IsNullOrWhiteSpace(orderNumber))
                return false;
            return _currentOrder.OrderNumber.Equals(orderNumber);
        }

        public bool IsStationPort(string dockedPort)
        {
            if (string.IsNullOrWhiteSpace(dockedPort)) return false;

            foreach (var port in _ports)
            {
                if (port.Value.GetPrefabID().Equals(dockedPort))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsStationPort(IDroneDestination dockedPort)
        {
            return IsStationPort(dockedPort.GetPrefabID());
        }

        public bool IsStationBaseID(string dockedPort)
        {

            return _portManager.GetBaseID().Equals(dockedPort);
        }

        internal void SetCurrentOrder(Shipment currentOrder)
        {
            _currentOrder = currentOrder;
        }

        internal void SetCompletePendingOrder(bool v)
        {
            _completePendingOrder = v;
        }

        internal void SetPendingPurchase(Dictionary<string, Shipment> pendingPurchases)
        {
            _pendingPurchase = ConvertPendingPurchase(pendingPurchases) ?? _pendingPurchase;

            if (_pendingPurchase != null)
            {
                foreach (KeyValuePair<string, Shipment> shipment in _pendingPurchase)
                {
                    FCSPDAController.Main.AddShipment(shipment.Value);
                }
            }
        }

        internal HashSet<DroneController> GetDrones()
        {
            return _drones;
        }

        public void OnConsoleCommand_warp()
        {
            
        }

        public void Save()
        {

        }

        public bool DetermineIfFixed()
        {
            return true;
        }
    }
}
