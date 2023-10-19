using FCS_AlterraHub.Models.Mono;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class DSSServerController : MonoBehaviour
{
    private FCSStorage _fcsStorage;

    private List<IRackGaugeListener> listners = new();

    private void Awake()
    {
        _fcsStorage = gameObject.GetComponent<FCSStorage>();
        _fcsStorage.container.onAddItem += Container_OnAddItem;
        _fcsStorage.container.onRemoveItem += Container_OnRemoveItem;
    }

    private void Container_OnRemoveItem(InventoryItem item)
    {
        foreach (IRackGaugeListener listener in listners)
        {
            listener.UpdateValues();
        }
    }

    private void Container_OnAddItem(InventoryItem item)
    {
        foreach (IRackGaugeListener listener in listners)
        {
            listener.UpdateValues();
        }
    }

    internal bool HasItem(TechType techType)
    {
        return _fcsStorage.container.Contains(techType);
    }

    internal bool AddItem(Pickupable item)
    {
        if(_fcsStorage.CanBeStored(1,item.inventoryItem.techType))
        {
            _fcsStorage.container.AddItem(item);
            return true;
        }
        return false;
    }

    internal Pickupable GetItem(TechType techType)
    {
        return _fcsStorage.container.RemoveItem(techType);
    }

    internal int GetMaxStorage()
    {
        return _fcsStorage.SlotsAssigned;
    }

    internal int GetStorageTotal()
    {
        return _fcsStorage.GetCount();
    }

    internal void Subscribe(IRackGaugeListener callback)
    {
        listners.Add(callback);
    }

    internal void UnSubscribe(IRackGaugeListener callback)
    {
        listners.Remove(callback);
    }
}
