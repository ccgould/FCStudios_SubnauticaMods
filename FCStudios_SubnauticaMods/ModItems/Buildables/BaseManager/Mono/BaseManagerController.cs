using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class BaseManagerController : FCSDevice
{
    [SerializeField]
    private GameObject statusInfoPrefab;

    [SerializeField]
    private Transform grid;
    private HabitatManager _baseManager;


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

    public override void Initialize()
    {
        if (!IsInitialized)
        {
            QuickLogger.Debug("Base Manager Initializer");
            _baseManager = HabitatService.main.GetBaseManager(this);

            AddInfo(SpriteManager.Get(TechType.Battery), () =>
            {
                return $"{_baseManager.GetTotalPowerUsage():F2}";
            });

            AddInfo(SpriteManager.Get(TechType.Battery), () =>
            {
                return _baseManager.GetTotalDevices().ToString();
            });

            AddInfo(SpriteManager.Get(TechType.Battery), () =>
            {
                return _baseManager.GetConnectedDevices().ToString();
            });
        }

        base.Initialize();
    }

    private void AddInfo(Atlas.Sprite sprite, Func<string> callBack)
    {
        var category = Instantiate(statusInfoPrefab);
        var statusInfo = category.GetComponent<StatusInfo>();
        statusInfo.Initialize(sprite,callBack);
        category.transform.SetParent(grid, false);
    }

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
}
