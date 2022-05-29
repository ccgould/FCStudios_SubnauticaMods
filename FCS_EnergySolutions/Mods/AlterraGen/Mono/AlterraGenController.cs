using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraGen.Mono
{
    internal class AlterraGenController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private AlterraGenDataEntry _savedData;
        internal bool IsFromSave { get; private set; }
    
        private string _prefabID;
        private bool _runStartUpOnEnable;
        private GameObject _xBubbles;
        private InterfaceInteraction _interactionHelper;

        internal AlterraGenPowerManager PowerManager { get; private set; } 
        internal AnimationManager AnimationManager { get; set; }
        internal AlterraGenDisplayManager DisplayManager { get; private set; }
        public DumpContainer DumpContainer { get; private set; }
        public override bool IsOperational => IsConstructed && IsInitialized;
        public override bool IsVisible => IsOperational;
        public override bool CanBeSeenByTransceiver => true;

        public override TechType[] AllowedTransferItems { get; } = Mod.AllowedBioItems().ToArray();

        public override int MaxItemAllowForTransfer { get; } = 9;


        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraGenModTabID, Mod.ModPackID);
        }

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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                    PowerManager.LoadFromSave(_savedData);
                }

                _runStartUpOnEnable = false;
            }
        }

        #endregion

        #region Public Methods

        public override Vector3 GetPosition()
        {
            return transform.position;
        }


        public override void Initialize()
        {
            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<AlterraGenPowerManager>();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
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
            
            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<AlterraGenDisplayManager>();
                DisplayManager.Setup(this);
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
#if DEBUG
            QuickLogger.Debug($"Changing AlterraGen color to {ColorList.GetName(color)}",true);
#endif

           return _colorManager.ChangeColor(template);
        }
        
        public override bool CanBeStored(int amount, TechType techType)
        {
            return !PowerManager.IsFull && PowerManager.IsAllowedToAdd(techType,true);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            return PowerManager.AddItemToContainer(item);
        }

        public override IFCSStorage GetStorage()
        {
            return PowerManager;
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
            _savedData = Mod.GetAlterraGenSaveData(id);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraGenDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}",true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Storage = PowerManager.GetItemsWithin();
            _savedData.ToConsume = PowerManager.GetToConsume();
            _savedData.PowerState = PowerManager.PowerState;
            _savedData.StoredPower = PowerManager.GetStoredPower();
            _savedData.Power = PowerManager.GetPowerSourcePower();
            _savedData.BaseId = BaseId;
            newSaveData.AlterraGenEntries.Add(_savedData);
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
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {_prefabID}");
            }
        }

        #endregion

        public override void OnHandHover(GUIHand hand)
        {
            if(!IsInitialized || IsConstructed || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);

            var data = new[]
            {
                $"{string.Format(Language.main.Get("PowerCellStatus"), PowerManager.GetTotalPowerString(), PowerManager.GetDevicePowerCapacity())} | {AlterraHub.IsDeviceOn(PowerManager.GetPowerState() == FCSPowerStates.Powered)} | {AlterraHub.PowerPerMinute(PowerManager.GetPowerUsagePerSecond() * 60)}"
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            // Not In Use
        }
    }
}
