using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuation;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Enumerators;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Enumerator;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Spawnable;
using FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Buildable;
using FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FCSToolTip = FCS_AlterraHub.Core.Components.uGUIComponents.FCSToolTip;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
internal class DSSTerminalController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private GridHelper _inventoryGrid;
    [SerializeField] private List<uGUI_StorageItem> _inventoryButtons = new();
    [SerializeField] private PaginatorController _paginatorController;
    [SerializeField] private Text currentBaseLabel;
    [SerializeField] private Text baseLabel;
    [SerializeField] private Text serverCountLabel;
    [SerializeField] private Text rackCountLabel;
    [SerializeField] private Text totalCountLabel;
    [SerializeField] private TMP_InputField inputField;

    public BulkMultipliers BulkMultiplier;
    private DSSManager _dssManager;
    private string _currentSearch;
    private DSSTerminalFilterOptions filter;




    public override void Awake()
    {
        base.Awake();
        inputField.onValueChanged.AddListener(OnSearchBoxValueChanged);
        _paginatorController.OnPageChanged += _paginatorController_OnPageChanged;
        IPCMessage += OnIPCRecieved;
    }

    private void OnIPCRecieved(string message)
    {
        if (message.Equals("BaseUpdate"))
        {
            RefreshBaseName();
        }

    }

    private void _paginatorController_OnPageChanged(object sender, PaginatorController.OnPageChangedArgs e)
    {
        _inventoryGrid.DrawPage(e.PageIndex);
    }

    private void OnSearchBoxValueChanged(string value)
    {
        _currentSearch = value;
        RefreshGrid();
    }

    public override void Start()
    {
        QuickLogger.Debug($"Terminal Start Called {GetPrefabID()}");
        
        base.Start();

        QuickLogger.Debug($"Terminal Start End {GetPrefabID()}");

    }

    public override void OnEnable()
    {
        QuickLogger.Debug($"On Enable {GetPrefabID()} : {_runStartUpOnEnable}");

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

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

                    var savedData = _savedData as BaseSaveData;

                    _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);
                }
            }

            _runStartUpOnEnable = false;
        }
               
        QuickLogger.Debug($"On Enable End {GetPrefabID()} : {_runStartUpOnEnable}");
    }

    public override void Initialize()
    {
        if (IsInitialized) return;

        _inventoryGrid.OnLoadDisplay += GridHelper_OnLoadDisplay;

        foreach (var button in _inventoryButtons)
        {
            button.onItemClicked += GivePlayerItem;
        }

        RefreshGrid();
        base.Initialize();
    }

    public void OnNetworkButtonCliicked()
    {
        QuickLogger.Debug("Test", true);
    }

    public void OnDumpButtonCliicked()
    {
        _dssManager.GetHabitatManager().OpenItemTransfer();
    }

    public void OnPowerButtonCliicked()
    {
        QuickLogger.Debug("Test", true);
    }

    private void GivePlayerItem(TechType techType)
    {
        var amount = (int)BulkMultiplier;

        for (int i = 0; i < amount; i++)
        {
            if (!PlayerInteractionHelper.CanPlayerHold(techType))
            {
                QuickLogger.Message(LanguageService.InventoryFull());
                continue;
            }

            var item = _dssManager.RemoveItem(techType);

            if (item is not null)
            {
                PlayerInteractionHelper.GivePlayerItem(item);
            }
        }

        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _inventoryGrid.DrawPage();
    }

    private void GridHelper_OnLoadDisplay(DisplayData data)
    {
        try
        {
            GetManager();

            if (_dssManager is not null)
            {
                

                Dictionary<TechType, int> grouped;

                if (!string.IsNullOrEmpty(_currentSearch))
                {
                    grouped = _dssManager.GetBaseItems(filter).Where(i => i.Key.AsString(true).Contains(_currentSearch.ToLower())).ToDictionary(i => i.Key, i => i.Value);
                }
                else
                {
                    grouped = _dssManager.GetBaseItems(filter);
                }


                if (grouped is null) return;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

                _inventoryGrid?.UpdaterPaginator(grouped.Count);

                if (_inventoryGrid is not null)
                {
                    _paginatorController?.ResetCount(_inventoryGrid.GetMaxPages());

                }
            }

            //UpdateValuesOnScreen();
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    private void UpdateValuesOnScreen()
    {
        if (totalCountLabel is null || 
           serverCountLabel is null || 
           rackCountLabel is null ||
           GetDSSManager() is null ||
           CachedHabitatManager is null ||
           _dssManager is null) return;

        if (!totalCountLabel.isActiveAndEnabled ||
           !serverCountLabel.isActiveAndEnabled ||
           !rackCountLabel.isActiveAndEnabled) return;


        //Check Filter

        if (filter == DSSTerminalFilterOptions.ShowAll || filter == DSSTerminalFilterOptions.Servers)
        {
            var serverTotal = GetDSSManager().GetDeviceItemTotal(DSSServerSpawnable.PatchedTechType);

            var devices = GetDSSManager().GetHabitatManager().GetCount<RackBase>();


            if (devices is not null)
            {
                int rackCount = 0;


                foreach (var device in devices)
                {
                    var rack = device as RackBase;
                    rackCount += rack.GetServerInSlotCount();
                }

                var serverCapacity = rackCount * 48;

                totalCountLabel.text = $"{serverTotal.ToString("D4")}/{serverCapacity.ToString("D4")}";

            }
        }
        else if (filter == DSSTerminalFilterOptions.StorageLocker)
        {
            var lockerTotal = GetDSSManager().GetDeviceItemTotal(TechType.Locker) + GetDSSManager().GetDeviceItemTotal(TechType.SmallLocker);
            totalCountLabel.text = $"{lockerTotal.ToString("D4")}";

        }
        else if (filter == DSSTerminalFilterOptions.AlterraStorage)
        {
            var devices = GetDSSManager().GetHabitatManager().GetCount<RemoteStorageController>();

            var remoteStorageTotal = GetDSSManager().GetDeviceItemTotal(RemoteStorageBuildable.PatchedTechType);
            var value = (devices.Count() * 200);
            totalCountLabel.text = $"{remoteStorageTotal.ToString("D4")}/{value.ToString("D4")}";
        }
        else
        {
            totalCountLabel.text = $"{0.ToString("D4")}/{0.ToString("D4")}";

        }









        //terminalController.GetDSSManager().GetDeviceTotal()









        //var seaBreezeTotal = _currentBase.GetTotal(StorageType.SeaBreeze);
        //var harvesterTotal = _currentBase.GetTotal(StorageType.Harvester);
        //var replicatorTotal = _currentBase.GetTotal(StorageType.Replicator);
        //var remoteStorageTotal = _currentBase.GetTotal(StorageType.RemoteStorage);
        //var storageLockerTotal = _currentBase.GetTotal(StorageType.StorageLockers);

        //var alterraStorageCapacity = _currentBase?.BaseFcsStorage.Sum(x => x.GetMaxStorage()) ?? 0;
        //var seaBreezeCapacity = _currentBase.GetDevicesCount("SB") * 100;
        //var harvesterCapacity = _currentBase.GetDevicesCount("HH") * 150;
        //var replicatorCapacity = _currentBase.GetDevicesCount("RM") * 25;

        serverCountLabel.text = $"Servers: {GetDSSManager().GetServerCount():D3}";

        rackCountLabel.text = $"Racks: {GetDSSManager().GetRackCount():D3}";
    }

    private void GetManager()
    {
        
        if (CachedHabitatManager is null)
        {
            StartCoroutine(AttemptConnection());
        }

        //RegisterEventListener();
    }

    private bool RegisterEventListener()
    {
        
        CachedHabitatManager.OnTransferActionCompleted += RefreshDisplay;


        if (_dssManager is null)
        {
            _dssManager = DSSService.main.GetDSSManager(CachedHabitatManager?.GetBasePrefabID());
        }

        if (_dssManager is not null)
        {
            _dssManager.OnServerAdded += RefreshDisplay;
            _dssManager.OnServerRemoved += RefreshDisplay;
            _dssManager.OnRackRemoved += RefreshDisplay;
            return true;
        }

        return false;
    }

    private void UnRegisterEventListener()
    {
        CachedHabitatManager.OnTransferActionCompleted -= RefreshDisplay;

        if (_dssManager is not null)
        {
            //_dssManager.OnServerAdded -= RefreshDisplay;
            //_dssManager.OnServerRemoved -= RefreshDisplay;
            _dssManager.OnRackRemoved -= RefreshDisplay;
        }
    }

    private void RefreshDisplay()
    {

        QuickLogger.Debug("Refresh Display", true);
        RefreshGrid();
        UpdateValuesOnScreen();
    }

    private IEnumerator AttemptConnection()
    {
        while(CachedHabitatManager is null)
        {
            yield return new WaitForSeconds(1);
            Start();
            yield return null;
        }

        if (RegisterEventListener())
        {
            _dssManager.GetHabitatManager().OnModuleAdded += OnModuleChanged;
            _dssManager.GetHabitatManager().OnModuleRemoved += OnModuleChanged;

            var selectBase = IsCurrentBase() ? Language.main.Get("AHB_CurrentBase") : Language.main.Get("AHB_CurrentBase");
            currentBaseLabel.text = $"[{selectBase}]";
            RefreshBaseName();
            RefreshGrid();
        }
    }

    private void RefreshBaseName()
    {
        baseLabel.text = GenerateBaseName();
    }

    private bool IsCurrentBase()
    {
        if (_dssManager is null) return true;

        return _dssManager.GetHabitatManager() == CachedHabitatManager;
    }

    private string GenerateBaseName()
    {
        var baseName = string.IsNullOrEmpty(_dssManager.GetBaseFriendlyName()) ? _dssManager.GetBaseFormattedID() : _dssManager.GetBaseFriendlyName();

        return $"{baseName} - BS{CachedHabitatManager.GetBaseID():D3}";

    }

    private void OnModuleChanged(TechType type)
    {
        if(type == FCSModsAPI.PublicAPI.GetDssInterationTechType())
        {
            RefreshDisplay();
        }
    }

    public override void ReadySaveData()
    {
        string id = (GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<BaseSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving DSS Antenna", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new BaseSaveData();
        }

        var save = _savedData as BaseSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager?.SaveTemplate() ?? new();

        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves DSS Antenna {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}]",
        };
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_dssManager is not null)
        {
            QuickLogger.Info("DSS Unsubscribing", true);
            _dssManager.OnServerAdded -= RefreshGrid;
            _dssManager.OnServerRemoved -= RefreshGrid;

            UnRegisterEventListener();
        }
    }

    internal void ChangeFocusBase(DSSManager dssManager)
    {
        UnRegisterEventListener();

       _dssManager = dssManager;

        StartCoroutine(AttemptConnection());

        _inventoryGrid.DrawPage();
    }

    internal void SetFilter(DSSTerminalFilterOptions filter)
    {
        this.filter = filter;
        RefreshDisplay();
    }

    internal DSSManager GetDSSManager()
    {
        return _dssManager;
    }
}