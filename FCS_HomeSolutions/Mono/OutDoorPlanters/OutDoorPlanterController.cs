using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mono.OutDoorPlanters
{
    internal class OutDoorPlanterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private PlanterDataEntry _savedData;
        private GameObject _storageRootGameObject;
        private StorageContainer _storageContainer;
        private ProtobufSerializer _serializer;
        private Planter _planter;
        private List<Plantable> _trackedPlants = new List<Plantable>();

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.SmartPlanterPotTabID, Mod.ModName);
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

                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                    _colorManager.ChangeColor(_savedData.Lum.Vector4ToColor(), ColorTargetMode.Emission);
#if SUBNAUTICA_STABLE
                    LoadStorage();
#else
StartCoroutine(LoadStorage());
#endif
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
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.Lum = _colorManager.GetLumColor().ColorToVector4();
            _savedData.PlantAges = GetPlantAges();
            
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

        private IEnumerable<PlantData> GetPlantAges()
        {
            foreach (Plantable plant in _trackedPlants)
            {
                yield return new PlantData(plant.plantAge,plant.GetSlotID());
            }
        }

        public override void Initialize()
        {
            _colorManager = gameObject.AddComponent<ColorManager>();
            _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial,ModelPrefab.EmissionControllerMaterial);
            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject, 5);
            _planter = gameObject.GetComponent<Planter>();

            IsInitialized = true;
            _planter.storageContainer.container.onAddItem += OnPlanterAddItem;
            _planter.storageContainer.container.onRemoveItem += OnPlanterRemoveItem;
        }

        private void OnPlanterRemoveItem(InventoryItem item)
        {
            _trackedPlants.Remove(item.item.GetComponent<Plantable>());
        }

        private void OnPlanterAddItem(InventoryItem item)   
        {
            _trackedPlants.Add(item.item.GetComponent<Plantable>());
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

#if SUBNAUTICA_STABLE
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
                    if (_savedData.PlantAges != null)
                    {
                        foreach (PlantData data in _savedData.PlantAges)
                        {
                            var slot = _planter.GetSlotByID(data.Slot);
                            slot.plantable.plantAge = data.Age;
                        }

                    }
                    QuickLogger.Debug("Loaded Storage Data", true);
                }
            }
        }
#else
        private IEnumerator LoadStorage()
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
                    yield return StorageHelper.RestoreItemsAsync(_serializer, _savedData.Bytes, _storageContainer.container);
                    if (_savedData.PlantAges != null)
                    {
                        foreach (PlantData data in _savedData.PlantAges)
                        {
                            var slot = _planter.GetSlotByID(data.Slot);
                            slot.plantable.plantAge = data.Age;
                        }
                        
                    }
                    QuickLogger.Debug("Loaded Storage Data", true);
                }
            }
            yield break;
        }
#endif

        public byte[] GetStorageBytes(ProtobufSerializer serializer)
        {
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

    internal struct PlantData
    {
        public float Age { get; set; }
        public int Slot { get; set; }

        public PlantData(float age,int slot)
        {
            Age = age;
            Slot = slot;
        }
    }
}
