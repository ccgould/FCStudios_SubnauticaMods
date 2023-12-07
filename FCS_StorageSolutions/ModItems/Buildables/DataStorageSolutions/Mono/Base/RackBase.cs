using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Configuation;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using System.Collections;
using UnityEngine;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using System.Collections.Generic;
using FCS_AlterraHub.Core.Services;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
internal class RackBase : FCSDevice, IFCSSave<SaveData>
{
    protected private FCSStorage _fcsStorage;
    private DumpContainer _dumpContainer;
    private HabitatManager _habitatManager;
    private DSSManager _dssManager;
    [SerializeField] private List<RackGaugeController> _uGUI_RackGauges;
    [SerializeField] private uGUI_DSSRackProgressBar _uGUI_DSSRackProgressBar;
    private Dictionary<int, RackSlotData> driveAssignments = new();

    public override void Awake()
    {
        QuickLogger.Debug("Rack Awake", true);

        base.Awake();
        _fcsStorage = gameObject.GetComponent<FCSStorage>();     
        _dumpContainer = gameObject.GetComponent<DumpContainer>();
        var interaction = gameObject.GetComponent<HoverInteraction>();
        interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        CreateSlotAssignments();
    }

    private void CreateSlotAssignments()
    {
        for (int i = 0; i < _uGUI_RackGauges.Count; i++)
        {
            driveAssignments.Add(i, new RackSlotData(i, _uGUI_RackGauges[i]));
        }
    }

    public override void Start()
    {
        QuickLogger.Debug("Rack Start",true);

        base.Start();

        InvokeRepeating(nameof(CheckTeleportationComplete), 0.2f, 0.2f);

        _habitatManager = FCSModsAPI.PublicAPI.GetHabitat(this);
        _dssManager = DSSService.main.GetDSSManager(_habitatManager?.GetBasePrefabID());
        _fcsStorage.container.onAddItem += RackContainer_onAddItem;
        _fcsStorage.container.onRemoveItem += RackContainer_onRemoveItem;
        _fcsStorage.AddAllowedTech(DSSServerSpawnable.PatchedTechType);

        for (int i = 0; i < _uGUI_RackGauges.Count; i++)
        {
            var gauge = _uGUI_RackGauges[i];
            gauge.Initalize(i,this);
        }

        if (IsFromSave)
        {
            if (_dssManager is null)
            {
                QuickLogger.Debug("DSS Manager was null in OnEnable");
            }

            if (_habitatManager is null)
            {
                QuickLogger.Debug("Habitat Manager was null in OnEnable");
            }

            LoadFromSave();
            _uGUI_DSSRackProgressBar.Refresh();
            IsFromSave = false;
        }
    }

    public override void OnEnable()
    {
        QuickLogger.Debug("Rack Enabled", true);

        

        base.OnEnable();
    }

    private void CheckTeleportationComplete()
    {
        if (LargeWorldStreamer.main.IsWorldSettled() && gameObject.activeSelf)
        {
            if (CachedHabitatManager is null)
            {
                StartCoroutine(AttemptConnection());
            }

            CancelInvoke(nameof(CheckTeleportationComplete));
        }
    }

    private IEnumerator AttemptConnection()
    {
        while (CachedHabitatManager is null)
        {
            yield return new WaitForSeconds(1);
            Start();
            yield return null;
        }
    }

    private void LoadFromSave()
    {
        QuickLogger.Debug($"Storage Disk Check: {_fcsStorage.GetCount()}", true);

        _dssManager.LoadFromSave(_habitatManager.GetBasePrefabID(), _fcsStorage);

        foreach (InventoryItem inventoryItem in _fcsStorage.container)
        {
            RackContainer_onAddItem(inventoryItem);
        }
    }

    public void Test()
    {
        StartCoroutine(TestCoroutine());

    }

    private IEnumerator TestCoroutine()
    {
        CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(DSSServerSpawnable.PatchedTechType, true);
        yield return request;
        Pickupable component = Instantiate(request.GetResult()).GetComponent<Pickupable>();
        component.Pickup(false);
        InventoryItem newItem = new InventoryItem(component);
        _fcsStorage.AddItemToContainer(newItem);
        yield break;

    }

    private void RackContainer_onRemoveItem(InventoryItem item)
    {
        var serverStorage = item.item.gameObject.GetComponent<FCSStorage>();
        serverStorage.ItemsContainer.onAddItem -= _uGUI_DSSRackProgressBar.Refresh;
        serverStorage.ItemsContainer.onRemoveItem -= _uGUI_DSSRackProgressBar.Refresh;
        _dssManager.UnRegisterServer(this,serverStorage);
    }

    private void RackContainer_onAddItem(InventoryItem item)
    {
        QuickLogger.Debug("Registering Server", true);

        if (item?.item is null)
        {
            QuickLogger.Debug("Registering Server failed. Gameobject is null", true);
            return;
        }

        var serverStorage = item.item.gameObject.GetComponent<FCSStorage>();


        if (serverStorage?.ItemsContainer is null)
        {
            QuickLogger.Debug("Registering Server failed. FCSStorage is null", true);
            return;
        }

        serverStorage.ItemsContainer.onAddItem += _uGUI_DSSRackProgressBar.Refresh;
        serverStorage.ItemsContainer.onRemoveItem += _uGUI_DSSRackProgressBar.Refresh;

        if(_dssManager is null)
        {
            QuickLogger.Debug("Registering Server failed. DSSManager is null", true);
            return;
        }

        _dssManager.RegisterServer(this, serverStorage);

        QuickLogger.Debug("Registered Server",true);

        

        foreach (var slot in driveAssignments)
        {
            QuickLogger.Debug("Attempting to Find Empty Gauge", true);

            if (!slot.Value.IsOccupied)
            {
                QuickLogger.Debug("Found Empty Gauge", true);
                slot.Value.RegisterServer(item);
                QuickLogger.Debug("Added Server", true);
                break;
            }
        }
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<BaseSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving Server Rack", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new BaseSaveData();
        }

        var save = _savedData as BaseSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();

        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves Server Rack {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}]",
        };
    }

    internal void OpenDumpContainer()
    {
        QuickLogger.Debug("Open  Dump Container",true);
        _dumpContainer.OpenStorage();
    }

    internal FCSStorage GetStorage()
    {
        return _fcsStorage;
    }

    internal int GetMaxStorage()
    {
        return GetStorage().SlotsAssigned;
    }

    internal Pickupable ClearSlotAndReturnServer(int slot)
    {
        Pickupable item = null;

        QuickLogger.Debug($"{slot}");
        
        if(driveAssignments.TryGetValue(slot, out var currentSlot))
        {
            if (currentSlot is null)
            {
                QuickLogger.Error($"Is current SLOT is null {slot}");
                return null;
            }

            var inventoryItem = currentSlot.GetInventoryItem();

            if (inventoryItem is null)
            {
                QuickLogger.Error($"Is inventory item is null");
                return null;
            }

            currentSlot.OnEjectPressed();

            _fcsStorage.ItemsContainer.RemoveItem(inventoryItem.item, true);

            item = inventoryItem.item;
        }
        else
        {
            QuickLogger.Debug($"Is current slot null {currentSlot is null}");
        }

        return item;
    }

    internal int CalculateStorageTotal()
    {
        int total = 0;
        foreach (var item in GetStorage().container)
        {
            var server = item.item.gameObject.GetComponentInChildren<DSSServerController>();

            if (server is not null)
            {
                total += server.GetStorageTotal();
            }
        }

        return total;
    }

    internal int CalculateMaxStorage()
    {
        int maxStorage = 0;
        foreach (var item in GetStorage().container)
        {
            var server = item.item.gameObject.GetComponentInChildren<DSSServerController>();

            if (server is not null)
            {
                maxStorage += server.GetMaxStorage();
            }
        }

        return maxStorage;
    }

    internal string GetStorageAmountFormat()
    {
        QuickLogger.Debug("GetStorageAmountFormat",true);
        return LanguageService.StorageCountFormat(CalculateStorageTotal(), CalculateMaxStorage());
    }

    internal int GetServerInSlotCount()
    {
        return _fcsStorage.GetCount();
    }
}
