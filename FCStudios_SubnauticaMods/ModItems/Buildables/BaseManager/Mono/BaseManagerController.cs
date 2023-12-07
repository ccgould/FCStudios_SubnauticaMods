using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCSCommon.Utilities;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class BaseManagerController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField]
    private GameObject statusInfoPrefab;


    public int MaxEPMStringLength { get; set; } = 24;

    //[SerializeField]
    //private Transform grid;

    [SerializeField]
    private TMP_Text infoItem1;

    [SerializeField]
    private TMP_Text infoItem2;

    [SerializeField]
    private TMP_Text infoItem3;

    [SerializeField]
    private TMP_Text infoItem4;

    [SerializeField]
    private TMP_Text infoItem5;

    [SerializeField]
    private TMP_Text infoItem6;

    [SerializeField]
    private TMP_Text infoItem7;

    [SerializeField]
    private TMP_Text infoItem8;

    [SerializeField]
    private TMP_Text baseName;



    private HabitatManager _baseManager;
    private float timeLeft;
    [SerializeField]
    private HoverInteraction _interaction;

    public override void OnEnable()
    {
        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            //if (_isFromSave)
            //{
            //    if (_savedData == null)
            //    {
            //        ReadySaveData();
            //    }

            //    _colorManager.LoadTemplate(_savedData.ColorTemplate);
            //    _requestedTime = _savedData.RequestedTime;
            //    _totalTime = _savedData.TotalTime;
            //    _countDown = _savedData.CountDown;

            //    if (_savedData.PendingItems?.Any() ?? false)
            //    {
            //        StartCoroutine(PerformShipping(_savedData.PendingItems, _savedData.PendingItems.Count() * 3.0f));
            //    }

            //}

            _runStartUpOnEnable = false;
        }
    }

    public override void Awake()
    {
        base.Awake();
        _interaction.onSettingsKeyPressed += onSettingsKeyPressed;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _interaction.onSettingsKeyPressed -= onSettingsKeyPressed;
    }

    public override void Initialize()
    {
        if (!IsInitialized)
        {
            QuickLogger.Debug("Base Manager Initializer");
            _baseManager = HabitatService.main.GetBaseManager(this);


            //AddInfo(SpriteManager.Get(TechType.Battery), () =>
            //{
            //    return $"{_baseManager.GetTotalPowerUsage():F2}";
            //});

            //AddInfo(SpriteManager.Get(TechType.Battery), () =>
            //{
            //    return _baseManager.GetTotalDevices().ToString();
            //});

            //AddInfo(SpriteManager.Get(TechType.Battery), () =>
            //{
            //    return _baseManager.GetConnectedDevices().ToString();
            //});
        }

        base.Initialize();
    }

    private void onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this);
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            UpdateData();
            timeLeft = 1;
        }
    }

    private void UpdateData()
    {
        //var friendlyName = !string.IsNullOrEmpty(_baseManager.GetBaseName()) ? _baseManager.GetBaseName() : "Base";
        baseName.text = _baseManager.GetBaseFriendlyName(); //$"{friendlyName} : BS{_baseManager.GetBaseID():D3}";
        infoItem1.text = DeterminePowerUsageString();

        var moreDevicesToConnect = _baseManager.GetFailedConnectionAttemptsCount() > 0 ? "*" : string.Empty;

        infoItem3.text = $"Connected Devices: {_baseManager.GetConnectedDevicesCount()}/{_baseManager.DetermineDeviceLimit()}{moreDevicesToConnect}";
        infoItem4.text = $"Remote Datalink: {IsRemoteDataLinkConnected()}";
        infoItem6.text = $"Installed Modules: {_baseManager.GetInstalledModulesCount()}/{_baseManager.DetermineModuleLimit()}";
        infoItem5.text = $"Fault: {DetermineFaults()}    Warn: {DetermineWarnings()}";
        infoItem2.text = $"Work Units: <color=#00ffffff>{_baseManager.GetWorkUnitsCount()}</color>";
        infoItem8.text = $"DSS Integration: {IsDssIntegration()}";
        infoItem7.text = $"Automated Ops.: {_baseManager.GetAutomatedOperationsCount()}/{_baseManager.DetermineOperationsLimit()}";
    }

    private string DeterminePowerUsageString()
    {
        var psuString = $"Power Use: {_baseManager.GetTotalPowerUsage() * 60:F1} epm";
        if (psuString.Length > MaxEPMStringLength)
        {
            psuString = $"EPM Use: {_baseManager.GetTotalPowerUsage() * 60:F1}";
        }

        return psuString;
    }

    private string DetermineWarnings()
    {
        var count = _baseManager.GetFaultsCount(FaultType.Warning);
        var color = count > 0 ? "yellow" : "green";
        return $"<color={color}>{count}</color>";
    }

    private string DetermineFaults()
    {
        var count = _baseManager.GetFaultsCount(FaultType.Fault);
        var color = count > 0 ? "red" : "green";
        return $"<color={color}>{count}</color>";
    }

    private string IsRemoteDataLinkConnected()
    {
        return _baseManager.IsRemoteLinkConnected() ? "<color=green>Online</color>" : "<color=red>Offline</color>";
    }

    private string IsDssIntegration()
    {
        bool isStorageSolutionsInstalled = false;
        string result;


        if (_baseManager.HasDevice("DSSTerminal".ToTechType()))
        {
            isStorageSolutionsInstalled = true;
        }

        if (!_baseManager.IsDssIntegration())
        {
            result = "<color=red>Offline</color>";
        }
        else if (!isStorageSolutionsInstalled && _baseManager.IsDssIntegration())
        {
            result = "<color=yellow>Offline</color>";
            uGUI_NotificationManager.Instance.AddNotification("No Terminal detected for DSS Integration");
        }        
        else
        {
            result = "<color=green>Online</color>";
        }

        return result;
    }

    //private void AddInfo(Atlas.Sprite sprite, Func<string> callBack)
    //{
    //    var category = Instantiate(statusInfoPrefab);
    //    var statusInfo = category.GetComponent<StatusInfo>();
    //    statusInfo.Initialize(sprite,callBack);
    //    category.transform.SetParent(grid, false);
    //}

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        
    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {
        
    }

    public override void ReadySaveData()
    {
        
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {GetPowerUsage() * 60:F2}] ",
        };


    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        
    }
}
