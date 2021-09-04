using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.PowerStorage.Enums;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.UniversalCharger.Mono
{
    internal class UniversalChargerController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;

        private UniversalChargerDataEntry _savedData;
        private GameObject _handTarget;
        private GameObject _ui;
        private FCSMessageBox _messageBox;
        private Toggle _powercellToggle;
        private Toggle _batteryToggle;
        private ToggleGroup _toggleGroup;
        private Toggle _previousToggle;

        internal PowercellCharger PowercellCharger { get; private set; }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.PowerStorageTabID, Mod.ModPackID);
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
                    LoadChargerFromSave();

                    _runStartUpOnEnable = false;
                }
            }
        }

        private void LoadChargerFromSave()
        {
            if (_savedData.Mode == PowerChargerMode.Battery)
            {
                _batteryToggle.SetIsOnWithoutNotify(true);
                _powercellToggle.SetIsOnWithoutNotify(false);
                _previousToggle = _batteryToggle;
            }
            else
            {
                _batteryToggle.SetIsOnWithoutNotify(false);
                _powercellToggle.SetIsOnWithoutNotify(true);
                _previousToggle = _powercellToggle;
            }
            
            PowercellCharger.SetMode(_savedData.Mode);
            PowercellCharger.Load(_savedData.ChargerData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetUniversalChargerSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial, ModelPrefab.EmissiveControllerMaterial);
            }

            _handTarget = GameObjectHelpers.FindGameObject(gameObject, "HandTarget");

            GameObjectHelpers.FindGameObject(gameObject, "EquipmentRoot").gameObject.EnsureComponent<ChildObjectIdentifier>().classId = Mod.UniversalChargerClassName;

            if (PowercellCharger == null)
            {
                _handTarget.SetActive(false);
                PowercellCharger = _handTarget.AddComponent<PowercellCharger>();
                PowercellCharger.Initialize(this, GameObjectHelpers.FindGameObject(gameObject, "PowercellMeters").GetChildren());
                _handTarget.SetActive(true);
            }

            var sc = gameObject.GetComponents<StorageContainer>();

            foreach (var storageContainer in sc)
            {
                storageContainer.enabled = false;
            }

            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();

            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
            
            _batteryToggle = GameObjectHelpers.FindGameObject(gameObject, "BatteryToggle").GetComponent<Toggle>();
            
            _batteryToggle.onValueChanged.AddListener(OnModeChangeClicked);

            _powercellToggle = GameObjectHelpers.FindGameObject(gameObject, "PowercellToggle").GetComponent<Toggle>();

            _powercellToggle.onValueChanged.AddListener(OnModeChangeClicked);

            IsInitialized = true;
        }

        private void OnModeChangeClicked(bool value)
        {
            if (PowercellCharger.HasRechargeables())
            {
                if (_toggleGroup.ActiveToggles().ElementAt(0) != _previousToggle)
                {
                    _previousToggle.SetIsOnWithoutNotify(true);
                }
                //if (_toggleGroup.ActiveToggles().ElementAt(0).name.StartsWith("BatteryToggle"))
                //{
                //    _powercellToggle.SetIsOnWithoutNotify(true);
                //    _batteryToggle.SetIsOnWithoutNotify(false);
                //}
                //else
                //{
                //    _powercellToggle.SetIsOnWithoutNotify(false);
                //    _batteryToggle.SetIsOnWithoutNotify(true);
                //}
                _messageBox.Show(AuxPatchers.UniversalChargerCannotChangeMode(), FCSMessageButton.OK, null);
                return;
            }


            if (_toggleGroup.ActiveToggles().Any())
            {
                PowercellCharger.SetMode(_toggleGroup.ActiveToggles().ElementAt(0).name.StartsWith("BatteryToggle")
                    ? PowerChargerMode.Battery
                    : PowerChargerMode.Powercell);
            }

            _previousToggle = _toggleGroup.ActiveToggles().ElementAt(0);
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
            if (PowercellCharger != null && !PowercellCharger.CanDeconstruct(out reason))
            {
                return false;
            }

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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new UniversalChargerDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.ChargerData = PowercellCharger.Save();
            _savedData.Mode = PowercellCharger.GetMode();
            newSaveData.UniversalChargerEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
            base.OnHandHover(hand);
            var data = new string[] { };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public GameObject GetUI()
        {
            if (_ui == null)
            {
               _ui = GameObjectHelpers.FindGameObject(gameObject, "uiDummy");
            }

            return _ui;
        }
    }
}
