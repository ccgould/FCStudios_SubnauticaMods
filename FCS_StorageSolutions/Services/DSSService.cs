using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_StorageSolutions.Services;
internal class DSSService : MonoBehaviour
{
    private Dictionary<string, DSSManager> _managers = new();
    internal Action<DSSManager, bool> NotifyNetworkConnectionChanged;
    public static DSSService main { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.


        if (main != null && main != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            main = this;
        }

        HabitatService.main.onBaseDestroyed += UnRegisterBase;
        HabitatService.main.onBaseCreated += RegisterBase;
    }

    private void UnRegisterBase(HabitatManager baseManager)
    {
        if(_managers.TryGetValue(baseManager.GetBasePrefabID(),out var dssManager))
        {
            dssManager.OnNetworkConnectionChanged -= OnNetworkConnectionChanged;
            _managers.Remove(baseManager.GetBasePrefabID());
        }

    }

    private void OnNetworkConnectionChanged(DSSManager dssManager,bool value)
    {
        NotifyNetworkConnectionChanged?.Invoke(dssManager, value);
    }

    private void RegisterBase(HabitatManager root)
    {
        QuickLogger.Debug($"Registering Base: {root.GetBaseName()}");
        var manager = root.gameObject.GetComponentInChildren<DSSManager>();
        var prefabID = root.GetBasePrefabID();

        if (manager != null)
        {
            if (!_managers.ContainsKey(prefabID))
            {
                _managers.Add(prefabID, manager);

                root.IsAllowedToAddToBase += (item) =>
                {
                    return manager.IsAllowedToAdd(item);
                };

                root.HasSpace += (amount) =>
                {
                    return manager.HasSpace(amount);
                };

                root.OnItemTransferedToBase += (item) =>
                {
                    manager.AddItem(item);
                };

                manager.OnNetworkConnectionChanged += OnNetworkConnectionChanged;

                QuickLogger.Debug($"Base: {root.GetBaseFriendlyName()} registered in DSSManager",true);
            }
        }
        else
        {
            QuickLogger.Error($"Failed to find DSSManager on Base {root.GetBasePrefabID()}");
        }
    }

    internal DSSManager GetDSSManager(string baseID)
    {

        QuickLogger.Debug($"Trying to find DSSManager with baseID {baseID} Registered bases: {_managers.Count}");

        if(!string.IsNullOrWhiteSpace(baseID) && _managers.ContainsKey(baseID))
        {
            return _managers[baseID];
        }
        return null;
    }

    internal bool AddItemToBase(HabitatManager habitat, InventoryItem inventoryItem)
    {
        if(_managers.ContainsKey(habitat.GetBasePrefabID()))
        {
            var ddsManager = _managers[habitat.GetBasePrefabID()];

            if(ddsManager is not null)
            {
                return ddsManager.AddItem(inventoryItem);
            }
        }

        return false;
    }

    internal Pickupable TakeItemFromBase(HabitatManager habitat, TechType techType)
    {
        if (_managers.ContainsKey(habitat.GetBasePrefabID()))
        {
            var ddsManager = _managers[habitat.GetBasePrefabID()];

            if (ddsManager is not null)
            {
                return ddsManager.RemoveItem(techType);
            }
        }

        return null;
    }

    internal Dictionary<string, DSSManager> GetManagers()
    {
        return _managers;
    }

    internal DSSManager GetDSSManager(HabitatManager manager)
    {
        return GetDSSManager(manager.GetBasePrefabID());
    }
}
