using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashReceptacle.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.TrashReceptacle.Mono
{
    internal class TrashReceptacleController : FcsDevice, IFCSSave<SaveData>, IHandTarget, IConstructable
    {
        private bool _runStartUpOnEnable;
        private DumpContainer _dumpContainer;
        private TrashStorage _storage;
        private TrashReceptacleDataEntry _savedData;
        private bool _isFromSave;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, TrashReceptaclePatch.TrashReceptacleTabID,
                Mod.ModPackID);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

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
            }

            _runStartUpOnEnable = false;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetTrashReceptacleSaveData(id);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || Manager == null) return;
            var main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Info);

#if SUBNAUTICA
                main.SetInteractText(
#else
            main.SetTextRaw(HandReticle.TextType.Use,
#endif

                Manager.IsDeviceBuilt(TrashReceptaclePatch.TrashReceptacleTabID)
                    ? AuxPatchers.ClickToOpenRecycle(TrashReceptaclePatch.TrashReceptacleFriendly)
                    : AuxPatchers.NoRecyclerConnected());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsInitialized || Manager == null) return;

            if (Manager.IsDeviceBuilt(TrashReceptaclePatch.TrashReceptacleTabID))
            {
                _dumpContainer.OpenStorage();
            }
        }

        public override void Initialize()
        {
            Manager = BaseManager.FindManager(gameObject);


            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainer>();
            }

            if (_storage == null)
            {
                _storage = gameObject.AddComponent<TrashStorage>();
                _storage.Initialize(this, _dumpContainer);
                _dumpContainer.Initialize(transform, AuxPatchers.TrashReceptacleDumpLabel(), _storage);
            }


            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {TrashReceptaclePatch.TrashReceptacleFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {TrashReceptaclePatch.TrashReceptacleFriendly}");
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
            reason = String.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = true;
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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new TrashReceptacleDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            newSaveData.TrashReceptacleEntries.Add(_savedData);
        }
    }
}