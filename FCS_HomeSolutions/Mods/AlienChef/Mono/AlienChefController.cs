using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCS_HomeSolutions.Mods.Stove.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class AlienChefController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private AlienChiefDataEntry _saveData;
        private InterfaceInteraction _interactionHelper;
        const int MAXSLOTS = 48;
        internal DisplayManager DisplayManager { get; private set; }
        internal Cooker Cooker { get; set; }
        internal FCSStorage StorageSystem { get; set; }
        public bool PullFromDataStorage { get; set; }
        public bool IsSendingToSeaBreeze { get; set; }

        private void Start()
        {
            //FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlienChiefTabID, Mod.ModPackID);
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
                RefreshUI();
                _fromSave = false;
            }
        }

        public override float GetPowerUsage()
        {
            return Cooker.IsCooking ? 0.05f : 0f;
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
            }

            if (StorageSystem == null)
            {
                StorageSystem = gameObject.GetComponent<FCSStorage>();
                StorageSystem.NotAllowedToAddItems = true;
                StorageSystem.SlotsAssigned = MAXSLOTS;
                StorageSystem.ItemsContainer.onAddItem += type =>
                {

                };
                StorageSystem.ItemsContainer.onRemoveItem += type =>
                {

                };
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            Mod.GetFoodCustomTrees();

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
                //QuickLogger.Info($"Saving {Mod.AlienChefFriendly}");
                //Mod.Save(serializer);
                //QuickLogger.Info($"Saved {Mod.AlienChefFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }

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
            _saveData.ColorTemplate= _colorManager.SaveTemplate();
            //_saveData.Bytes = StorageSystem.Save(serializer);
            newSaveData.AlienChiefDataEntries.Add(_saveData);
        }

        internal void SendToSeaBreeze(InventoryItem inventoryItem)
        {
            var seabreezes = Manager.GetDevices(SeaBreezeBuildable.SeaBreezeTabID);
            foreach (FcsDevice device in seabreezes)
            {
                if (!device.IsConstructed) continue;
                if (device.CanBeStored(1, inventoryItem.item.GetTechType()))
                {
                    device.AddItemToContainer(inventoryItem);
                }
            }
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public bool HasPowerToConsume()
        {
            return Manager.HasEnoughPower(GetPowerUsage());
        }

        public override void OnHandHover(GUIHand hand)
        {
            if(!IsInitialized || !IsConstructed  || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);
            
            var data = new[]
            {
                AlterraHub.PowerPerMinute(GetPowerUsage() * 60)
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not In Use
        }
    }
}