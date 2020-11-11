using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mono.OutDoorPlanters
{
    internal class OutDoorPlanterController : FcsDevice, IFCSSave<SaveData>
    {
        private ColorManager _colorManager;
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private PlanterDataEntry _savedData;
        private GameObject _storageRootGameObject;
        private StorageContainer _storageContainer;
        private ProtobufSerializer _serializer;
        
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

                    LoadStorage();

                    _colorManager.ChangeColor(_savedData.Color.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryColor.Vector4ToColor(), ColorTargetMode.Secondary);
                    _colorManager.ChangeColor(_savedData.LUMColor.Vector4ToColor(), ColorTargetMode.Emission);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPlanterDataEntrySaveData(GetPrefabID());
        }

        public void Save(SaveData newSaveData,ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new PlanterDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Color = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.LUMColor = _colorManager.GetLumColor().ColorToVector4();
            if (serializer != null)
            {
                _savedData.Bytes = GetStorageBytes(serializer);
            }
            else
            {
                QuickLogger.DebugError("Serializer was null");
            }
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.PlanterEntries.Add(_savedData);
        }

        public override void Initialize()
        {
            _colorManager = gameObject.AddComponent<ColorManager>();
            _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial,ModelPrefab.LUMControllerMaterial);
            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.LUMControllerMaterial, gameObject, 5);
            IsInitialized = true;
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
            _serializer = serializer;
            
            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        private void LoadStorage()
        {
            GetContainer();

            ResetInventory();

            if (_savedData.Bytes != null)
            {
                if (_storageContainer.container == null)
                {
                    QuickLogger.DebugError("Storage Container is null");
                }
                else
                {
                    StorageHelper.RestoreItems(_serializer, _savedData.Bytes, _storageContainer.container);
                    QuickLogger.Debug("Loaded Storage Data", true);
                }
            }
        }

        public byte[] GetStorageBytes(ProtobufSerializer serializer)
        {
            QuickLogger.ModMessage($"Getting Storage Bytes");
            return StorageHelper.Save(serializer, GetStorageRoot());
        }

        private void GetContainer()
        {
            if(_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponent<StorageContainer>();
            }
        }

        private void ResetInventory()
        {
            var storageRoot = GetStorageRoot();
            if (_storageContainer == null || storageRoot == null) return;
            _storageContainer?.container?.Clear();
            StorageHelper.RenewIdentifier(storageRoot);
        }

        private GameObject GetStorageRoot()
        {
            if (_storageRootGameObject == null)
            {
                _storageRootGameObject = GameObjectHelpers.FindGameObject(gameObject, "StorageRoot");
            }

            return _storageRootGameObject;

        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
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

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
