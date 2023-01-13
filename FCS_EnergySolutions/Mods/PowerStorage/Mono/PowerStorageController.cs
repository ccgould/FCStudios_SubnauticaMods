﻿using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.PowerStorage.Mono
{
    internal class PowerStorageController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private PowerStorageDataEntry _savedData;
        public override bool IsOperational => IsConstructed && IsInitialized;

        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private Image _bar;
        private ParticleSystem[] _particles;
        private FMOD_CustomLoopingEmitter _audio;
        private bool _allowedToCharge;
        private BasePowerStorage _basePowerStorage;
        private float _amountRemain;

        public PowerSource PowerSource { get; private set; }
        
        

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void UpdateVisuals()
        {
            var percentage = PowerSource.power / PowerSource.maxPower;
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
                var index = Main.Configuration.TelepowerPylonTrailBrightness;
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

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.PowerStorageTabID, Mod.ModPackID);
            if (Manager != null)
            {
                _basePowerStorage = Manager.Habitat.gameObject.EnsureComponent<BasePowerStorage>();
                _basePowerStorage.Register(this);
            }

        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                    _runStartUpOnEnable = false;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _basePowerStorage?.Unregister(this);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPowerStorageSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (PowerSource == null)
            {
                PowerSource = gameObject.GetComponent<PowerSource>();
            }

            _particles = gameObject.GetComponentsInChildren<ParticleSystem>();
            _bar = GameObjectHelpers.FindGameObject(gameObject, "BarFill").GetComponent<Image>();
            _audio = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);

            InvokeRepeating(nameof(UpdateVisuals),1f,1f);
            InvokeRepeating(nameof(TakePower),1f,1f);

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

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            
            _isFromSave = true;
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

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            newSaveData.PowerStorageEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
            base.OnHandHover(hand);
            var data = new string[]
            {
                AlterraHub.PowerPerMinute((IsCharging() ? Main.Configuration.PowerStoragePowerDrainPerSecond : 0f) * 60f),
                $"Is Charging: {IsCharging()}",
                $"Storage: {Mathf.FloorToInt(PowerSource.power)} / {Mathf.FloorToInt(PowerSource.maxPower)} | Percentage: {PowerSource.power / PowerSource.maxPower:P0}"
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        private bool IsCharging()
        {
            return PowerSource != null && _allowedToCharge && PowerSource.power < PowerSource.maxPower;
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public void TakePower()
        {
            if (!IsCharging()) return;

            _amountRemain = Main.Configuration.PowerStoragePowerDrainPerSecond;

            foreach (var iPowerInterface in Manager.GetPowerRelay().inboundPowerSources)
            {
                MonoBehaviour mb = iPowerInterface as MonoBehaviour;

                if (mb.gameObject.name.StartsWith("PowerStorage"))
                {
                    QuickLogger.Debug("PowerStorage Found Skipping", true);
                    continue;
                }

                ChargeBySource(iPowerInterface);
                QuickLogger.Debug($"Amount Request Left Source: {_amountRemain}", true);

                if (Mathf.Approximately(_amountRemain, 0))
                {
                    break;
                }

                ChargeByRelay(iPowerInterface);
                QuickLogger.Debug($"Amount Request Left Relay: {_amountRemain}", true);
                if (Mathf.Approximately(_amountRemain,0))
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
                    PowerSource.power = Mathf.Clamp(PowerSource.power + amountConsumed, 0f, PowerSource.maxPower);
                    QuickLogger.Debug($"Added {amountConsumed} to Power Storage");
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
                    PowerSource.power = Mathf.Clamp(PowerSource.power + amountConsumed, 0f, PowerSource.maxPower);
                    QuickLogger.Debug($"Added {amountConsumed} to Power Storage");
                }
            }
        }

        public override float GetMaxPower()
        {
            return PowerSource?.maxPower ?? 0f;
        }

        public override float GetStoredPower()
        {
            return PowerSource?.power ?? 0f;
        }
    }

    internal class BasePowerStorage : MonoBehaviour
    {
        private readonly HashSet<PowerStorageController> _registeredDevices = new HashSet<PowerStorageController>();
        private bool _isCharging;
        private float _baseCapacity;
        private float _basePower;
        private bool _showInLog;

        private void Start()
        {
            Manager = BaseManager.FindManager(gameObject);
        }

        public BaseManager Manager { get; set; }

        private float CalculatePowerPercentage()
        {
            //Get Capactity
            _baseCapacity = CalculateBasePowerCapacity();
            _basePower = CalculateBasePower();
            
            if (_basePower <= 0 || _baseCapacity <= 0) return 0;

            return (_basePower / _baseCapacity) * 100;
        }

        private float CalculateBasePowerCapacity()
        {
            if (BaseManager.FindManager(Player.main.currentSub) == Manager && _showInLog)
            {
                QuickLogger.Debug($"CalculateBasePowerCapacity:", true);
                QuickLogger.Debug($"Base Capacity {Manager.GetBasePowerCapacity()}", true);
                QuickLogger.Debug($"Power Storage Capacity {TotalPowerStorageCapacityAtBase()}", true);
                QuickLogger.Debug($"Capacity Result {Manager.GetBasePowerCapacity() - TotalPowerStorageCapacityAtBase()}", true);
            }
            return Manager.GetBasePowerCapacity() - TotalPowerStorageCapacityAtBase();
        }

        private float TotalPowerStorageCapacityAtBase()
        {
            float total = 0f;
            foreach (PowerStorageController controller in _registeredDevices)
            {
                total += controller.GetMaxPower();
            }

            return total;
        }

        public float CalculateBasePower()
        {
            if (BaseManager.FindManager(Player.main.currentSub) == Manager && _showInLog)
            {
                QuickLogger.Debug($"CalculateBasePower:",true);
                QuickLogger.Debug($"Base Power {Manager.GetPower()}",true);
                QuickLogger.Debug($"Power Storage Power {TotalPowerStoragePowerAtBase()}",true);
                QuickLogger.Debug($"Power Result {Manager.GetPower() - TotalPowerStoragePowerAtBase()}",true);
            }
            return Manager.GetPower() - TotalPowerStoragePowerAtBase();
        }

        private float TotalPowerStoragePowerAtBase()
        {
            float total = 0f;
            
            foreach (PowerStorageController controller in _registeredDevices)
            {
                total += controller.GetStoredPower();
            }

            return total;
        }

        private void Update()
        {
            foreach (PowerStorageController controller in _registeredDevices)
            {
                if (CalculatePowerPercentage() > 40)
                {
                    controller.Charge();
                    _isCharging = true;
                }
                else
                {
                    controller.Discharge();
                    _isCharging = false;
                }
            }
        }

        public void Register(PowerStorageController controller)
        {
            if (!_registeredDevices.Contains(controller))
            {
                _registeredDevices.Add(controller);
            }
        }

        public void Unregister(PowerStorageController controller)
        {
            _registeredDevices.Remove(controller);
        }
    }
}
