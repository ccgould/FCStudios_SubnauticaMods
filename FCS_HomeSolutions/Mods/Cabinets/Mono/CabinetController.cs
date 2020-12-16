using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
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
        private ColorManager _colorManager;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.CabinetTabID, Mod.ModName);
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
                _colorManager.ChangeColor(_saveData.Color.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryColor.Vector4ToColor(),ColorTargetMode.Secondary);
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
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial);
            }

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponentInChildren<StorageContainer>();
                _storageContainer.container.Resize(3,6);
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.AlienChiefFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.AlienChiefFriendly}");
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
            _saveData.Color = _colorManager.GetColor().ColorToVector4();
            _saveData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();

            newSaveData.CabinetDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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