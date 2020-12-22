using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class AlienChefController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private AlienChiefDataEntry _saveData;

        internal DisplayManager DisplayManager { get; private set; }
        internal Cooker Cooker { get; set; }
        internal FCSStorage StorageSystem { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlienChiefTabID, Mod.ModName);
            StorageSystem.CleanUpDuplicatedStorageNoneRoutine();
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
                _colorManager.ChangeColor(_saveData.Fcs.Vector4ToColor());
                _fromSave = false;
            }
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<DisplayManager>();
                DisplayManager.Setup(this);
            }

            if (Cooker == null)
            {
                Cooker = gameObject.EnsureComponent<Cooker>();
                Cooker.Initialize(this);
            }

            if (StorageSystem == null)
            {
                StorageSystem = gameObject.EnsureComponent<FCSStorage>();
                StorageSystem.Initialize(10);
            }

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject, 5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetAlienChiefSaveData(id);
        }
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.AlienChefFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.AlienChefFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new AlienChiefDataEntry();
            }
            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();

            newSaveData.AlienChiefDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public bool HasPowerToConsume()
        {
            return Manager.HasEnoughPower(GetPowerUsage());
        }

        public bool TryGetItem(CookerItemController dialog,int amount)
        {
            if (Inventory.main.container.Contains(dialog.TechType) && Inventory.main.container.GetCount(dialog.TechType) >= amount)
            {
                for (int i = 0; i < amount; i++)
                {
                    Destroy(Inventory.main.container.RemoveItem(dialog.TechType));
                    Cooker.AddToQueue(dialog.TechType,dialog.CookedTechType,dialog.CuredTechType);
                }

                Cooker.StartCooking();
                return true;
            }

            //if (StorageSystem.ItemsContainer.Contains(techType) && Inventory.main.container.GetCount(techType) >= amount)
            //{
            //    Cooker.AddToQueue(StorageSystem.ItemsContainer.RemoveItem(techType));
            //}

            return false;
        }

        public void AddToOrder(CookerItemController cookerItemDialog)
        {
            DisplayManager.AddToOrder(cookerItemDialog);
        }
    }
}
