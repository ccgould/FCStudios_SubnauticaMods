using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Mono;
internal class PowerStorageController : FCSDevice, IFCSSave<SaveData>
{



    private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
    private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
    private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Image _bar;
    [SerializeField] private ParticleSystem[] _particles;
    private FMOD_CustomLoopingEmitter _audio;
    private bool _allowedToCharge;
    private BasePowerStorage _basePowerStorage;
    private float _amountRemain;

    [SerializeField] private PowerSource powerSource;

    public override void OnConnectedToManager()
    {
        if(CachedHabitatManager is not null)
        {
            _basePowerStorage = CachedHabitatManager.gameObject.GetComponent<BasePowerStorage>();
            _basePowerStorage.Register(this);
        }
    }

    public override Vector3 GetPosition()
    {
        return transform.position;
    }

    private void UpdateVisuals()
    {
        var percentage = powerSource.power / powerSource.maxPower;
        if (_bar != null)
        {

            if (percentage >= 0f)
            {
                Color value = (percentage < 0.5f) ? Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percentage) : Color.Lerp(this._colorHalf, this._colorFull, 2f * percentage - 1f);
                _bar.color = value;
                _bar.fillAmount = percentage;
                ChangeEffectColor(value);
                return;
            }
            _bar.color = _colorEmpty;
            _bar.fillAmount = 0f;
            ChangeEffectColor(_colorEmpty);
        }
    }

    internal void ChangeTrailBrightness()
    {
        foreach (ParticleSystem system in _particles)
        {
            var index = Plugin.Configuration.TelepowerPylonTrailBrightness;
            var h = system.trails;
            h.colorOverLifetime = new Color(index, index, index);
        }
    }

    private void ChangeEffectColor(Color color)
    {
        foreach (ParticleSystem system in _particles)
        {
            var main = system.main;
            main.startColor = color;
        }
    }

    public override void Start()
    {
        base.Start();
    }

    public override void OnEnable()
    {
        base.OnEnable();
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

               // _colorManager.LoadTemplate(_savedData.ColorTemplate);
                _runStartUpOnEnable = false;
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _basePowerStorage?.Unregister(this);
    }

    public override void ReadySaveData()
    {
        
            string id = (GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>()).Id;
            _savedData = ModSaveManager.GetSaveData<PowerStorageDataEntry>(id);
            QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public override void Initialize()
    {
        if (IsInitialized) return;

        //if (powerSource == null)
        //{
        //    powerSource = gameObject.GetComponent<PowerSource>();
        //}

        _particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        _bar = GameObjectHelpers.FindGameObject(gameObject, "BarFill").GetComponent<Image>();
        _audio = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");

        MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);

        InvokeRepeating(nameof(UpdateVisuals), 1f, 1f);
        InvokeRepeating(nameof(TakePower), 1f, 1f);

        IsInitialized = true;
    }

    public void Charge()
    {
        _allowedToCharge = true;
    }

    public void Discharge()
    {
        _allowedToCharge = false;
    }
      

    public override bool CanDeconstruct(out string reason)
    {
        //if (PowercellCharger != null && PowercellCharger.HasPowerCells())
        //{
        //    reason = AuxPatchers.PowerStorageNotEmpty();
        //    return false;
        //}
        reason = string.Empty;
        return true;
    }

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
            _audio?.Play();
        }
        else
        {
            _audio?.Stop();
        }
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new PowerStorageDataEntry();
        }

        var save = _savedData as PowerStorageDataEntry;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager?.SaveTemplate() ?? new();

        newSaveData.Data.Add(save);
    }



    private bool IsCharging()
    {
        return powerSource != null && _allowedToCharge && powerSource.power < powerSource.maxPower;
    }

    public void TakePower()
    {
        if (!IsCharging()) return;

        _amountRemain = Plugin.Configuration.PowerStoragePowerDrainPerSecond;

        foreach (var iPowerInterface in CachedHabitatManager.GetSubRoot().powerRelay.inboundPowerSources)
        {
            MonoBehaviour mb = iPowerInterface as MonoBehaviour;

            if (mb.gameObject.name.StartsWith("PowerStorage"))
            {
                //QuickLogger.Debug("PowerStorage Found Skipping", true);
                continue;
            }

            ChargeBySource(iPowerInterface);
            //QuickLogger.Debug($"Amount Request Left Source: {_amountRemain}", true);

            if (Mathf.Approximately(_amountRemain, 0))
            {
                break;
            }

            ChargeByRelay(iPowerInterface);
            //QuickLogger.Debug($"Amount Request Left Relay: {_amountRemain}", true);
            if (Mathf.Approximately(_amountRemain, 0))
            {
                break;
            }
        }
    }

    private void ChargeByRelay(IPowerInterface iPowerInterface)
    {
        if (iPowerInterface is PowerRelay relay)
        {
            if (relay.ConsumeEnergy(_amountRemain, out float amountConsumed))
            {
                _amountRemain -= amountConsumed;
                powerSource.power = Mathf.Clamp(powerSource.power + amountConsumed, 0f, powerSource.maxPower);
                //QuickLogger.Debug($"Added {amountConsumed} to Power Storage");
            }
        }
    }

    private void ChargeBySource(IPowerInterface iPowerInterface)
    {
        if (iPowerInterface is PowerSource relay)
        {
            if (relay.ConsumeEnergy(_amountRemain, out float amountConsumed))
            {
                _amountRemain -= amountConsumed;
                powerSource.power = Mathf.Clamp(powerSource.power + amountConsumed, 0f, powerSource.maxPower);
                QuickLogger.Debug($"Added {amountConsumed} to Power Storage");
            }
        }
    }

    public float GetMaxPower()
    {
        return powerSource?.maxPower ?? 0f;
    }

    public float GetStoredPower()
    {
        return powerSource?.power ?? 0f;
    }
}
