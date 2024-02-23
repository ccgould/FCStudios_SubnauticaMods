using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;

internal class DSSItemDisplayController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private Text itemCountLBL;
    [SerializeField] private uGUI_Icon itemIcon;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private DumpContainer dumpContainer;
    private TechType currentItem = TechType.None;
    [SerializeField] private FCSToolTip fcsToolTip;
    private DSSManager _dssManager;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnEnable()
    {
        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                if (_savedData is not null)
                {
                    var savedData = _savedData as DSSItemDisplaySaveData;
                    currentItem = savedData.CurrentTechType;
                    //_colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);
                }

                SetItem(currentItem);
            }

            _runStartUpOnEnable = false;
        }
    }

    public override void Initialize()
    {
        if(!IsInitialized)
        {
            dumpContainer.OnDumpContainerItemSample += DumpContainer_OnDumpContainerItemSample;

            fcsToolTip.RequestPermission += () =>
            {
                return WorldHelpers.CheckIfPlayerInRange(this, 3f);
            };

            SetIcon(currentItem);

            InvokeRepeating(nameof(AttemptDSSConnection), 0.2f, 0.2f);
        }

        base.Initialize();
    }

    private void SetIcon(TechType techType)
    {
        itemIcon.sprite = SpriteManager.Get(techType);
        currentItem = techType;
    }

    private void DumpContainer_OnDumpContainerItemSample(Pickupable obj)
    {
        SetItem(obj.GetTechType());
    }

    private void SetItem(TechType obj)
    {
        SetIcon(obj);
        RefreshDisplay();
    }

    private void AttemptDSSConnection()
    {
        if(CachedHabitatManager is null)
        {
            QuickLogger.DebugError("CachedHabitatManager is null");
            Start();
            return;
        }

        if(FindDSSManager())
        {
            _dssManager.GetHabitatManager().OnItemTransferedToBase += BaseDump_ItemTransferedToBase;
            _dssManager.OnServerAdded += RefreshDisplay;
            _dssManager.OnServerRemoved += RefreshDisplay;
            RefreshDisplay();
            CancelInvoke(nameof(AttemptDSSConnection));
        }        
    }

    //private void CheckTeleportationComplete()
    //{
    //    QuickLogger.Debug("Checking if world is settled");

    //    if (LargeWorldStreamer.main.IsWorldSettled() && gameObject.activeSelf)
    //    {
    //        if (CachedHabitatManager is null)
    //        {
    //            StartCoroutine(AttemptConnection());
    //        }

    //        CancelInvoke(nameof(CheckTeleportationComplete));
    //    }
    //}


    //private IEnumerator AttemptConnection()
    //{
    //    while (CachedHabitatManager is null)
    //    {
    //        yield return new WaitForSeconds(1);
    //        Start();
    //        yield return null;
    //    }

    //    while (_dssManager is null)
    //    {
    //        yield return new WaitForSeconds(1);
    //        RegisterEventListener();
    //        yield return null;
    //    }

    //    if (RegisterEventListener())
    //    {
    //        _dssManager.GetHabitatManager().OnItemTransferedToBase += BaseDump_ItemTransferedToBase;
    //        _dssManager.OnServerAdded += RefreshDisplay;
    //        _dssManager.OnServerRemoved += RefreshDisplay;
    //        RefreshDisplay();
    //    }
    //}

    private void BaseDump_ItemTransferedToBase(InventoryItem item)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if(_dssManager is not null)
        {
            itemCountLBL.text = _dssManager.GetItemTotal(currentItem).ToString();
        }
        resetButton.SetActive(currentItem != TechType.None);
    }

    private bool FindDSSManager()
    {
        if (_dssManager is null)
        {
            _dssManager = DSSService.main.GetDSSManager(CachedHabitatManager?.GetBasePrefabID());
        }

        if (_dssManager is not null)
        {
            return true;
        }

        return false;
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<DSSItemDisplaySaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving DSS S24", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new DSSItemDisplaySaveData();
        }

        var save = _savedData as DSSItemDisplaySaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();
        save.CurrentTechType = currentItem;
        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves DSS S24 {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}]",
        };
    }

    public void OnStorageButtonClicked()
    {
        _dssManager.GetHabitatManager().OpenItemTransfer();
    }

    public void OnItemButtonClicked()
    {
        if(currentItem == TechType.None)
        {
            dumpContainer.OpenStorage();
        }
        else
        {
            QuickLogger.Debug($"Give Player Item {currentItem.AsString()}");

            if (!WorldHelpers.CheckIfPlayerInRange(this, 3f))
            {
                QuickLogger.DebugError($"Player not in range of item display");
                return;
            }

            if (!PlayerInteractionHelper.CanPlayerHold(currentItem))
            {
                QuickLogger.Message(LanguageService.InventoryFull());
            }

            var item = _dssManager.RemoveItem(currentItem);

            if (item is not null)
            {
                PlayerInteractionHelper.GivePlayerItem(item);
            }

            CachedHabitatManager.OnTransferActionCompleted?.Invoke();        
            RefreshDisplay();
        }
    }

    public void OnResetButtonClicked()
    {
        currentItem = TechType.None;
        itemCountLBL.text = "0";
        itemIcon.sprite = SpriteManager.Get(TechType.None);
        resetButton.SetActive(false);
    }

    public override void OnDestroy()
    {
        _dssManager.GetHabitatManager().OnItemTransferedToBase -= BaseDump_ItemTransferedToBase;
        _dssManager.OnServerAdded -= RefreshDisplay;
        _dssManager.OnServerRemoved -= RefreshDisplay;
    }
}