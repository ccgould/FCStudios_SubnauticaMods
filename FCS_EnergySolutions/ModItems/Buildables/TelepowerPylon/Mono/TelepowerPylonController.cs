using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Buildable;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using System.Collections;
using TMPro;


namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono;
internal class TelepowerPylonController : FCSDevice, IFCSSave<SaveData>, IFCSDumpContainer
{

    public Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }

    private BaseTelepowerPylonManager _baseTelepowerPylonManager;
    private readonly Dictionary<string, FrequencyItemController> _trackedPullFrequencyItem = new();
    private readonly Dictionary<string, FrequencyItemController> _trackedPushFrequencyItem = new();
    private GameObject _pullGrid;   
    private bool _attemptedToLoadConnections;    
    private bool _loadingFromSave;
    private bool _isInUse;
    private bool _isFromConstructed;
    private TechType _currentUpgrade;

    [SerializeField] public TMP_Text status;
    [SerializeField] private DumpContainerSimplified _dumpContainer;
    [SerializeField] private ParticleSystem[] _particles;
    private Color _orangeColor = new Color(1f, 0.9398438f,1f);


    #region Unity Methods

    public override void Awake()
    {
        base.Awake();
        var interaction = gameObject.GetComponent<HoverInteraction>();
        interaction.onSettingsKeyPressed += onSettingsKeyPressed;
    }
    
    internal void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    public override void Start()
    {
        base.Start();

        if (CachedHabitatManager == null)
        {
            if (_particles is not null)
            {
                foreach (ParticleSystem particle in _particles)
                {
                    particle?.Stop();
                }
            }
        }
        else
        {
            _baseTelepowerPylonManager = CachedHabitatManager.GetSubRoot().GetComponentInChildren<BaseTelepowerPylonManager>();
        }
        InvokeRepeating(nameof(CheckTeleportationComplete), 0.2f, 0.2f);
        _baseTelepowerPylonManager?.RegisterPylon(this);
        HabitatService.main.GlobalNotifyByID(PluginInfo.PLUGIN_NAME, "PylonBuilt");
    }

    private void CheckTeleportationComplete()
    {
        QuickLogger.Debug("Checking if world is settled");
        if (LargeWorldStreamer.main.IsWorldSettled())
        {
            OnWorldSettled();
            CancelInvoke(nameof(CheckTeleportationComplete));
        }
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
            }

            _runStartUpOnEnable = false;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        OnDestroyCalledAction?.Invoke(null);
        _baseTelepowerPylonManager?.UnRegisterPylon(this);
        HabitatService.main.GlobalNotifyByID(PluginInfo.PLUGIN_NAME, "PylonDestroy");
    }

    #endregion

    #region Public Methods

    public override float GetPowerUsage()
    {
        if (!IsConstructed || CachedHabitatManager == null) return 0f;
        return CalculatePowerUsage();
    }
    
    public override Vector3 GetPosition()
    {
        return transform.position;
    }

    public override void Initialize()
    {
        if (IsInitialized) return;

        InitializeDumpContainer();

        _particles = gameObject.GetComponentsInChildren<ParticleSystem>();

        InvokeRepeating(nameof(UpdateStatus), 1f, 1f);

        IsInitialized = true;

        base.Initialize();
    }

    private void UpdateStatus()
    {
        if (_baseTelepowerPylonManager is null) return;
        var result = "N/A";

        switch (_baseTelepowerPylonManager.GetCurrentMode())
        {
            case TelepowerPylonMode.NONE:
                result = "AES_Standby";
                status.color = _orangeColor;
                break;
            case TelepowerPylonMode.PULL:
                result = "AES_Pull";
                status.color = Color.green;
                break;
            case TelepowerPylonMode.PUSH:
                result = "AES_Push";
                status.color = Color.green;
                break;
            case TelepowerPylonMode.RELAY:
                result = "";
                break;
        }

        status.text = Language.main.Get(result);
    }

    private void InitializeDumpContainer()
    {
        _dumpContainer.Initialize(transform, "Add Upgrade", this, 1, 1);
    }
    
    public bool AddItemToContainer(InventoryItem item)
    {
        var result = _baseTelepowerPylonManager.AttemptUpgrade(item.item.GetTechType());

        if (result)
        {
            Destroy(item.item.gameObject);
            return true;
        }

        PlayerInteractionHelper.GivePlayerItem(item);
        return false;
    }

    internal void ChangeTrailColor()
    {
        foreach (ParticleSystem system in _particles)
        {
            var index = Plugin.Configuration.TelepowerPylonTrailBrightness;
            var h = system.trails;
            h.colorOverLifetime = new Color(index, index, index);
        }
    }

    public bool CanAddNewPylon()
    {
        return _baseTelepowerPylonManager.GetConnections().Count < _baseTelepowerPylonManager.GetMaxConnectionLimit();
    }

    public void ActivateItemOnPushGrid(ITelepowerPylonConnection parentController)
    {
        var id = parentController.UnitID;
        if (IsPushFrequencyItem(id))
        {
            GetPushFrequencyItem(id)?.Check();
        }
    }

    public void ActivateItemOnPullGrid(ITelepowerPylonConnection parentController)
    {
        var id = parentController.UnitID.ToLower();
        if (IsPullFrequencyItem(id))
        {
            GetPullFrequencyItem(id)?.Check();
        }
    }

    public List<string> GetBasePylons()
    {
        return CachedHabitatManager.GetConnectedDevices(TelepowerPylonBuildable.PatchedTechType);
    }
  
    internal bool IsPullFrequencyItem(string id)
    {
        return _trackedPullFrequencyItem.ContainsKey(id.ToLower());
    }

    internal bool IsPushFrequencyItem(string id)
    {
        return _trackedPushFrequencyItem.ContainsKey(id.ToLower());
    }

    public void UnCheckFrequencyItem(string getBaseId)
    {
        if (_trackedPullFrequencyItem.ContainsKey(getBaseId))
        {
            _trackedPullFrequencyItem[getBaseId].UnCheck();
        }


        if (_trackedPushFrequencyItem.ContainsKey(getBaseId))
        {
            _trackedPushFrequencyItem[getBaseId].UnCheck();
        }
    }

    public void CheckFrequencyItem(string getBaseId)
    {
        if (_trackedPullFrequencyItem.ContainsKey(getBaseId))
        {
            _trackedPullFrequencyItem[getBaseId].Check();
        }


        if (_trackedPushFrequencyItem.ContainsKey(getBaseId))
        {
            _trackedPushFrequencyItem[getBaseId].Check();
        }
    }

    #endregion

    #region Private Methods

    private TelepowerPylonUpgrade GetCurrentUpgrade()
    {
        return _baseTelepowerPylonManager.GetUpgrade();
    }

    private float CalculatePowerUsage()
    {
        float amount = 0f;

        if (_baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PUSH)
        {
            foreach (var manager in BaseTelepowerPylonManager.GetGlobalTelePowerPylonsManagers())
            {
                if (manager.GetIsConnected(_baseTelepowerPylonManager.GetBaseID()))
                {
                    var distance = WorldHelpers.GetDistance(this, manager.GetRoot());
                    amount += distance * Plugin.Configuration.TelepowerPylonPowerUsagePerMeter;
                }
            }
        }
        return amount;
    }

    private FrequencyItemController GetPullFrequencyItem(string unitID)
    {
        return _trackedPullFrequencyItem[unitID.ToLower()];
    }

    private FrequencyItemController GetPushFrequencyItem(string unitID)
    {
        return _trackedPushFrequencyItem[unitID.ToLower()];
    }

    private void DeleteConnection(string id, bool removeCurrentContection = true, bool removeTrackedFrequencyItem = true)
    {
        QuickLogger.Debug($"Attempting to delete current connection {id}", true);

        if (_baseTelepowerPylonManager.GetConnections().ContainsKey(id))
        {
            _baseTelepowerPylonManager.GetConnections().Remove(id);
        }
        else
        {
            QuickLogger.Debug($"Failed to find connection in the list: {id}");
        }
    }

    private void OnWorldSettled()
    {
        if (_attemptedToLoadConnections) return;

        QuickLogger.Debug("OnWorld Settled", true);

        //if (_savedData?.CurrentConnections != null)
        //{

        //    foreach (string connection in _savedData.CurrentConnections)
        //    {
        //        QuickLogger.Debug($"Does Current Connections Contain Key: {connection} = {_baseTelepowerPylonManager.GetConnections().ContainsKey(connection)}");

        //        if (_baseTelepowerPylonManager.GetConnections().ContainsKey(connection)) continue;

        //        var item = _trackedPullFrequencyItem.SingleOrDefault(x => x.Key.Equals(connection));

        //        if (item.Value != null)
        //        {
        //            item.Value.Check(true);
        //        }
        //    }
        //}

        _attemptedToLoadConnections = true;
        _loadingFromSave = false;
    }

    internal void ChangeEffectColor(Color color)
    {
        foreach (ParticleSystem system in _particles)
        {
            var main = system.main;
            main.startColor = color;
        }
    }

    #endregion

    #region IProtoEventListener

    public override void ReadySaveData()
    {
        QuickLogger.Debug("In OnProtoDeserialize");
        //_savedData = Mod.GetTelepowerPylonSaveData(GetPrefabID());
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer)
    {
        //if (!IsInitialized
        //    || !IsConstructed) return;

        //if (_savedData == null)
        //{
        //    _savedData = new TelepowerPylonDataEntry();
        //}

        //_savedData.Id = GetPrefabID();

        //QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
        //_savedData.ColorTemplate = _colorManager.SaveTemplate();
        //_savedData.BaseId = BaseId;
        //_savedData.PylonMode = _baseTelepowerPylonManager.GetCurrentMode();
        //_savedData.CurrentConnections = GetCurrentConnectionIDs().ToList();
        //newSaveData.TelepowerPylonEntries.Add(_savedData);
    }

    private IEnumerable<string> GetCurrentConnectionIDs()
    {
        foreach (var connection in _baseTelepowerPylonManager.GetConnections())
        {
            var item = _trackedPullFrequencyItem.Any(x => x.Key.Equals(connection.Key) && x.Value.IsChecked());

            if (!item) continue;

            yield return connection.Key;
        }
    }

    #endregion

    #region IHand Target

    public override string[] GetDeviceStats()
    {


        //string additionalText = string.Empty;
        //if (_baseTelepowerPylonManager?.GetCurrentMode() == TelepowerPylonMode.PUSH)
        //{
        //    additionalText = AlterraHub.PowerPerMinuteDistance(CalculatePowerUsage() * 60);
        //}

        //string[] data;
        //if (string.IsNullOrWhiteSpace(additionalText))
        //{
        //    data = new[]
        //    {
        //                $"Unit ID: {UnitID} {additionalInformation} \n {message}",
        //            };
        //}
        //else
        //{
        //    data = new[]
        //    {
        //                $"Unit ID: {UnitID} {additionalInformation} \n {message} |",
        //                additionalText
        //            };
        //}

        return base.GetDeviceStats();
    }

    public bool IsAllowedToAdd(Pickupable pickupable, int containerTotal)
    {
        var result = pickupable.GetTechType() == BaseTelepowerPylonManager.Mk2UpgradeTechType || pickupable.GetTechType() == BaseTelepowerPylonManager.Mk3UpgradeTechType;
        if (!result)
        {
            QuickLogger.ModMessage("Only Telepower Pylon Upgrade MK2 and MK3 Allowed.");
        }

        return result;
    }

    internal void SetMode(TelepowerPylonMode mode)
    {
        _baseTelepowerPylonManager?.SetCurrentMode(mode);
    }

    internal BaseTelepowerPylonManager GetTelepowerBaseManager()
    {
        return _baseTelepowerPylonManager;
    }

    internal void OpenUpgradeContainer()
    {
        StartCoroutine(OpenStorage());
    }

    public IEnumerator OpenStorage()
    {
        QuickLogger.Debug($"Storage Button Clicked", true);

        //Close FCSPDA so in game pda can open with storage
        FCSPDAController.Main.Close();

        QuickLogger.Debug($"Closing FCS PDA", true);

        QuickLogger.Debug("Attempting to open the In Game PDA", true);
        Player main = Player.main;
        PDA pda = main.GetPDA();

        while (pda != null && pda.isInUse || pda.isOpen)
        {
            QuickLogger.Debug("Waiting for In Game PDA Settings to reset", true);
            yield return null;
        }

        QuickLogger.Debug("Gettings Reset", true);
        _dumpContainer.OpenStorage();
        yield break;
    }

    #endregion

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        
        if(_baseTelepowerPylonManager.HasConnections())
        {
            reason = Language.main.Get("AES_CannotDecontructPylon");
            return false;
        }
        return true;
    }
}
