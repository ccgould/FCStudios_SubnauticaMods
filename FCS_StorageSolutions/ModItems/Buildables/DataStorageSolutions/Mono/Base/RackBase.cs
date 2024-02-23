using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using System.Collections;
using UnityEngine;
using FCS_AlterraHub.Core.Components;
using System.Collections.Generic;
using FCS_AlterraHub.Core.Services;
using System.Linq;
using FCS_StorageSolutions.Configuration;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
internal class RackBase : FCSDevice, IFCSSave<SaveData>
{
    protected private FCSStorage _fcsStorage;
    private DumpContainer _dumpContainer;
    private HabitatManager _habitatManager;
    private DSSManager _dssManager;
    [SerializeField] private List<RackSlotData> slots;
    [SerializeField] private uGUI_DSSRackProgressBar _uGUI_DSSRackProgressBar;

    public override void Awake()
    {
        QuickLogger.Debug("Rack Awake", true);

        base.Awake();


        _fcsStorage = gameObject.GetComponent<FCSStorage>();



        _dumpContainer = gameObject.GetComponent<DumpContainer>();
        var interaction = gameObject.GetComponent<HoverInteraction>();
        interaction.onSettingsKeyPressed += onSettingsKeyPressed;
    }

    public override void Initialize()
    {
        if(IsInitialized) return;

        InvokeRepeating(nameof(AttemptConnectionInvoke), 0.2f, 0.2f);

        StartCoroutine(LoadConfigurations());

        _fcsStorage.container.onAddItem += DockServer;
        _fcsStorage.container.onRemoveItem += UnDockServer;
        _fcsStorage.AddAllowedTech(DSSServerSpawnable.PatchedTechType);

        base.Initialize();
    }

    private IEnumerator LoadConfigurations()
    {
        while(_habitatManager is null || _dssManager is null)
        {
            if(_habitatManager is null)
            {
                _habitatManager = FCSModsAPI.PublicAPI.GetHabitat(this);

            }

            if(_dssManager is null)
            {
                _dssManager = DSSService.main.GetDSSManager(_habitatManager?.GetBasePrefabID());
            }

            yield return null;

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

        QuickLogger.Debug("Attempt Connection Successful", true);
    }

    public override void Start()
    {
        QuickLogger.Debug("Rack Start", true);

        base.Start();

    }

    public override void OnEnable()
    {
        QuickLogger.Debug("Rack Enabled", true);
        base.OnEnable();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_fcsStorage?.container is not null)
        {
            _fcsStorage.container.onAddItem -= DockServer;
            _fcsStorage.container.onRemoveItem -= UnDockServer;
        }
    }

    private void AttemptConnectionInvoke()
    {
        if (LargeWorldStreamer.main.IsWorldSettled() && gameObject.activeSelf)
        {
            if (CachedHabitatManager is null)
            {
                StartCoroutine(AttemptConnection());
            }

            CancelInvoke(nameof(AttemptConnectionInvoke));
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
        if (IsFromSave)
        {
            QuickLogger.Debug($"Is From Save: {GetPrefabID()}");
            if (_savedData == null)
            {
                ReadySaveData();
            }

            QuickLogger.Debug($"Is Save Data Present: {_savedData is not null}");

            if (_savedData is not null)
            {
                QuickLogger.Debug($"Setting Data");

                var savedData = _savedData as DSSRackSaveData;

                QuickLogger.Debug($"Storage Disk Check: {_fcsStorage.GetCount()}", true);

                _dssManager.LoadFromSave(_habitatManager.GetBasePrefabID(), _fcsStorage);

                List<InventoryItem> pending = new();

                QuickLogger.Debug($"FCStorage container count: {_fcsStorage.container.count}");

                foreach (InventoryItem inventoryItem in _fcsStorage.container)
                {
                    var prefabId = inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.id;
                    var dssserver = inventoryItem.item.gameObject.GetComponent<DSSServerController>();

                    QuickLogger.Debug($"Try to find server with Prefab ID: {prefabId}");

                    if (savedData.RackData.TryGetValue(prefabId, out int slotID))
                    {
                        QuickLogger.Debug($"Found server with Prefab ID: {prefabId} with slot ID {slotID}");

                        var slot = slots.FirstOrDefault(x => x.GetSlotID() == slotID);

      


                        if (slot is not null)
                        {
                            QuickLogger.Debug($"Found slot with ID: {slotID}");
                            slot.RegisterServer(inventoryItem);
                        }
                    }
                    else
                    {
                        QuickLogger.Debug($"Couldnt find slot with ID: {slotID} adding to queue");

                        pending.Add(inventoryItem);
                    }

                    DockServer(inventoryItem);
                }

                if(pending.Any())
                {
                    QuickLogger.Debug($"Trying to add pending servers");

                    foreach (var item in pending)
                    {
                        var slot = slots.FirstOrDefault(x => x.IsOccupied == false);

                        if (slot is not null)
                        {
                            QuickLogger.Debug($"Trying to add server to slor {slot.GetSlotID()}");
                            slot.RegisterServer(item);
                        }
                    }

                    pending.Clear();
                }

                _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);
            }
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

    private void UnDockServer(InventoryItem item)
    {
        var dssserver = item.item.gameObject;

        var serverStorage = dssserver.GetComponent<FCSStorage>();
        serverStorage.ItemsContainer.onAddItem -= Refresh;
        serverStorage.ItemsContainer.onRemoveItem -= Refresh;
        _dssManager.UnRegisterServer(this, serverStorage);

        var rigidbody = dssserver.GetComponent<Rigidbody>();
        dssserver.SetActive(false);
        rigidbody.isKinematic = false;
    }

    private void DockServer(InventoryItem item)
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

        serverStorage.ItemsContainer.onAddItem += Refresh;
        serverStorage.ItemsContainer.onRemoveItem += Refresh;

        if (_dssManager is null)
        {
            QuickLogger.Debug("Registering Server failed. DSSManager is null", true);
            return;
        }

        _dssManager.RegisterServer(this, serverStorage);

        QuickLogger.Debug("Registered Server", true);

        if (!IsFromSave)
        {
            var slot = slots.FirstOrDefault(x => x.IsOccupied == false);
            
            if(slot != null)
            {
                slot.RegisterServer(item);
            }
        }

        var dssserver = item.item.gameObject;

        var rigidbody = dssserver.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        //dssserver.position = driveSlotPositions.transform.position;

        dssserver.SetActive(true); // changed to true because easycraft isnt able to read the disk
    }

    private void Refresh(InventoryItem item)
    {
        _uGUI_DSSRackProgressBar.Refresh(item);
        HabitatService.main.GlobalNotifyByID(null, "RefreshTerminal");
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    public override void ReadySaveData()
    {
        string id = (GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<DSSRackSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving Server Rack", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new DSSRackSaveData();
        }

        var save = _savedData as DSSRackSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();
        save.RackData = new();

        foreach (RackSlotData driveAssignment in slots)
        {
            if (driveAssignment.IsOccupied)
            {
                var slotSave = driveAssignment.GetServer().Save();
                save.RackData.Add(slotSave.Item1, slotSave.Item2);
            }
        }

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
        QuickLogger.Debug("Open  Dump Container", true);
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

    internal Pickupable ClearSlotAndReturnServer(RackSlotData currentSlot)
    {
        Pickupable item = null;

        if (currentSlot is null)
        {
            QuickLogger.Error($"Is current SLOT is null");
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
        QuickLogger.Debug("GetStorageAmountFormat", true);
        return LanguageService.StorageCountFormat(CalculateStorageTotal(), CalculateMaxStorage());
    }

    internal int GetServerInSlotCount()
    {
        return _fcsStorage.GetCount();
    }

    internal List<RackSlotData> GetSlots()
    {
        return slots;
    }
}