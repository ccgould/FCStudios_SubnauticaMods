using System.Collections.Generic;
using System.Globalization;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;

namespace DataStorageSolutions.Mono
{
    internal class DSSAntennaController : DataStorageSolutionsController, IBaseAntenna, IHandTarget
    {
        private bool _isContructed;
        private PowerRelay _powerRelay;
        private float _energyToConsume;
        private bool _showConsumption = true;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private float _amountConsumed;

        public override bool IsConstructed => _isContructed;
        public SubRoot SubRoot { get; private set; }
        public BaseManager Manager { get; set; }
        public TechType TechType => GetTechType();
        public ColorManager ColorManager { get; set; }
        internal NameController NameController { get; private set; }
        public PowerManager PowerManager { get; private set; }

        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    QuickLogger.Debug("Trying to set base name");
                    
                    if (string.IsNullOrEmpty(_savedData.AntennaName))
                    {
                        QuickLogger.Debug("Base Name is null or empty");
                        var defaultName = Manager.GetDefaultName();
                        SetBaseName(defaultName);
                    }
                    else
                    {
                        SetBaseName(_savedData.AntennaName);
                    }
                    
                    QuickLogger.Debug("Trying to set antenna color");


                    if (_savedData?.AntennaBodyColor != null)
                    {
                        ColorManager.SetMaskColorFromSave(_savedData.AntennaBodyColor.Vector4ToColor());
                    }
                }
            }
        }

        private void SetBaseName(string defaultName)
        {
            QuickLogger.Debug("Got Default Name");
            NameController.SetCurrentName(defaultName);
            QuickLogger.Debug("Setting Name Controller");
            Manager.SetBaseName(defaultName);
            QuickLogger.Debug("Setting Base Name");
        }

        private void OnLabelChangedMethod(string newName, NameController controller)
        {
            Manager.SetBaseName(newName);
            Mod.OnBaseUpdate?.Invoke();
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
        }

        private void Update()
        {
            FindSubRoot();

            if (IsConstructed && PowerManager != null)
            {
                PowerManager?.UpdatePowerState();
                PowerManager?.ConsumePower();
                QuickLogger.Debug($"Antenna {GetPrefabIDString()} Power Usage: {PowerManager.GetPowerUsage()}");
            }
        }

        private void OnDestroy()
        {
            BaseManager.RemoveAntenna(this);
            Mod.OnAntennaBuilt?.Invoke(false);
        }

        private void FindSubRoot()
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }
        }

        private void GetPowerRelay()
        {
            if (SubRoot != null)
            {
                _powerRelay = SubRoot.powerRelay;
            }
        }

        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabID = id != null ? id.Id : string.Empty;
            }

            return _prefabID;
        }

        internal void ToggleShowConsumption()
        {
            _showConsumption = !true;
        }

        public override void Initialize()
        {
            GetData();
            
            QuickLogger.Debug($"Manager Count: {BaseManager.BaseAntennas.Count}");

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(gameObject, QPatch.Configuration.Config.AntennaPowerUsage);
                PowerManager.OnPowerUpdate += OnPowerUpdate;
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            if (NameController == null)
            {
                NameController = gameObject.EnsureComponent<NameController>();
                NameController.Initialize(AuxPatchers.Submit(), Mod.AntennaFriendlyName);
                NameController.OnLabelChanged += OnLabelChangedMethod;

                if (Manager != null)
                {
                    QuickLogger.Debug("Trying to set default name");
                    SetBaseName(Manager.GetDefaultName());
                }
            }

            GetPowerRelay();
            
            Manager?.AddAntenna(this);

            Mod.OnAntennaBuilt?.Invoke(true);

            IsInitialized = true;
        }

        private void OnPowerUpdate(FCSPowerStates obj)
        {
            Mod.OnBaseUpdate?.Invoke();
        }

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.AntennaBodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _savedData.AntennaName = Manager.GetBaseName();
            newSaveData.Entries.Add(_savedData);
        }

        internal List<DSSRackController> GetData()
        {
            if (!_isContructed) return null;

            FindSubRoot();

            Manager = Manager ?? BaseManager.FindManager(SubRoot);

            return Manager.BaseUnits;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabIDString();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            _fromSave = true;
        }
        
        public override void OnConstructedChanged(bool constructed)
        {
            _isContructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        string IBaseAntenna.GetName()
        {
            return Manager.GetBaseName();
        }

        public void ChangeBaseName()
        {
            NameController.Show();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            var state = PowerManager?.GetPowerState() == FCSPowerStates.Powered ? "On" : "Off";
#if SUBNAUTICA
            main.SetInteractTextRaw(Manager?.GetBaseName(), $"Antenna {state} || Power usage: {PowerManager?.GetPowerUsage():F1}");
#elif BELOWZERO
            main.SetText(HandReticle.TextType.Info, Manager.GetBaseName(), false);
#endif
            main.SetIcon(HandReticle.IconType.Info, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public bool IsVisible()
        {
            return IsConstructed && PowerManager.GetPowerState() == FCSPowerStates.Powered;
        }

        //private void UpdatePower()
        //{
        //    if (IsConstructed)
        //    {
        //        if (_powerRelay)
        //        {
        //            if (_powerRelay.GetPowerStatus() == PowerSystem.Status.Normal)
        //            {
        //                _powerRelay.ConsumeEnergy(powerPerSecond * updateInterval, out num);
        //            }
        //        }
        //    }
        //}
    }
}