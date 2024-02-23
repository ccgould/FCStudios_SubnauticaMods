using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;

public class RackSlotData : MonoBehaviour
{
    [SerializeField] private RackGaugeController _gaugeController;
    [SerializeField] private int _slotID;
    private DSSServerController _dsserverController;
    private InventoryItem _inventoryItem;

    internal bool IsOccupied => _inventoryItem != null;

    private void Awake()
    {
        InvokeRepeating(nameof(UpdateGauge),1f, 1f);
    }

    internal void OnEjectPressed()
    {
        QuickLogger.Debug("[OnEjectPressed] RackSlotData", true);
        _dsserverController.SetSlot(null);
        _inventoryItem = null;
        _dsserverController = null;
        //_gaugeController.UpdateValues(null);

    }

    internal void RegisterServer(InventoryItem inventoryItem)
    {
        _inventoryItem = inventoryItem;
        _dsserverController = inventoryItem.item.GetComponent<DSSServerController>();
        _dsserverController.SetSlot(this);
        //UpdateGauge();
    }

    internal bool HasServer(InventoryItem inventoryItem)
    {
        return _inventoryItem == inventoryItem;
    }

    internal InventoryItem GetInventoryItem()
    {
        QuickLogger.Debug($"[GetInventoryItem] is null {_inventoryItem is null}", true);
        return _inventoryItem;
    }

    internal DSSServerController GetServer()
    {
        return _dsserverController;
    }

    internal int GetSlotID()
    {
        return _slotID;
    }

    private void UpdateGauge()
    {
        _gaugeController.UpdateValues(_dsserverController);
    }
}