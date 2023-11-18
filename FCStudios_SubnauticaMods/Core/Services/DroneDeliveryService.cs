using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Spawnables.Drone;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services;
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

    //public IEnumerable<AlterraTransportDroneEntry> SaveDrones()
    //{
    //    var drones = GameObject.FindObjectsOfType<DroneController>();

    //    foreach (DroneController drone in drones)
    //    {
    //        yield return drone.Save();
    //    }
    //}

    private void CreatePorts()
    {
        //if (_ports?.Count > 0) return;
        //var ports = GameObjectHelpers.FindGameObjects(gameObject, "DronePortPad_HubWreck", SearchOption.StartsWith).ToArray();


        //if (ports.Length == 0) return;

        //for (int i = 0; i < 1; i++) // forcing to only make one port
        //{
        //    var portController = ports.ElementAt(i).AddComponent<AlterraDronePortController>();
        //    portController.SetPortID(i);
        //    _ports.Add($"Port_{i}", portController);
        //    portController.Initialize();
        //}
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
        QuickLogger.Debug("Delivery Initiated", true);

        //Call Coroutine to spawn a drone
        StartCoroutine(SpawnDrone(destination));
        callback?.Invoke(true);
    }

    private IEnumerator SpawnDrone(IShippingDestination destination)
    {
        //TODO Spawn Drone 

        //var portManager = AlterraHubLifePodDronePortController.main.GetDronePortController();


        //var itemResult = new TaskResult<DroneController>();
        //yield return portManager.SpawnDrone(itemResult);

        //var drone = itemResult.Get();

        //drone?.ShipOrder(destination);

        yield break;
    }

    public bool IsCurrentOrder(string shipmentOrderNumber)
    {
        return false;
    }
}