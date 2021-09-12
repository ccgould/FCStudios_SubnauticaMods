using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Cabinets.Mono
{
    internal class CabinetController : FcsDevice, IHandTarget, IFCSSave<SaveData>
    {
        private StorageContainer _storageContainer;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private CabinetDataEntry _saveData;
        private LabelController _labelController;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Cabinet1Buildable.CabinetTabID, Mod.ModPackID);
        }

        private void Update()
        {
            if (_storageContainer != null && _labelController != null)
            {
                _storageContainer.enabled = !_labelController.IsHovered;
            }
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
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                _labelController?.SetText(!string.IsNullOrWhiteSpace(_saveData.Label) ? _saveData.Label : "Locker");

                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetCabinetSaveData(id);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!base.enabled)
                return;

            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
#if BELOWZERO
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "OpenLocker");
#else
            HandReticle.main.SetInteractText("OpenLocker");
#endif
        }
        
        public void OnHandClick(GUIHand hand)
        {
            if (this._storageContainer != null)
                this._storageContainer.OnHandClick(hand);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponentInChildren<StorageContainer>();
                _storageContainer.container.Resize(3,6);
            }

            _labelController = GameObjectHelpers.FindGameObject(gameObject, "Label")?.AddComponent<LabelController>();
            _labelController?.Initialize();
            _labelController?.SetText("Locker");

            MaterialHelpers.ChangeMaterialColor(AlterraHub.BaseDecalsEmissiveController,gameObject,Color.cyan);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject,2f);
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
                _saveData = new CabinetDataEntry();
            }
            _saveData.Id = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            _saveData.Label = _labelController?.GetText();
            newSaveData.CabinetDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_storageContainer == null)
            {
                reason = string.Empty;
                return true;
            }
            return _storageContainer.CanDeconstruct(out reason);
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
    }
}