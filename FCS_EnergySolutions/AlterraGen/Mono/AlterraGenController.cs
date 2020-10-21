using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.AlterraGen.Buildables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.AlterraGen.Mono
{
    internal class AlterraGenController : FcsDevice
    {
        private SaveDataEntry _savedData;
        internal bool IsFromSave { get; private set; }
    
        private string _prefabID;
        private bool _runStartUpOnEnable;
        private GameObject _xBubbles;


        internal AlterraGenPowerManager PowerManager { get; private set; }
        internal ColorManager ColorManager { get; set; }
        internal AnimationManager AnimationManager { get; set; }
        internal AlterraGenDisplayManager DisplayManager { get; private set; }
        public DumpContainer DumpContainer { get; private set; }

        #region Unity Methods

        private void OnEnable()
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

                    ColorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor());
                    PowerManager.LoadFromSave(_savedData);
                    IsVisible = _savedData.IsVisible;
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {

        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<AlterraGenPowerManager>();
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,"AlterraGen Receptacle",PowerManager,4,4);
            }

            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraGenModTabID);


            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<AlterraGenDisplayManager>();
                DisplayManager.Setup(this);
            }

            _xBubbles = GameObjectHelpers.FindGameObject(gameObject, "xBubbles");
            
            IsInitialized = true;
        }

        internal void SetXBubbles(bool value)
        {
            if (_xBubbles != null)
            {
                _xBubbles.SetActive(value);
            }
        }

        public override string GetPrefabID()
        {
            if (!string.IsNullOrEmpty(_prefabID)) return _prefabID;

            var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();

            if (id != null)
            {
                _prefabID = id.Id;
            }

            return _prefabID;
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

        #region IProtoEventListener


        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        internal void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.ID}",true);

            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.Storage = PowerManager.GetItemsWithin();
            _savedData.ToConsume = PowerManager.GetToConsume();
            _savedData.PowerState = PowerManager.PowerState;
            _savedData.StoredPower = PowerManager.GetStoredPower();
            _savedData.Power = PowerManager.GetPowerSourcePower();
            _savedData.IsVisible = IsVisible;
            newSaveData.Entries.Add(_savedData);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            IsFromSave = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {_prefabID}");
                Mod.Save();
                QuickLogger.Info($"Saved {_prefabID}");
            }
        }

        #endregion

    }
}
