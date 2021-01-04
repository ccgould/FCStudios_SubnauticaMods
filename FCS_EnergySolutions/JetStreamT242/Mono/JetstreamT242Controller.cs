using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.JetStreamT242.Mono
{
    internal class JetStreamT242Controller : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private JetStreamT242PowerManager _powerManager;
        private JetStreamT242DataEntry _savedData;
        private MotorHandler _motor;
        private RotorHandler _tilter;
        private RotorHandler _rotor;
        private Text _unitID;
        
        #region Unity Methods       

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.JetStreamT242TabID, Mod.ModName);
            _unitID.text = $"UnitID: {UnitID}";
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

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                    _powerManager.LoadFromSave(_savedData);
                    _motor.LoadSave(_savedData);
                    _tilter.LoadSave(_savedData);

                    if (_savedData.IsIncreasing && IsUnderWater())
                    {
                        ChangeStatusLight();
                        _rotor.ResetToMag();
                        _rotor.Run();
                    }
                    else
                    {
                        DeActivateTurbine();
                    }

                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetJetStreamT242SaveData(GetPrefabID());
        }
        
        #endregion

        private bool IsUpright()
        {
            if (Mathf.Approximately(transform.up.y, 1f))
            {
                return true;
            }

            return false;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            _tilter = GameObjectHelpers.FindGameObject(gameObject, "Tilter").AddComponent<RotorHandler>();
            _tilter.SetTargetAxis(TargetAxis.Y);

            _rotor = GameObjectHelpers.FindGameObject(gameObject, "Rotor").AddComponent<RotorHandler>();
            _rotor.SetTargetAxis(TargetAxis.Y);
            
            _motor = GameObjectHelpers.FindGameObject(gameObject, "Blades").AddComponent<MotorHandler>();
            _motor.Initialize();

            if (_powerManager == null)
            {
                _powerManager = gameObject.AddComponent<JetStreamT242PowerManager>();
                _powerManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            _unitID = GameObjectHelpers.FindGameObject(gameObject, "UNITID").GetComponent<Text>();
            DeActivateTurbine();
            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissiveBControllerMaterial, gameObject,5);
            IsInitialized = true;
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
            }
        }

        internal bool IsUnderWater()
        {
            return GetDepth() > 3.0f;
        }

        internal float GetDepth()
        {
#if SUBNAUTICA
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
#elif BELOWZERO
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
#endif
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized|| !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new JetStreamT242DataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            _powerManager.Save(_savedData);
            _motor.Save(_savedData);
            _tilter.Save(_savedData);
            _savedData.BaseId = BaseId;
            newSaveData.MarineTurbineEntries.Add(_savedData);
        }

        public float GetCurrentSpeed()
        {
            return _motor.GetSpeed();
        }

        public void ActivateTurbine()
        {
            _tilter.ChangePosition(0,false);
            _tilter.Run();

            _rotor.ResetToMag();
            _rotor.Run();

            _motor.SpeedByPass(_powerManager.GetBiomeData());
            _motor.Run();

            ChangeStatusLight();

            _powerManager.ChangePowerState(FCSPowerStates.Powered);
        }

        public void DeActivateTurbine()
        {
            _tilter.ChangePosition(-90);
            _tilter.Stop();
            _motor.Stop();
            ChangeStatusLight(false);
            _powerManager.ChangePowerState(FCSPowerStates.Tripped);
        }

        private void ChangeStatusLight(bool isOperational = true)
        {
            if (isOperational)
            {
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.DecalMaterial, gameObject, Color.cyan);
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissiveBControllerMaterial, gameObject, Color.cyan);
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissiveControllerMaterial, gameObject, Color.cyan);
            }
            else
            {
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.DecalMaterial, gameObject, Color.red);
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissiveBControllerMaterial, gameObject, Color.red);
                MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissiveControllerMaterial, gameObject, Color.red);
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override float GetPowerProducing()
        {
            return _powerManager.GetEnergyProducing();
        }

        public override float GetMaxPower()
        {
            return _powerManager.GetMaxPower();
        }

        public override float GetStoredPower()
        {
            return _powerManager.GetStoredPower();
        }
    }
}
