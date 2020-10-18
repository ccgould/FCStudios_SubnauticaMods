using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Mono.OreConsumer
{
    internal class OreConsumerController : FcsDevice, IFCSStorage
    {
        private float _balance;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private OreConsumerDataEntry _savedData;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public DumpContainer DumpContainer { get; private set; }
        public bool IsOperational { get; set; }
        public OreConsumerDisplay DisplayManager { get; private set; }
        public TransferHandler TransferHandler { get; private set; }
        public MotorHandler MotorHandler { get; private set; }
        public EffectsManager EffectsManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public Action<bool> onUpdateSound { get; private set; }

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {

        }

        private void Update()
        {
            //Try not to use update if possible
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

                    AppendMoney(_savedData.OreConsumerCash);
                    MotorHandler.SpeedByPass(_savedData.RPM);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {

        }

        #endregion

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,Buildables.AlterraHub.OreConsumerReceptacle(),this);
            }

            if (TransferHandler == null)
            {
                TransferHandler = gameObject.AddComponent<TransferHandler>();
                TransferHandler.Initialize();
            }

            if(DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<OreConsumerDisplay>();
                DisplayManager.Setup(this);

                DisplayManager.onDumpButtonClicked.AddListener(() =>
                {
                    DumpContainer.OpenStorage();
                });

                DisplayManager.onTransferMoneyClicked.AddListener(() =>
                {
                    TransferHandler.OpenStorage(WithDrawMoney);
                });
            }

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "core_anim").AddComponent<MotorHandler>();
                MotorHandler.Initialize(30);
                //TODO Control motor based off power handler
                MotorHandler.Start();
            }

            if (EffectsManager == null)
            {
                EffectsManager = gameObject.AddComponent<EffectsManager>();
                EffectsManager.Initialize(IsUnderWater());
                //TODO Control effect based off power handler
                EffectsManager.ShowEffect();
            }

            if(AudioManager == null)
            {
                AudioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
                AudioManager.PlayMachineAudio();
            }

            QPatch.Configuration.OnPlaySoundToggleEvent += value => { onUpdateSound?.Invoke(value); };

            onUpdateSound += value =>
            {
                if(value)
                {
                    AudioManager.PlayMachineAudio();
                }
                else
                {
                    AudioManager.StopMachineAudio();
                }
            };

#if DEBUG
            QuickLogger.Debug($"Initialized Ore Consumer {GetPrefabID()}");
#endif
            IsInitialized = true;
        }

        private bool IsUnderWater()
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
            if (_balance > 0)
            {
                reason = Buildables.AlterraHub.RemoveAllCreditFromDevice();
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

        public bool CanBeStored(int amount, TechType techType)
        {
            return StoreInventorySystem.VaildResource(techType);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                AppendMoney(StoreInventorySystem.GetOrePrice(item.item.GetTechType()));
                Destroy(item.item.gameObject);
            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void AppendMoney(float price)
        {
            _balance += price;
            DisplayManager.onTotalChanged?.Invoke(_balance);
        }

        private void WithDrawMoney(string cardNumber)
        {
            if (CardSystem.main.CardExist(cardNumber))
            {
                CardSystem.main.AddFinances(_balance);
                EmptyBalance();
                return;
            }

            MessageBoxHandler.main.Show(string.Format(Buildables.AlterraHub.AccountNotFoundFormat(),cardNumber));
        }

        private void EmptyBalance()
        {
            _balance = 0f;
            DisplayManager.onTotalChanged?.Invoke(_balance);
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(0, pickupable.GetTechType());
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

        public void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new OreConsumerDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.OreConsumerCash = _balance;
            _savedData.RPM = MotorHandler.GetRPM();
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);

            //_savedData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            newSaveData.OreConsumerEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetOreConsumerDataEntrySaveData(GetPrefabID());
        }
    }
}