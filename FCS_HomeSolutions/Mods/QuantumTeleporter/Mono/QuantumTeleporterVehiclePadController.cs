using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCSCommon.Utilities;
using UnityEngine;



namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal class QuantumTeleporterVehiclePadController : FcsDevice, IFCSSave<SaveData>, IQuantumTeleporter
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private QuantumTeleporterVehiclePadDataEntry _saveData;
        private string _teleportingVehiclePrefabID;
        private GameObject _seamothPoint;
        private GameObject _prawnsuitPoint;
        private QTPTriggerHandler _trigger;
        public override bool IsOperational => IsInitialized && IsConstructed && !GetIsOccupied();

        private bool GetIsOccupied()
        {
            return _trigger?.IsOccupied ?? true;
        }

        public IQTPower PowerManager { get; set; }


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, QuantumTeleporterVehiclePadBuildable.QuantumTeleporterVehiclePadTabID, Mod.ModPackID);

        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                if (_saveData != null)
                {
                    _teleportingVehiclePrefabID = _saveData.OccupiedVehicleID;
                    _colorManager.LoadTemplate(_saveData.ColorTemplate);
                }

                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _saveData = Mod.GetQuantumTeleporterVehiclePadSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (PowerManager == null)
            {
                var manager = gameObject.AddComponent<QTVehiclePadPowerManager>();
                manager.Initialize();
                PowerManager = manager;
            }

            _trigger = GameObjectHelpers.FindGameObject(gameObject, "Trigger").AddComponent<QTPTriggerHandler>();
            _seamothPoint = GameObjectHelpers.FindGameObject(gameObject, "Seamoth_Point");
            _prawnsuitPoint = GameObjectHelpers.FindGameObject(gameObject, "PrawnSuit_Point");

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.green);
            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                Mod.Save(serializer);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new QuantumTeleporterVehiclePadDataEntry();
            }

            _saveData.Id = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();

            newSaveData.QuantumTeleporterVehiclePadDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public Transform GetTarget(TeleportItemType senderType,string senderID)
        {
            switch (senderType)
            {
                case TeleportItemType.Seamoth:
                    _teleportingVehiclePrefabID = senderID;
                    return _seamothPoint.transform;
                case TeleportItemType.Exosuit:
                    _teleportingVehiclePrefabID = senderID;
                    return _prawnsuitPoint.transform;
                case TeleportItemType.Player:
                    _teleportingVehiclePrefabID = string.Empty;
                    return null;
            }

            _teleportingVehiclePrefabID = string.Empty;
            return null;
        }
    }

    internal class QTPTriggerHandler : MonoBehaviour
    {
        private string _colliderObjectName;
        public bool IsOccupied { get; private set; }


        private void OnTriggerStay(Collider collider)
        {
            if (IsOccupied || collider?.gameObject == null) return;

            GameObject colliderGo = UWE.Utils.GetEntityRoot(collider.gameObject);
            var vehicle = colliderGo?.GetComponent<Vehicle>();
            
            if (vehicle == null) return;
            IsOccupied = true;

            _colliderObjectName = vehicle.vehicleName;
        }

        private string GetVehicleName()
        {
            return _colliderObjectName;
        }

        private void OnTriggerExit(Collider collider)
        {
            _colliderObjectName = string.Empty;
            IsOccupied = false;
        }
    }
}