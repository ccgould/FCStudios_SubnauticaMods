using UnityEngine;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class TelepowerPylonPowerManager : MonoBehaviour
    {
       // private bool _isInitialized;
       //// private TelepowerPylonController _mono;
       //private PowerRelay _connectedPowerSource;
       // private readonly Dictionary<string, ITelepowerPylonConnection> _connections = new();
       // private bool _pauseUpdates;


       // private void UpdateConnections()
       // {
       //     if (_mono != null && _mono.Manager != null && _mono.IsConstructed && !_pauseUpdates)
       //     {

       //         if (_connectedPowerSource == null)
       //         {
       //             _connectedPowerSource = _mono.Manager.Habitat.powerRelay;
       //         }

       //         foreach (var connection in _connections)
       //         {
       //             if (connection.Value.GetPowerRelay() == null) continue;
       //             _connectedPowerSource.AddInboundPower(connection.Value.GetPowerRelay());
       //         }
       //     }
       // }

       // public void Initialize(TelepowerPylonController mono)
       // {
       //     if (_isInitialized) return;
       //     _mono = mono;
            
       //     InvokeRepeating(nameof(UpdateConnections),1f,1f);

       //     _isInitialized = true;
       // }
        
       // public void AddConnection(ITelepowerPylonConnection controller)
       // {
       //     var unitID = controller.UnitID.ToLower();
       //     if (!_connections.ContainsKey(unitID))
       //         _connections?.Add(unitID, controller);
       // }

       // public IPowerInterface GetPowerRelay()
       // {
       //     return _connectedPowerSource;
       // }

       // public bool HasConnections()
       // {
       //     return _connections.Any();
       // }

       // public void RemoveConnection(string id)
       // {
       //     if (!_connections.ContainsKey(id.ToLower())) return;
       //     _pauseUpdates = true;
       //     _connectedPowerSource.RemoveInboundPower(_connections[id.ToLower()].GetPowerRelay());
       //     _connections.Remove(id.ToLower());
       //     _pauseUpdates = false;
       // }

       // public Dictionary<string, ITelepowerPylonConnection> GetConnections()
       // {
       //     return _connections;
       // }
        
       // public bool HasConnection(string unitId)
       // {
       //     if (string.IsNullOrWhiteSpace(unitId)) return false;
       //     return _connections?.ContainsKey(unitId.ToLower()) ?? false;
       // }
    }
}
