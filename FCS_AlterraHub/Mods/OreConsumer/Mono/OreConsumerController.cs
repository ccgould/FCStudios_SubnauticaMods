using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.OreConsumer.Buildable;
using FCS_AlterraHub.Mods.OreConsumer.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
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
        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

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
        private bool CheckIfOperational()
        {
            if(IsInitialized && IsConstructed && Manager != null && _oreQueue != null && Manager.HasEnoughPower(GetPowerUsage()) && !_isBreakerTripped) return true;
            return false;
        }
        public override Vector3 GetPosition()
        {
            return transform.position;
        }
        public TransferHandler TransferHandler { get; private set; }
        public MotorHandler MotorHandler { get; private set; }
        public EffectsManager EffectsManager { get; private set; }
        public Action<bool> OnUpdateSound { get; private set; }

        private readonly List<PistonBobbing> _pistons = new();

        private Queue<TechType> _oreQueue;

        private float _timeLeft;
        private bool _isBreakerTripped;

        private AudioSource _drillSound;
        private AudioLowPassFilter _lowPassFilter;
        private bool _wasDrillSoundPlaying;
        private const int MAXITEMLIMIT = 10;
        private SpeedModes _currentSpeedMode = SpeedModes.Low;
        private SpeedModes _pendingSpeedMode = SpeedModes.Low;
        private MotorHandler _antenna;
        private OreConsumerStatus _status = OreConsumerStatus.None;

        public override float GetPowerUsage()
        {
            return _oreQueue != null && _oreQueue?.Count > 0 && !_isBreakerTripped ? 0.85f * (int)_currentSpeedMode : 0;
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
            if (!IsConstructed || !IsInitialized || Manager == null) return;

            if (_oreQueue != null && _oreQueue.Count > 0 && (Manager.GetPowerState() == PowerSystem.Status.Normal || Manager.GetPowerState() == PowerSystem.Status.Emergency) && !_isBreakerTripped)
            {
                MotorHandler.StartMotor();
                MotorHandler.RPMByPass(30 * (int)_currentSpeedMode);
                MotorHandler.SetIncreaseRate(5);
                foreach (PistonBobbing piston in _pistons)
                {
                    piston.SetState(true);
                }
                if (_drillSound != null && !_drillSound.isPlaying)
                {
                    _drillSound.Play();
                }
            }
            else
            {
                if (_drillSound != null && _drillSound.isPlaying)
                {
                    _drillSound.Stop();
                }
                MotorHandler.StopMotor();
                foreach (PistonBobbing piston in _pistons)
                {
                    piston.SetState(false);
                }
            }
        }

        private void Update()
        {
            if(!IsConstructed || !IsInitialized || Manager == null) return;

            if (_lowPassFilter != null)
            {
                _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() ||
                                                 Player.main.IsInBase() ||
                                                 Player.main.IsInSub() ||
                                                 Player.main.inSeamoth ||
                                                 Player.main.inExosuit ? 1566f : 22000f;
            }

            if (_drillSound != null && _drillSound.isPlaying)
            {
                if (WorldHelpers.CheckIfPaused())
                {
                    _drillSound.Pause();
                    _wasDrillSoundPlaying = true;
                }

                if (_wasDrillSoundPlaying && !WorldHelpers.CheckIfPaused())
                {
                    _drillSound.Play();
                    _wasDrillSoundPlaying = false;
                }
            }
            
            if (IsOperational && _oreQueue.Count > 0)
            {
  
                _timeLeft -= DayNightCycle.main.deltaTime;

                if (_timeLeft < 0)
                {
                    if (_currentSpeedMode != _pendingSpeedMode)
                    {
                        _currentSpeedMode = _pendingSpeedMode;
                    }

                    AppendMoney(GetOreValue());
                    _oreQueue.Dequeue();
                    CalculateTimeLeft();
                    OnProcessingCompleted?.Invoke();
                }
            }

            if (WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 5f) && IsOperational && _oreQueue.Any())
            {
                MainCameraControl.main.ShakeCamera(.3f);
            }
        }

        private void CalculateTimeLeft()
        {
            _timeLeft = CalculateTargetTime();
        }

        private float CalculateTargetTime()
        {
            return OreConsumerPatcher.OreProcessingTime / (int)_currentSpeedMode;
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
                    }

                    _currentSpeedMode = _savedData.CurrentSpeedMode == SpeedModes.Off ? SpeedModes.Low : _savedData.CurrentSpeedMode;
                    _pendingSpeedMode = _savedData.PendingSpeedMode == SpeedModes.Off ? SpeedModes.Low : _savedData.PendingSpeedMode;
                    MotorHandler.SpeedByPass(_savedData.RPM);
                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ToggleBreakerState()
        {
            _isBreakerTripped ^= true;
        }

        private void UpdateVisibleElements()
        {
            if (!IsOperational && _antenna.IsRunning)
            {
                _antenna.StopMotor();
            }
            else if (IsOperational && !_antenna.IsRunning)
            {
                _antenna.StartMotor();
            }

            var isPowered = !_isBreakerTripped && Manager.GetPowerState() != PowerSystem.Status.Offline;

            if (!isPowered && _status != OreConsumerStatus.Tripped)
            {
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
                _status = OreConsumerStatus.Tripped;
                return;
            }

            if(_oreQueue.Any() && isPowered && IsOperational && _status != OreConsumerStatus.Running)
            {
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
                _status = OreConsumerStatus.Running;
                return;
            }

            if (!_oreQueue.Any() && _status != OreConsumerStatus.Idle && isPowered)
            {
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.yellow);
                _status = OreConsumerStatus.Idle;
            }
        }

        #endregion

        public override void Initialize()
        {
            if (IsInitialized) return;

            _lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();

            if (_oreQueue == null)
            {
                _oreQueue = new Queue<TechType>();
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform, AlterraHub.OreConsumerReceptacle(), this, 2, 5);
            }

            if (TransferHandler == null)
            {
                TransferHandler = gameObject.AddComponent<TransferHandler>();
                TransferHandler.Initialize();
            }

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "GrinderMeshController").AddComponent<MotorHandler>();
                MotorHandler.Initialize(30);
            }

            _antenna = GameObjectHelpers.FindGameObject(gameObject, "AntennaMeshController").AddComponent<MotorHandler>();
            _antenna.Initialize(15);

            if (EffectsManager == null)
            {
                EffectsManager = gameObject.AddComponent<EffectsManager>();
                EffectsManager.Initialize(IsUnderWater());
            }
            
            QPatch.Configuration.OnPlaySoundToggleEvent += value => { OnUpdateSound?.Invoke(value); };

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            var piston1 = GameObjectHelpers.FindGameObject(gameObject, "pump01MeshController").EnsureComponent<PistonBobbing>();
            piston1.Invert = true;

            var piston2 = GameObjectHelpers.FindGameObject(gameObject, "pump02MeshController").EnsureComponent<PistonBobbing>();

            _pistons.Add(piston1);
            _pistons.Add(piston2);
            
            if (_drillSound == null)
            { 
                _drillSound = gameObject.GetComponent<AudioSource>();
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.yellow);

            CalculateTimeLeft();

            InvokeRepeating(nameof(UpdateVisibleElements),1,1);

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
            QuickLogger.Debug($"In OnProtoSerialize -  Ore Consumer");

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
            if (_oreQueue != null && _oreQueue.Any())
            {
                reason = AlterraHub.NotEmpty();
                return false;
            }
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
            decimal deduction = 0;
            CardSystem.main.AddFinances(price);


            if (FCSPDAController.Main.Screen.GetAutomaticDebitDeduction() && !CardSystem.main.IsDebitPaid())
            {
                QuickLogger.Debug("Getting ready to deduct",true);
                deduction = MathHelpers.PercentageOfNumber(Convert.ToDecimal(FCSPDAController.Main.Screen.GetRate()), price);
                CardSystem.main.PayDebit(null,deduction);
            }
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }
        
        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public Action OnProcessingCompleted { get; set; }

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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            _savedData.IsBreakerTripped = _isBreakerTripped;
            _savedData.CurrentSpeedMode = _currentSpeedMode;
            _savedData.PendingSpeedMode = _pendingSpeedMode;
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.OreConsumerEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetOreConsumerDataEntrySaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);   
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

            if (_oreQueue != null)
            {
                if (_oreQueue.Any())
                {
                    var pendingAmount = _oreQueue.Count > 1 ? _oreQueue.Count - 1 : 0;
                    
                    var data = new[]
                    {
                        AlterraHub.OreConsumerTimeLeftFormat(GetProcessingItemString(), GetTimeLeftString(), $"{pendingAmount}", MAXITEMLIMIT),
                        AlterraHub.PressToTurnDeviceOnOff(KeyCode.F.ToString())
                    };
                    data.HandHoverPDAHelperEx(techType,HandReticle.IconType.Hand);
                }
                else
                {
                    var data = new[]
                    {
                        AlterraHub.NoOresToProcess(),
                        AlterraHub.PressToTurnDeviceOnOff(KeyCode.F.ToString())
                    };
                    data.HandHoverPDAHelperEx(techType,HandReticle.IconType.Hand);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleBreakerState();
            }
        }

        internal string GetProcessingItemString()
        {
            return !_oreQueue.Any() ? Language.main.Get("Empty") : Language.main.Get(_oreQueue.Peek());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (!CardSystem.main.HasBeenRegistered())
            {
                VoiceNotificationSystem.main.ShowSubtitle(AlterraHub.AccountNotFoundFormat());
                return;
            }

            if (Manager == null)
            {
                VoiceNotificationSystem.main.ShowSubtitle(AlterraHub.MustBeBuiltOnBasePlatform());
                return;
            }

            OreConsumerHUD.Main.Show(this);
        }

        public void OpenStorage()
        {
            DumpContainer.OpenStorage();
        }

        public void ChangeSpeedMultiplier(SpeedModes value)
        {
            if (_oreQueue.Any())
            {
                _pendingSpeedMode = value;
            }
            else
            {
                _pendingSpeedMode = _currentSpeedMode = value;
                CalculateTimeLeft();
                CalculateTargetTime();
            }
        }

        public decimal GetOreValue()
        {
            if (!_oreQueue.Any()) return 0;
            return StoreInventorySystem.GetOrePrice(_oreQueue.Peek());
        }

        public string GetTimeLeftString()
        {
            return _timeLeft.ToString("N0");
        }

        public Queue<TechType> GetOreQueue()
        {
            return _oreQueue;
        }

        public int GetSpeedMultiplier()
        {
            return (int)_pendingSpeedMode;
        }
    }

    internal enum OreConsumerStatus
    {
        None,
        Tripped,
        Running,

        Idle
    }
}