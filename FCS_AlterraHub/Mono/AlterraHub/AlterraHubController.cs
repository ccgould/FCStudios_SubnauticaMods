using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers.Mission;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Converters;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class AlterraHubController: FcsDevice, IFCSSave<SaveData>, IHandTarget, IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private AlterraHubDataEntry _savedData;
        private bool _cursorLockCached;
        private GameObject _inputDummy;
        private GameObject _hubCameraPosition;
        private bool _isInRange;
        private GameObject _screenBlock;
        private DumpContainerSimplified _dumpContainer;
        private MotorHandler _motorHandler;
        private GameObject _playerBody;
        private bool _isInUse;
        private MissionManager _missionManager => QPatch.MissionManagerGM;
        public Sun Sun => Sun.main;

        internal HubTrigger AlterraHubTrigger { get; set; }
        internal AlterraHubDisplay DisplayManager { get; private set; }
        private MessageBoxHandler messageBoxHandler => MessageBoxHandler.main;


        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraHubTabID, Mod.ModName);
            IPCMessage += IpcMessage;
        }

        private void IpcMessage(string message)
        {
            if (message.Equals("ActivateGoal"))
            {
                var panelGroup = DisplayManager.GetPanelGroup();
                foreach (PanelHelper panelHelper in panelGroup.PanelHelpers)
                {
                    panelHelper.ActivateStoreItem(BaseManager.ActivateGoalTechType);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && _isInRange)
            {
                ExitStore();
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

                    DisplayManager.Load(_savedData);
                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                }
                
                _runStartUpOnEnable = false;
            }
        }

        #endregion

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            if(IsInitialized) return;

            MessageBoxHandler.main.ObjectRoot = gameObject;

            if (AlterraHubTrigger == null)
            {
                AlterraHubTrigger = GameObjectHelpers.FindGameObject(gameObject, "Trigger").AddComponent<HubTrigger>();
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform, "Item Return", this);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<AlterraHubDisplay>();
                DisplayManager.Setup(this);
                DisplayManager.OnReturnButtonClicked += () =>
                {
                    ExitStore();
                    _dumpContainer.OpenStorage();
                };
            }

            if (_motorHandler == null)
            {
                _motorHandler = GameObjectHelpers.FindGameObject(gameObject, "RoundSignDisplay01").AddComponent<MotorHandler>();
                _motorHandler.Initialize(30);
                _motorHandler.StartMotor();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, Buildables.AlterraHub.BodyMaterial);
            }

            _playerBody = Player.main.playerController.gameObject.FindChild("body");

            //var ui = GameObject.Instantiate(Buildables.AlterraHub.ColorPickerDialogPrefab);
            //HUD = ui.AddComponent<FCSHUD>();
            //HUD.Move();

            _screenBlock = GameObjectHelpers.FindGameObject(gameObject, "Blocker");

            LoadStore();

            InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);


            AlterraHubTrigger.onTriggered += value =>
            {
                _isInRange = true;
                if (!value)
                {
                    _isInRange = false;
                    ExitStore();
                }
            };

            _hubCameraPosition = GameObjectHelpers.FindGameObject(gameObject, "CameraPosition");

            IsInitialized = true;
        }

        private void OnQuit()
        {
            if (!CardSystem.main.HasAccountBeenSaved())
            {
                Mod.DeepCopySave(CardSystem.main.GetAccount());
            }
            QuickLogger.Debug("Quitting Purging CardSystem and AlterraHubSave",true);
            CardSystem.main.Purge();
            Mod.PurgeSave();
        }

        public FCSHUD HUD { get; set; }

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

        internal bool MakeAPurchase(CartDropDownHandler cart,bool giveToPlayer = true)
        {
            if (giveToPlayer)
            {
                var totalCash = cart.GetTotal();
                if (CardSystem.main.HasEnough(totalCash))
                {
                    CardSystem.main.RemoveFinances(totalCash);
                    foreach (CartItem item in cart.GetItems())
                    {
                        QuickLogger.Debug($"{item.ReceiveTechType}",true);
                        PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType);
                    }
                }

                return true;
            }
            else
            {
                //TODO send to DSS
            }

            return false;
        }

        internal void ReturnItem(InventoryItem item)
        {
            var result = StoreInventorySystem.ItemReturn(item);
            
            if (result)
            {
                //TODO Send message about transaction completed
                Destroy(item.item.gameObject);
            }
        }

        internal string GetAccountAmount()
        {
            var amount = CardSystem.main.GetAccountBalance();
            return Converters.DecimalToMoney("C",amount);
        }

        private void LoadStore()
        {
            var panelGroup = DisplayManager.GetPanelGroup();
            
            foreach (PanelHelper panelHelper in panelGroup.PanelHelpers)
            {
                QuickLogger.Debug($"Loading Panel: {panelHelper.StoreCategory}:");
                foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
                {
                    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.Key)} to Panel: {panelHelper.StoreCategory}:");

                    if (panelHelper.StoreCategory == storeItem.Value.StoreCategory)
                    {
                        StoreInventorySystem.AddNewStoreItem(storeItem.Value);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(storeItem.Value, AddToCardCallBack,IsInUse));
                        QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.Key)} to Panel: {panelHelper.StoreCategory}:");
                    }
                }

                foreach (FCSStoreEntry storeItem in QPatch.Configuration.AdditionalStoreItems)
                {
                    if (panelHelper.StoreCategory == storeItem.StoreCategory)
                    {
                        QuickLogger.Info($"Item: {storeItem.TechType} || Category: {storeItem.StoreCategory} || Cost: {storeItem.Cost}");
                        StoreInventorySystem.AddNewStoreItem(storeItem);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(storeItem, AddToCardCallBack, IsInUse));
                    }
                }
            }
        }

        private bool IsInUse()
        {
            return _isInUse;
        }

        private void AddToCardCallBack(TechType techType,TechType receiveTechType)
        {
            DisplayManager.onItemAddedToCart?.Invoke(techType, receiveTechType);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraHubDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.CartItems = DisplayManager.SaveCartItems();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            newSaveData.AlterraHubEntries.Add(_savedData);
            QuickLogger.Debug($"Saved ID {_savedData.Id}", true);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraHubSaveData(GetPrefabID());
        }

        public bool IsPlayerInRange()
        {
            return AlterraHubTrigger.IsPlayerInRange;
        }

        private GameObject inputDummy
        {
            get
            {
                if (this._inputDummy == null)
                {
                    this._inputDummy = new GameObject("InputDummy");
                    this._inputDummy.SetActive(false);
                }
                return this._inputDummy;
            }
        }

        internal void InterceptInput(bool state)
        {
            if (inputDummy.activeSelf == state)
            {
                return;
            }
            if (state)
            {
                _screenBlock.SetActive(false);
                Player.main.EnterLockedMode(null);
                MainCameraControl.main.enabled = false;
                InputHandlerStack.main.Push(inputDummy);
                _cursorLockCached = UWE.Utils.lockCursor;
                UWE.Utils.lockCursor = false;
                return;
            }
            
            UWE.Utils.lockCursor = _cursorLockCached;
            InputHandlerStack.main.Pop(inputDummy);
            MainCameraControl.main.enabled = true;
            _screenBlock.SetActive(true);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_isInRange)
            {
                HandReticle main = HandReticle.main;
                main.SetInteractText("Click to use Alterra Hub");
                main.SetIcon(HandReticle.IconType.Hand);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_isInRange)
            {
                InterceptInput(true);
                _isInUse = true;
                var hudCameraPos = _hubCameraPosition.transform.position;
                var hudCameraRot = _hubCameraPosition.transform.rotation;
                Player.main.SetPosition(new Vector3(hudCameraPos.x, Player.main.transform.position.y, hudCameraPos.z), hudCameraRot);
                _playerBody.SetActive(false);
                //Player.main.gameObject.transform.position = new Vector3(hudCameraPos.x, Player.main.gameObject.transform.position.y, hudCameraPos.z);
                SNCameraRoot.main.transform.position = hudCameraPos;
                SNCameraRoot.main.transform.rotation = hudCameraRot;
            }
        }

        internal void ExitStore()
        {
            _isInUse = false;
            SNCameraRoot.main.transform.localPosition = Vector3.zero;
            SNCameraRoot.main.transform.localRotation = Quaternion.identity;
            ExitLockedMode();
            _playerBody.SetActive(true);
        }

        private void ExitLockedMode()
        {
            Player.main.ExitLockedMode(false, false);
            InterceptInput(false);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            CardSystem.main.AddFinances(StoreInventorySystem.GetPrice(item.item.GetTechType(),true));
            Destroy(item.item.gameObject);
            return true;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            return StoreInventorySystem.StoreHasItem(techType,true);
        }

        public bool IsAllowedToAdd(Pickupable inventoryItem, bool verbose)
        {
            return false;
        }
    }
}
