using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
internal class RemoteStorageController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private List<uGUI_RemoteStorageItem> _inventoryButtons  = new();
    [SerializeField] private Text storageAmountLbl;
    [SerializeField] private Text deviceNameLbl;
    [SerializeField] private GridHelper _inventoryGrid;


    private FCSStorage _container;
    private DumpContainer _dumpContainer;
    private HoverInteraction _interaction;
    private const int MAXSTORAGE  = 200;

    public override void Awake()
    {
        base.Awake();
        _container = gameObject.GetComponent<FCSStorage>();
        _dumpContainer = gameObject.GetComponent<DumpContainer>();
        _interaction = gameObject.GetComponent<HoverInteraction>();

    }

    public override void Start()
    {
        base.Start();

        _dumpContainer.OnDumpContainerClosed += RefreshDevice;
        _inventoryGrid.OnLoadDisplay += GridHelper_OnLoadDisplay;
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
        deviceNameLbl.text = GetDeviceName();
        _container.container.onAddItem += onContainerChanged;
        _container.container.onRemoveItem += onContainerChanged;

        RefreshDevice();

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
    }

    private void RefreshDevice()
    {
        storageAmountLbl.text = LanguageService.StorageCountFormat(_container.GetCount(), MAXSTORAGE);
        _inventoryGrid.DrawPage();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        _dumpContainer.OnDumpContainerClosed -= RefreshDevice;

        _interaction.onSettingsKeyPressed -= onSettingsKeyPressed;
        _container.container.onAddItem -= onContainerChanged;
        _container.container.onRemoveItem -= onContainerChanged;    
    }

    private void GridHelper_OnLoadDisplay(DisplayData data)
    {
        try
        {
            var grouped = _container.container._items;

            if (grouped == null) return;

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
                _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value.items.Count);
            }

            _inventoryGrid.UpdaterPaginator(grouped.Count);
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    private void onContainerChanged(InventoryItem item)
    {
        RefreshDevice();
    }

    internal void OpenDumpContainer()
    {
        if (IsInitialized && IsConstructed)
        {
            _dumpContainer.OpenStorage();
        }
    }

    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, onPDAClose);
    }

    private void onPDAClose(FCSPDAController pda)
    {
        deviceNameLbl.text = GetDeviceName();
    }

    public override bool CanDeconstruct(out string reason)
    {
        return base.CanDeconstruct(out reason);
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<BaseSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving Cube Gen", true);

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
        QuickLogger.Debug($"Saves Cube Gen {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}]",
        };
    }

    internal FCSStorage GetStorage()
    {
        return _container;
    }
}