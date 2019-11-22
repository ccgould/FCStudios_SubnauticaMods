using AE.IntraBaseTeleporter.Buildables;
using AE.IntraBaseTeleporter.Configuration;
using AE.IntraBaseTeleporter.Managers;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.IntraBaseTeleporter.Mono
{
    internal class BaseTeleporterController: FCSController, IRenameNameTarget
    {
        private Constructable _buildable;
        private string _prefabIdentifier;
        private SaveDataEntry _saveData;
        private bool _runStartUpOnEnable;
        private readonly GameObject CubePrefab = CraftData.GetPrefabForTechType(TechType.PrecursorIonCrystal);
        private SaveDataEntry _data;
        private bool _fromSave;
        public BaseManager Manager { get; set; }
        public SubRoot SubRoot { get; set; }
        public ColorManager ColorManager { get; private set; }
        public override bool IsInitialized { get; set; }
        public override bool IsConstructed => _buildable != null && _buildable.constructed;
        public TeleportManager TeleportManager { get; private set; }
        public NameController NameController { get; set; }
        public AnimationManager AnimationManager { get; private set; }
        public BTDisplayManager DisplayManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public BTPowerManager PowerManager { get; private set; }

        public void Save(SaveData saveData)
        {
            var id = _prefabIdentifier;

            if (_saveData == null)
            {
                _saveData = new SaveDataEntry();
            }
            _saveData.Id = id;
            _saveData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _saveData.UnitName = NameController.GetCurrentName();
            saveData.Entries.Add(_saveData);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (DisplayManager != null)
            {
                DisplayManager.Setup(this);
                _runStartUpOnEnable = false;
            }

            CreateDisplayedIonCube();

            if (_data == null)
            {
                ReadySaveData();
            }
            
            if (_fromSave)
            {
                ColorManager.ChangeColor(_data.BodyColor.Vector4ToColor());
                NameController.SetCurrentName(_data.UnitName);
                QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                _fromSave = false;
            }
        }
        
        public override void Initialize()
        {
            _prefabIdentifier = GetComponent<PrefabIdentifier>().Id ?? GetComponentInParent<PrefabIdentifier>().Id;
            if (_buildable == null)
                _buildable = GetComponentInParent<Constructable>() ?? GetComponent<Constructable>();

            if (ColorManager == null)
                ColorManager = new ColorManager();

            ColorManager.Initialize(gameObject, BaseTeleporterBuildable.BodyMaterial);

            if (TeleportManager == null)
                TeleportManager = gameObject.GetComponent<TeleportManager>();

            TeleportManager.Initialize(this);

            if (NameController == null)
                NameController = new NameController();


            if (AnimationManager == null)
                AnimationManager = gameObject.GetComponent<AnimationManager>();

            if (DisplayManager == null)
                DisplayManager = gameObject.GetComponent<BTDisplayManager>();

            if (SubRoot == null)
                SubRoot = GetComponentInParent<SubRoot>();
            
            if (Manager == null)
                Manager = BaseManager.FindManager(SubRoot);

            if (AudioManager == null)
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            AudioManager.LoadFModAssets("/env/use_teleporter_use_loop","use_teleporter_use_loop");

            if (PowerManager == null)
                PowerManager = new BTPowerManager(this);


            DisplayManager.Setup(this);

            NameController.Initialize(this, BaseTeleporterBuildable.Submit(), Mod.FriendlyName);
            NameController.SetCurrentName(GetNewName(),DisplayManager.GetNameTextBox());
            NameController.OnLabelChanged += OnLabelChanged;
            Manager.OnBaseUnitsChanged += OnBaseUnitsChanged;
            
            AddToManager();

            IsInitialized = true;
        }

        private void OnLabelChanged(string obj)
        {
            Manager.UpdateUnits();
            DisplayManager.SetDisplay(GetName());
        }

        private void OnBaseUnitsChanged()
        {
            if(DisplayManager != null)
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

                    if (DisplayManager != null)
                    {
                        DisplayManager.Setup(this);
                        _runStartUpOnEnable = false;
                    }

                    CreateDisplayedIonCube();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.SaveMod();
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_data == null)
            {
                ReadySaveData();
            }

            _fromSave = true;
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetSaveData(id);
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
                QuickLogger.Error<BaseTeleporterController>("SubRoot returned null");
                return;
            }

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddBaseUnit(this);
            QuickLogger.Debug($"{Mod.FriendlyName} has been connected", true);
        }

        private void CreateDisplayedIonCube()
        {
            GameObject ionSlot = gameObject.FindChild("model")
                .FindChild("ion_cube_placeholder")?.gameObject;

            if (ionSlot != null)
            {
                QuickLogger.Debug("Ion Cube Display Object Created", true);
                var displayedIonCube = GameObject.Instantiate<GameObject>(CubePrefab);
                Pickupable pickupable = displayedIonCube.GetComponent<Pickupable>();
                pickupable.isPickupable = false;
                pickupable.destroyOnDeath = true;

                displayedIonCube.transform.SetParent(ionSlot.transform);
                displayedIonCube.transform.localPosition = new Vector3(0f, 0.0f, 0f);
                displayedIonCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //displayedIonCube.transform.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                QuickLogger.Error("Cannot Find IonCube in the prefab");
            }
        }

        public override void UpdateScreen()
        {
            OnBaseUnitsChanged();
        }

        private void OnDestroy()
        {
            Manager?.RemoveBaseUnit(this);
        }
    }
}
