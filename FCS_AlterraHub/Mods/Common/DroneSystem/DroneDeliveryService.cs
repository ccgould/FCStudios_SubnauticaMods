using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mods.AlterraHubPod.Spawnable;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem
{
    internal class DroneDeliveryService : MonoBehaviour
    {
        private Dictionary<string, IDroneDestination> _ports = new();
        private HashSet<DroneController> _drones = new();
        private PortManager _portManager;
        private int MAXDRONECOUNT = 1;

        private void Awake()
        {
            QuickLogger.Debug("Drone Delivery Service Awake", true);

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
            //InvokeRepeating(nameof(TryShip), 1f, 1f);
            CreatePorts();
        }

        internal IDroneDestination GetAssignedPort(string prefabID)
        {
            return Mod.GamePlaySettings.DronePortAssigns.ContainsKey(prefabID)
                ? _ports.ElementAt(Mod.GamePlaySettings.DronePortAssigns[prefabID]).Value
                : null;
        }

        //public void TryShip()
        //{
        //    SpawnMissingDrones();

        //    if (_completePendingOrder && _drones.Any())
        //    {
        //        _drones.ElementAt(0).LoadData();
        //        _completePendingOrder = false;
        //    }


        //    if (!LargeWorldStreamer.main.IsWorldSettled() || _pendingPurchase.Count <= 0) return;

        //    for (int i = _pendingPurchase.Count - 1; i >= 0; i--)
        //    {
        //        foreach (DroneController drone in _drones)
        //        {
        //            var purchase = _pendingPurchase.ElementAt(i);
        //            if (purchase.Key is null)
        //            {
        //                foreach (CartItemSaveData cartItemSaveData in purchase.Value.CartItems)
        //                {
        //                    cartItemSaveData.Refund();
        //                }

        //                _pendingPurchase.Remove(purchase.Key);
        //                return;
        //            }

        //            if (drone is null)
        //            { 
        //                continue;
        //            }

        //            if (!drone.AvailableForTransport()) continue;

        //            if (drone.ShipOrder(purchase.Value.Port))
        //            {
        //                _currentOrder = purchase.Value;
        //                _pendingPurchase.Remove(purchase.Key);

        //                VoiceNotificationSystem.main.ShowSubtitle($"{AlterraHub.OrderBeingShipped()} {purchase.Value.Port.GetBaseName()}");

        //                Mod.GamePlaySettings.CurrentOrder = _currentOrder;
        //            }

        //            if (_pendingPurchase.Count <= 0) break;
        //        }
        //    }

        //    //SavePendingPurchase();
        //}

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
            //var drones = GameObject.FindObjectsOfType<DroneController>();

            //foreach (DroneController controller in drones)
            //{
            //    DestroyImmediate(controller.gameObject);
            //}

            //ClearDronesList();
            
            //Mod.GamePlaySettings.TransDroneSpawned = false;

            //SpawnMissingDrones();
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


            if(ports.Length == 0) return;

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

            return _portManager?.FindPort(port);
        }

        public IDroneDestination GetOpenPort()
        {
            return _portManager?.GetOpenPort();
        }

        
        public float GetOrderCompletionPercentage(string orderNumber)
        {
             return 0f;
            //return _drones.ElementAt(0).GetCompletionPercentage() : 0f;
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
        
        private HashSet<DroneController> GetDrones()
        {
            return _drones;
        }
        
        public void Save()
        {

        }

        public bool DetermineIfFixed()
        {
            return true;
        }

        public void ShipOrder(IShippingDestination destination, string orderNumber, Action<bool> callback)
        {
            QuickLogger.Debug("Delivery Initiated",true);
            var portManager = AlterraHubLifePodDronePortController.main.GetDronePortController();

            var drone = portManager.SpawnDrone();

            drone?.ShipOrder(destination);

            callback?.Invoke(true);
        }

        public bool IsCurrentOrder(string shipmentOrderNumber)
        {
            return false;
        }
    }
}
