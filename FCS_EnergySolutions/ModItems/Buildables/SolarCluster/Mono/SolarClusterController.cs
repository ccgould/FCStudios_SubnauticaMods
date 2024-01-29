using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using System.Runtime.InteropServices;
using UnityEngine;
using static FCS_EnergySolutions.Configuration.SaveData;


namespace FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Mono;
internal class SolarClusterController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private SolarClusterPowerManager _powerManager;
    [SerializeField] private SolarClusterMovementManager _movementManager;
    private float nextChargeAttemptTimer;

    #region Unity Methods

    public override void Start()
    {
        base.Start();
        _powerManager.CheckIfConnected();
    }

    private void OnGlobalEntitiesLoaded()
    {
        QuickLogger.Debug($"On Global Entites Loaded: {GetPrefabID()}");
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

                var save = _savedData as SolarClusterSaveData;

                //if (!string.IsNullOrEmpty(_savedData.BaseId))
                //{
                //    BaseId = _savedData.BaseId;
                //}
                //_colorManager.LoadTemplate(_savedData.ColorTemplate);
                
                _powerManager.SetPower(save.StoredPower);
            }

            _runStartUpOnEnable = false;
        }
    }

    private void Update()
    {
        if (WorldHelpers.CheckIfPaused())
        {
            return;
        }

        //if (CachedHabitatManager == null)
        //{

        //    if (nextChargeAttemptTimer > 0f)
        //    {
        //        this.nextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
        //        if (this.nextChargeAttemptTimer < 0f)
        //        {
        //            var manager = BaseManager.FindManager(BaseId);
        //            if (manager != null)
        //            {
        //                Manager = manager;
        //            }
        //            else
        //            {
        //                this.nextChargeAttemptTimer = 5f;
        //            }
        //        }
        //    }
        //}
    }

    #endregion

    #region Public Methods

    public override Vector3 GetPosition()
    {
        return transform.position;
    }

    public override void Initialize()
    {
        base.Initialize();    
        

        //if (_colorManager == null)
        //{
        //    _colorManager = gameObject.AddComponent<ColorManager>();
        //    _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
        //}


       // MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
        //MaterialHelpers.ChangeSpecSettings(string.Empty, AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);

        //MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 50);
        //MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.red);

        IsInitialized = true;
    }

    public override bool IsOperational()
    {
        return IsConstructed;
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
             AuxPatchers.SolarClusterHover(Mathf.RoundToInt(_powerManager.GetRechargeScalar() * 100f),
                        Mathf.RoundToInt(_powerManager.GetPower()), Mathf.RoundToInt(_powerManager.GetMaxPower()),
                        Mathf.RoundToInt((_powerManager.GetRechargeScalar() * 0.20f/*0.25f old value */ * 5f) * 13f))
        };
    }
    #endregion

    #region IConstructable

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;

        if (constructed)
        {
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                IsInitialized = true;
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }
    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    #endregion

    #region Saving


    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<SolarClusterSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer)
    {
        if (!IsInitialized
            || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new SolarClusterSaveData();
        }

        var save = _savedData as SolarClusterSaveData;


        save.Id = GetPrefabID();

        QuickLogger.Debug($"Saving ID {save.Id}", true);
        save.ColorTemplate = _colorManager?.SaveTemplate() ?? new ColorTemplateSave();
        save.StoredPower = _powerManager.GetStoredPower();
        newSaveData.Data.Add(save);
    }

    #endregion
}
