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
        public bool PullFromDataStorage { get; set; }
        public bool SendToSeaBreeze { get; set; }

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
            
            if (Cooker == null)
            {
                Cooker = gameObject.EnsureComponent<Cooker>();
                Cooker.Initialize(this);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<DisplayManager>();
                DisplayManager.Setup(this);
            }

            if (StorageSystem == null)
            {
                StorageSystem = gameObject.EnsureComponent<FCSStorage>();
                StorageSystem.Initialize(48,6,8,"Alien Chef Storage");
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
            if(_saveData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
            StorageSystem.RestoreItems(serializer, _saveData.Bytes);

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
            _saveData.Bytes = StorageSystem.Save(serializer);
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
            QuickLogger.Debug($"Trying to get {dialog.TechType}",true);

            var targetAmount = amount;

            QuickLogger.Debug($"Inventory has item {Inventory.main.container.Contains(dialog.TechType)} || Target Amount: {targetAmount}", true);
            if (Inventory.main.container.Contains(dialog.TechType))
            {
                QuickLogger.Debug($"Inventory has item count {Inventory.main.container.GetCount(dialog.TechType)}", true);
                for (int i = 0; i < Inventory.main.container.GetCount(dialog.TechType); i++)
                {
                    if(targetAmount == 0) break;
                    Destroy(Inventory.main.container.RemoveItem(dialog.TechType));
                    Cooker.AddToQueue(dialog.TechType,dialog.CookedTechType,dialog.CuredTechType);
                    targetAmount--;
                }
            }

            if (targetAmount > 0 && PullFromDataStorage)
            {
                QuickLogger.Debug($"Manager has item {Manager.HasItem(dialog.TechType)}", true);
                if (Manager.HasItem(dialog.TechType))
                {
                    for (int i = 0; i < Manager.GetItemCount(dialog.TechType); i++)
                    {
                        if (targetAmount == 0) break;
                        Destroy(Manager.TakeItem(dialog.TechType));
                        Cooker.AddToQueue(dialog.TechType, dialog.CookedTechType, dialog.CuredTechType);
                        targetAmount--;
                    }
                }
            }

            if (targetAmount < amount)
            {
                Cooker.StartCooking();
                QuickLogger.ModMessage($"Alien Chef {UnitID} is cooking {amount - targetAmount} items");
                return true;
            }

            QuickLogger.ModMessage($"Alien Chef failed to get any items for cooking items");
            return false;
        }

        public void AddToOrder(CookerItemController cookerItemDialog, int amount)
        {
            DisplayManager.AddToOrder(cookerItemDialog,amount);
        }
    }
}
