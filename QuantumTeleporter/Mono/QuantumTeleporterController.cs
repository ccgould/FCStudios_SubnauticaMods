using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Configuration;
using QuantumTeleporter.Managers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace QuantumTeleporter.Mono
{
    internal class QuantumTeleporterController: FCSController,IRenameNameTarget
    {
        private bool _runStartUpOnEnable;
        private SaveDataEntry _data;
        private bool _fromSave;
        private string _prefabIdentifier;
        private Constructable _buildable;
        private int _spinTitle;
        private bool _isGlobal;
        private Transform _target;
        public override bool IsConstructed => _buildable != null && _buildable.constructed;
        public override bool IsInitialized { get; set; }
        public NameController NameController { get; set; }
        public AnimationManager AnimationManager { get; private set; }
        public QTDisplayManager DisplayManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public QTPowerManager PowerManager { get; private set; }
        public ColorManager ColorManager { get; private set; }

        public QTTriggerBoxManager PlayerTrigger { get; private set; }
        public SubRoot SubRoot { get; set; }
        public BaseManager Manager { get; set; }
        public QTDoorManager QTDoorManager { get; private set; }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_data == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                ColorManager.ChangeColor(_data.BodyColor.Vector4ToColor());
                NameController.SetCurrentName(_data.UnitName);
                SetIsGlobal(_data.IsGlobal);
                DisplayManager.SetTab(_data.SelectedTab);
                QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                _fromSave = false;
            }
        }

        internal void SetIsGlobal(bool dataIsGlobal)
        {
            _isGlobal = dataIsGlobal;
            DisplayManager.SetGlobalCheckBox(dataIsGlobal);
            BaseManager.UpdateGlobalTargets();
        }

        public override void Initialize()
        {

            _prefabIdentifier = GetComponent<PrefabIdentifier>().Id ?? GetComponentInParent<PrefabIdentifier>().Id;
            _spinTitle = Animator.StringToHash("SpinTitle");

            var target = gameObject.FindChild("targetPos");
            
            if (target == null)
            {
                QuickLogger.Error("Cant find trigger targetPos");
                return;
            }

            if (_buildable == null)
                _buildable = GetComponentInParent<Constructable>() ?? GetComponent<Constructable>();

            if (ColorManager == null)
                ColorManager = new ColorManager();

            ColorManager.Initialize(gameObject, QuantumTeleporterBuildable.BodyMaterial);

            TeleportManager.Initialize();

            if (NameController == null)
                NameController = new NameController();


            if (AnimationManager == null)
                AnimationManager = gameObject.GetComponent<AnimationManager>();

            if (DisplayManager == null)
                DisplayManager = gameObject.GetComponent<QTDisplayManager>();

            if (SubRoot == null)
                SubRoot = GetComponentInParent<SubRoot>();

            if (Manager == null)
                Manager = BaseManager.FindManager(SubRoot);

            if (AudioManager == null)
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            AudioManager.LoadFModAssets("/env/use_teleporter_use_loop", "use_teleporter_use_loop");

            if (PowerManager == null)
                PowerManager = new QTPowerManager(this);


            DisplayManager.Setup(this);
            
            NameController.Initialize(this, QuantumTeleporterBuildable.Submit(), Mod.FriendlyName);
            NameController.SetCurrentName(GetNewName(), DisplayManager.GetNameTextBox());
            NameController.OnLabelChanged += OnLabelChanged;
            Manager.OnBaseUnitsChanged += OnBaseUnitsChanged;
            AddToManager();

            AnimationManager.SetBoolHash(_spinTitle,true);

            if (QTDoorManager == null)
                QTDoorManager = gameObject.FindChild("model").FindChild("anims").FindChild("door").AddComponent<QTDoorManager>();
            QTDoorManager.Initalize(this);

            var trigger = target.AddComponent<QTTriggerBoxManager>();
            trigger.OnPlayerExit += QTDoorManager.OnPlayerExit;
            _target = target.transform;

            IsInitialized = true;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetSaveData(id);
        }

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_data == null)
            {
                _data = new SaveDataEntry();
            }
            _data.ID = id;
            _data.BodyColor = ColorManager.GetColor().ColorToVector4();
            _data.UnitName = NameController.GetCurrentName();
            _data.IsGlobal = _isGlobal;
            _data.SelectedTab = DisplayManager.GetSelectedTab();
            saveData.Entries.Add(_data);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override string GetPrefabIDString()
        {
            return _prefabIdentifier;
        }

        public void AddToManager(BaseManager managers = null)
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>() ?? GetComponent<SubRoot>() ?? GetComponentInChildren<SubRoot>();
            }

            if (SubRoot == null)
            {
                QuickLogger.Error<QuantumTeleporterController>("SubRoot returned null");
                return;
            }

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddBaseUnit(this);
            QuickLogger.Debug($"{Mod.FriendlyName} has been connected", true);
        }

        public override void UpdateScreen()
        {
            OnBaseUnitsChanged();
        }

        private void OnDestroy()
        {
            Manager?.RemoveBaseUnit(this);
        }

        private void OnLabelChanged(string obj, NameController nameController)
        {
            DisplayManager.SetDisplay(GetName());
        }
        
        private void OnBaseUnitsChanged()
        {
            if (DisplayManager != null)
                DisplayManager.UpdateUnits();
        }

        public override string GetName()
        {
            return NameController.GetCurrentName();
        }

        private string GetNewName()
        {
            return $"{Mod.ModName}_{Manager.BaseUnits.Count + 1}";
        }

        public bool GetIsGlobal()
        {
            return _isGlobal;
        }

        public Transform GetTarget()
        {
            return _target;
        }
    }
}
