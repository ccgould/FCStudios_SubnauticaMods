using AE.SeaCooker.Buildable;
using AE.SeaCooker.Configuration;
using AE.SeaCooker.Helpers;
using AE.SeaCooker.Managers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System.Collections;
using UnityEngine;

namespace AE.SeaCooker.Mono
{
    internal class SeaCookerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        internal SCDisplayManager DisplayManager { get; set; }
        internal bool IsConstructed { get; private set; }
        internal GasManager GasManager { get; private set; }
        internal SCStorageManager StorageManager { get; private set; }
        internal PowerManager PowerManager { get; private set; }
        internal AnimationManager AnimationManager { get; set; }
        internal FoodManager FoodManager { get; private set; }

        private SaveDataEntry _saveData;
        private bool _initialized;
        private int _isRunning;

        private void Awake()
        {
            _isRunning = Animator.StringToHash("IsRunning");
        }
        private void Update()
        {
            PowerManager?.ConsumePower();
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (StorageManager.CanDeconstruct()) return true;
            reason = SeaCookerBuildable.UnityNotEmpty();
            return false;

        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (!constructed) return;

            if (!_initialized)
            {
                Initialize();
            }

            //AnimationManager.SetBoolHash(IsRunningHash, true);
        }

        private void Initialize()
        {
            TechTypeHelpers.Initialize();

            if (FoodManager == null)
            {
                FoodManager = gameObject.AddComponent<FoodManager>();
                FoodManager.Initialize(this);
            }

            if (StorageManager == null)
            {
                StorageManager = new SCStorageManager();
                StorageManager.Initialize(this);
            }

            if (GasManager == null)
            {
                GasManager = new GasManager();
                GasManager.Initialize(this);
            }

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(this);
                StartCoroutine(UpdatePowerState());
            }

            AnimationManager = gameObject.GetComponent<AnimationManager>();

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<SCDisplayManager>();
                DisplayManager.Setup(this);
            }

            if (ColorManager == null)
            {
                ColorManager = new ColorManager();
                ColorManager.Initialize(this, SeaCookerBuildable.BodyMaterial);

            }

            _initialized = true;
        }

        public ColorManager ColorManager { get; private set; }

        internal void UpdateIsRunning(bool value = true)
        {
            AnimationManager.SetBoolHash(_isRunning, value);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Info($"Loading {Mod.FriendlyName}");
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetSaveData(id);

            if (data == null)
            {
                QuickLogger.Info($"No save found for PrefabId {id}");
                return;
            }


            GasManager.SetTankLevel(data.FuelLevel);
            GasManager.CurrentFuel = data.TankType;
            GasManager.SetEquipment();
            ColorManager.SetCurrentBodyColor(data.BodyColor.Vector4ToColor());
            StorageManager.LoadExportContainer(data.Export);
            StorageManager.LoadInputContainer(data.Input);
            StorageManager.SetExportToSeabreeze(data.ExportToSeaBreeze);
            QuickLogger.Info($"Loaded {Mod.FriendlyName}");
        }

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new SaveDataEntry();
            }
            _saveData.ID = id;
            _saveData.FuelLevel = GasManager.GetTankLevel();
            _saveData.TankType = GasManager.CurrentFuel;
            _saveData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _saveData.Export = StorageManager.GetExportContainer();
            _saveData.Input = StorageManager.GetInputContainer();
            _saveData.ExportToSeaBreeze = StorageManager.GetExportToSeabreeze();
            saveData.Entries.Add(_saveData);
        }

        private IEnumerator UpdatePowerState()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                PowerManager.UpdatePowerState();
            }
        }
    }
}
