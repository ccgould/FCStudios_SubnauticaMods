using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.OreConsumer.Buildable;
using FCS_AlterraHub.Mods.OreConsumer.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Mono
{
    internal class OreConsumerController : FcsDevice, IFCSStorage, IFCSSave<SaveData>, IHandTarget
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private OreConsumerDataEntry _savedData;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public DumpContainer DumpContainer { get; private set; }
        public override bool IsOperational => CheckIfOperational();
        public override bool CanBeSeenByTransceiver => true;
        public bool IsOnPlatform => Manager?.Habitat != null;
        public override bool IsVisible => IsOperational;
        public override int MaxItemAllowForTransfer { get; } = 10;
        public override TechType[] AllowedTransferItems { get; } =
        {
            TechType.Titanium,
            TechType.Copper,
            TechType.Quartz,
            TechType.Lead,
            TechType.Diamond,
            TechType.Silver,
            TechType.Gold,
            TechType.Lithium,
            TechType.Sulphur,
            TechType.Magnetite,
            TechType.Nickel,
            TechType.AluminumOxide,
            TechType.UraniniteCrystal,
            TechType.Kyanite
        };
        
        private Dictionary<string,FcsDevice> SavedDevices => FCSAlterraHubService.PublicAPI.GetRegisteredDevices();

        private bool CheckIfOperational()
        {
            if(IsInitialized && IsConstructed && Manager != null && _oreQueue != null && Manager.HasEnoughPower(GetPowerUsage()) && !_isBreakerTripped) return true;
            return false;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }
        
        public OreConsumerDisplay DisplayManager { get; private set; }
        public TransferHandler TransferHandler { get; private set; }
        public MotorHandler MotorHandler { get; private set; }
        public EffectsManager EffectsManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public Action<bool> OnUpdateSound { get; private set; }


        private Queue<TechType> _oreQueue;

        private float _timeLeft;
        private bool _isBreakerTripped;
        private const int MAXITEMLIMIT = 10;

        public override float GetPowerUsage()
        {
            return _oreQueue != null && _oreQueue?.Count > 0 && !_isBreakerTripped ? 0.85f : 0;
        }

        #region Unity Methods

        public override void Awake()
        {
            _timeLeft = OreConsumerPatcher.OreProcessingTime;
        }

        private void Start()
        {
            InvokeRepeating(nameof(UpdateAnimation),1f,1f);
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.OreConsumerTabID, Mod.ModPackID);
        }

        private void UpdateAnimation()
        {
            if (!IsConstructed && !IsInitialized) return;
            if (_oreQueue != null && _oreQueue.Count > 0 && (Manager.GetPowerState() == PowerSystem.Status.Normal || Manager.GetPowerState() == PowerSystem.Status.Emergency) && !_isBreakerTripped)
            {
                MotorHandler.StartMotor();
                EffectsManager.ShowEffect();
                AudioManager?.PlayMachineAudio();
            }
            else
            {
                MotorHandler.StopMotor();
                EffectsManager.HideEffect();
                AudioManager?.StopMachineAudio();
            }
        }

        private void Update()
        {
            if(!IsConstructed && !IsInitialized) return;
            if (IsOperational && _oreQueue.Count > 0)
            {
                _timeLeft -= DayNightCycle.main.deltaTime;
                if (_timeLeft < 0)
                {
                    var techType = _oreQueue.Dequeue();
                    AppendMoney(StoreInventorySystem.GetOrePrice(techType));
                    _timeLeft = OreConsumerPatcher.OreProcessingTime;
                }
            }
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

                    if(_savedData.OreQueue != null)
                    {
                        _oreQueue = _savedData.OreQueue;
                        _timeLeft = _savedData.TimeLeft;
                    }

                    if(_savedData.IsBreakerTripped)
                    {
                        _isBreakerTripped = true;
                        ToggleBreakerState();
                    }
                    MotorHandler.SpeedByPass(_savedData.RPM);
                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor(),ColorTargetMode.Both);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ToggleBreakerState()
        {
            _isBreakerTripped ^= true;
        }

        #endregion

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_oreQueue == null)
            {
                _oreQueue = new Queue<TechType>();
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform, Buildables.AlterraHub.OreConsumerReceptacle(), this, 2, 5);
            }

            if (TransferHandler == null)
            {
                TransferHandler = gameObject.AddComponent<TransferHandler>();
                TransferHandler.Initialize();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<OreConsumerDisplay>();
                DisplayManager.Setup(this);
                DisplayManager.onDumpButtonClicked.AddListener(() =>
                {
                    DumpContainer.OpenStorage();
                });
                DisplayManager.ForceRefresh(CardSystem.main.GetAccountBalance());
            }

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "core_anim").AddComponent<MotorHandler>();
                MotorHandler.Initialize(30);
            }

            if (EffectsManager == null)
            {
                EffectsManager = gameObject.AddComponent<EffectsManager>();
                EffectsManager.Initialize(IsUnderWater());
            }

            if (AudioManager == null)
            {
                AudioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
                AudioManager.PlayMachineAudio();
            }

            QPatch.Configuration.OnPlaySoundToggleEvent += value => { OnUpdateSound?.Invoke(value); };

            OnUpdateSound += value =>
            {
                if (value)
                {
                    AudioManager.PlayMachineAudio();
                }
                else
                {
                    AudioManager.StopMachineAudio();
                }
            };

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, Buildables.AlterraHub.BodyMaterial);
            }

            CardSystem.main.onBalanceUpdated += () =>
            {
                DisplayManager?.onTotalChanged?.Invoke(CardSystem.main.GetAccountBalance());
            };

            DisplayManager?.onTotalChanged?.Invoke(CardSystem.main.GetAccountBalance());

#if DEBUG
                        QuickLogger.Debug($"Initialized Ore Consumer {GetPrefabID()}");
#endif
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            IsInitialized = true;
        }

        public override bool IsUnderWater()
        {
            return GetDepth() >= 7.0f;
        }

        internal float GetDepth()
        {
#if SUBNAUTICA
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
#elif BELOWZERO
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
#endif
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
                QuickLogger.Info($"Saved {GetPrefabID()}");
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

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            if (amount + _oreQueue.Count > MAXITEMLIMIT) return false;
            return StoreInventorySystem.ValidResource(techType);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                _oreQueue.Enqueue(item.item.GetTechType());
                Destroy(item.item.gameObject);
            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void AppendMoney(decimal price)
        {
            CardSystem.main.AddFinances(price);
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return _oreQueue.Count;
        }

        public override IFCSStorage GetStorage()
        {
            return this;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new OreConsumerDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.OreQueue = _oreQueue;
            _savedData.TimeLeft = _timeLeft;
            _savedData.RPM = MotorHandler.GetRPM();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            _savedData.IsBreakerTripped = _isBreakerTripped;
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.OreConsumerEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetOreConsumerDataEntrySaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color,mode);   
        }

        public override void OnHandHover(GUIHand hand)
        {
            base.OnHandHover(hand);
            var techType = GetTechType();
            if (_isBreakerTripped)
            {
                var data = new[]
                {
                    
                    AlterraHub.PressToTurnDeviceOnOff(KeyCode.F.ToString()),
                    AlterraHub.DeviceOff()
                };
                data.HandHoverPDAHelperEx(techType);
            }

            if (_oreQueue != null && DisplayManager?.CheckInteraction.IsHovered == false)
            {
                if (_oreQueue.Any())
                {
                    var pendingAmount = _oreQueue.Count > 1 ? _oreQueue.Count - 1 : 0;
                    
                    var data = new[]
                    {
                        AlterraHub.OreConsumerTimeLeftFormat(Language.main.Get(_oreQueue.Peek()), _timeLeft.ToString("N0"), $"{pendingAmount}", MAXITEMLIMIT),
                        AlterraHub.PressToTurnDeviceOnOff(KeyCode.F.ToString())
                    };
                    data.HandHoverPDAHelperEx(techType);
                }
                else
                {
                    var data = new[]
                    {
                        AlterraHub.NoOresToProcess(),
                        AlterraHub.PressToTurnDeviceOnOff(KeyCode.F.ToString())
                    };
                    data.HandHoverPDAHelperEx(techType);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleBreakerState();
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}