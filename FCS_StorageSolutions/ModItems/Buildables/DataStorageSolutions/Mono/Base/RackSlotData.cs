using FCSCommon.Utilities;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;

public class RackSlotData
{
    private int _slotID { get; set; }
    private string _serverPrefabID { get; set; }
    private readonly RackGaugeController _gaugeController;
    private DSSServerController _dsserverController;
    private InventoryItem _inventoryItem;

    public RackSlotData(int slotID, RackGaugeController rackGaugeController)
    {
        _slotID = slotID;
        _gaugeController = rackGaugeController;
    }

    internal bool IsOccupied => _inventoryItem != null;

    internal void OnEjectPressed()
    {
        QuickLogger.Debug("[OnEjectPressed] RackSlotData",true);
        _gaugeController.Eject();
        _inventoryItem = null;
        _dsserverController = null;
    }

    internal void RegisterServer(InventoryItem inventoryItem)
    {
        _inventoryItem = inventoryItem;
        _dsserverController = inventoryItem.item.GetComponent<DSSServerController>();
        _serverPrefabID = _dsserverController.gameObject.GetComponent<PrefabIdentifier>()?.id ?? string.Empty;
        _gaugeController.SetServer(inventoryItem);
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
}