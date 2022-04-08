using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Spawnables;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal class QuantumPowerBankChargerController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private QuantumPowerBankChargerDataEntry _saveData;
        public StorageContainer _storage;
        private bool _locked;
        private float _timeCurrDeltaTime;
        private QuantumPowerBankController _powerBank;
        private float _partialPower;
        private Text _percentText;
        private Text _powerUsagePerSecond;
        private GameObject _ionCube;
        private GameObject _ionCubePosition;
        private Button _startButton;
        private const float MAXTIME = 30000;
        public override bool IsOperational => IsInitialized && IsConstructed;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, QuantumPowerBankChargerBuildable.QuantumPowerBankChargerTabID, Mod.ModPackID);
            PerformContainerCheck();
        }

        private void PerformContainerCheck()
        {
            var powerbank = _storage?.container?.GetItems(QuantumPowerBankSpawnable.PatchedTechType);
            if (powerbank?.Any() ?? false)
            {
                _powerBank = powerbank[0].item.gameObject.GetComponent<QuantumPowerBankController>();
            }

            var ioncube = _storage?.container?.GetItems(TechType.PrecursorIonCrystal);
            if (ioncube?.Any() ?? false)
            {
                MountIonCube(ioncube[0].item.gameObject);
            }

        }

        private void Update()
        {
            if (!_locked || _powerBank == null) return;

            _startButton.interactable = false;
            _percentText.text = $"{Mathf.CeilToInt(_powerBank.PowerManager.PowerAvailable() / 3000f * 100f)}%";

            QuickLogger.Debug("Charging", true);

            _timeCurrDeltaTime += DayNightCycle.main.deltaTime;

            QuickLogger.Debug($"Delta Time: {_timeCurrDeltaTime}");
            if (!(_timeCurrDeltaTime >= 1)) return;

            QuickLogger.Debug("Delta Passed", true);

            _timeCurrDeltaTime = 0.0f;
            
            if (!_powerBank.PowerManager.IsFull())
            {
                QuickLogger.Debug("Added charge", true);
                _powerBank.PowerManager.ModifyCharge(_partialPower);
                QuickLogger.Debug($"Player Health = {_powerBank.PowerManager.PowerAvailable()}", true);
            }
            else
            {
                var crystal = _storage.container.RemoveItem(TechType.PrecursorIonCrystal);
                if (crystal != null)
                {
                    Destroy(crystal.gameObject);
                }

                _startButton.interactable = true;
                _locked = false;
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
                var powerBanks = _storage.container.GetItems(QuantumPowerBankSpawnable.PatchedTechType) ?? new List<InventoryItem>();
                if (powerBanks.Any())
                {
                    MountPowerBank(powerBanks[0]);
                }

                var ionCubes = _storage.container.GetItems(TechType.PrecursorIonCrystal) ?? new List<InventoryItem>();
                if (ionCubes.Any())
                {
                    MountIonCube(ionCubes[0]);
                }
                
                _locked = _saveData.IsLocked;
                _partialPower = _saveData.PartialPower;

                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetQuantumPowerBankChargerSaveData(id);
        }

        public override void Initialize()
        {
            if(IsInitialized) return;
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (_storage?.container is not null)
            {
                QuickLogger.Debug("Storage is not null", true);
                _storage.enabled = false;
                _storage.container.onAddItem += ContainerOnAddItem;
                _storage.container.onRemoveItem += ContainerOnRemoveItem;
                _storage.container.isAllowedToAdd += IsAllowedToAdd;

            }

            _percentText = GameObjectHelpers.FindGameObject(gameObject, "PowerBankAmount").GetComponent<Text>();
            _ionCubePosition = GameObjectHelpers.FindGameObject(gameObject, "ionCube_PlaceHolder");
            _powerUsagePerSecond = GameObjectHelpers.FindGameObject(gameObject, "PowerUsagePerSecond").GetComponent<Text>();

            var powerBankSlotBTN = GameObjectHelpers.FindGameObject(gameObject, "PowerBankSlotBTN").GetComponent<Button>();
            powerBankSlotBTN.onClick.AddListener((() =>
            {
                if (_locked) return;
                _storage.Open(transform);
            }));

            _startButton = GameObjectHelpers.FindGameObject(gameObject, "StartBTN").GetComponent<Button>();
            _startButton.onClick.AddListener((() =>
            {
                if (_storage.container.Contains(TechType.PrecursorIonCrystal) &&
                    _storage.container.Contains(QuantumPowerBankSpawnable.PatchedTechType))
                {

                    _locked = true;
                }
            }));

            MaterialHelpers.ChangeMaterialColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseLightsEmissiveController, gameObject, Color.green);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 3f);
            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.GetTechType() == TechType.PrecursorIonCrystal &&
                !_storage.container.Contains(TechType.PrecursorIonCrystal))
            {
                return true;
            }

            if (pickupable.GetTechType() == QuantumPowerBankSpawnable.PatchedTechType &&
                !_storage.container.Contains(QuantumPowerBankSpawnable.PatchedTechType))
            {
                var controller = pickupable.gameObject.GetComponent<QuantumPowerBankController>();
                if (controller.PowerManager.IsFull())
                {
                    QuickLogger.ModMessage(AuxPatchers.QuantumPowerBankFull());
                    return false;
                }
                return true;
            }

            return false;
        }

        private void ContainerOnRemoveItem(InventoryItem item)
        {
            if (item.item.GetTechType() == QuantumPowerBankSpawnable.PatchedTechType)
            {
                UnMountPowerBank();
                _percentText.text = "0%";
            }

            if (item.item.GetTechType() == TechType.PrecursorIonCrystal)
            {
                UnMountIonCube();
            }
        }
        
        private void ContainerOnAddItem(InventoryItem item)
        {
            if (item.item.GetTechType() == QuantumPowerBankSpawnable.PatchedTechType)
            {
                MountPowerBank(item);
                var remainder = 3000 - _powerBank.PowerManager.PowerAvailable();
                _partialPower = remainder / 30;
            }

            if (item.item.GetTechType() == TechType.PrecursorIonCrystal)
            {
                MountIonCube(item);
            }
        }

        private void MountIonCube(InventoryItem item)
        {
            MountIonCube(item.item.gameObject);
        }
        
        private void MountIonCube(GameObject item)
        {
            _ionCube = item;
            var rg = _ionCube.GetComponent<Rigidbody>();
            DisableCollision(_ionCube);
            rg.isKinematic = true;
            _ionCube.transform.position = _ionCubePosition.transform.position;
            _ionCube.gameObject.SetActive(true);
        }

        private void DisableCollision(GameObject item)
        {
            var colliders = item.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider bc in colliders)
            {
                bc.isTrigger = true;
            }
        }

        private void EnableCollision(GameObject item)
        {
            var colliders = item.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider bc in colliders)
            {
                bc.isTrigger = true;
            }
        }

        private void UnMountIonCube()
        {
            var rg = _ionCube?.GetComponent<Rigidbody>();
            EnableCollision(_ionCube);
            if (rg is not null) rg.isKinematic = false;
            _ionCube?.gameObject.SetActive(false);
            _ionCube = null;
        }

        private void MountPowerBank(InventoryItem item)
        {
            _powerBank = item.item.gameObject.GetComponent<QuantumPowerBankController>();
            var rg = _powerBank.gameObject.GetComponent<Rigidbody>();
            DisableCollision(_powerBank.gameObject);
            rg.isKinematic = true;
            _powerBank.gameObject.SetActive(true);
        }

        private void UnMountPowerBank()
        {
            var rg = _powerBank.gameObject.GetComponent<Rigidbody>();
            EnableCollision(_powerBank.gameObject);
            rg.isKinematic = false;
            _powerBank.gameObject.SetActive(false);
            _powerBank = null;
        }

        
        public override float GetPowerUsage()
        {
            var amount = _locked ? 0.7f : 0f;
            _powerUsagePerSecond.text = $"Power Usage Per Second : {amount}";
            return amount;
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
                _saveData = new QuantumPowerBankChargerDataEntry();
            }

            _saveData.Id = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            _saveData.IsLocked = _locked;
            _saveData.PartialPower = _partialPower;
            newSaveData.QuantumPowerBankChargerDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            if (_storage == null)
            {
                return true;
            }

            if (!_storage.preventDeconstructionIfNotEmpty || _storage.IsEmpty()) return true;
            
            reason = Language.main.Get("DeconstructNonEmptyStorageContainerError");
            return false;
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