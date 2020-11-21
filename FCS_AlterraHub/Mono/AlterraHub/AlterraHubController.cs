using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Converters;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using rail;
using UnityEngine;
using UnityEngine.UI;

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
        private ColorManager _colorManager;
        private DumpContainerSimplified _dumpContainer;


        internal HubTrigger AlterraHubTrigger { get; set; }
        internal AlterraHubDisplay DisplayManager { get; private set; }
        public MotorHandler MotorHandler { get; private set; }

        #region Unity Methods
        
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
                    _colorManager.ChangeColor(_savedData.Color.Vector4ToColor());
                }
                
                _runStartUpOnEnable = false;
            }
        }

        #endregion
        
        public override void Initialize()
        {
            if(IsInitialized) return;

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
                DisplayManager.OnReturnButtonClicked += () => { _dumpContainer.OpenStorage();};
            }

            if (MotorHandler == null)
            {
                MotorHandler = GameObjectHelpers.FindGameObject(gameObject, "RoundSignDisplay01").AddComponent<MotorHandler>();
                MotorHandler.Initialize(30);
                //TODO Control motor based off power handler
                MotorHandler.Start();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, Buildables.AlterraHub.BodyMaterial);
            }

            //var ui = GameObject.Instantiate(Buildables.AlterraHub.ColorPickerDialogPrefab);
            //HUD = ui.AddComponent<FCSHUD>();
            //HUD.Move();
            _screenBlock = GameObjectHelpers.FindGameObject(gameObject, "Blocker");

            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.ModID);

            LoadStore();
            
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
                        var invType = item.ReceiveTechType.ToInventoryItem();
                        PlayerInteractionHelper.GivePlayerItem(invType);
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
                foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
                {
                    if (panelHelper.StoreCategory == storeItem.Value.StoreCategory)
                    {
                        var item = storeItem.Value;
                        StoreInventorySystem.AddNewStoreItem(item.TechType,item.Cost);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(item.TechType, item.ReceiveTechType, item.StoreCategory, item.Cost, AddToCardCallBack));
                    }
                }

                foreach (CustomStoreItem customStoreItem in QPatch.Configuration.AdditionalStoreItems)
                {
                    if (panelHelper.StoreCategory == customStoreItem.Category)
                    {
                        QuickLogger.Info($"Item: {customStoreItem.TechType} || Category: {customStoreItem.Category} || Cost: {customStoreItem.Cost}");
                        StoreInventorySystem.AddNewStoreItem(customStoreItem.TechType, customStoreItem.Cost);
                        panelHelper.AddContent(StoreInventorySystem.CreateStoreItem(customStoreItem.TechType, customStoreItem.ReturnItemTechType, customStoreItem.Category, customStoreItem.Cost, AddToCardCallBack));
                    }
                }
            }
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
            _savedData.Color = _colorManager.GetColor().ColorToVector4();
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

                var hudCameraPos = _hubCameraPosition.transform.position;
                var hudCameraRot = _hubCameraPosition.transform.rotation;
                Player.main.SetPosition(new Vector3(hudCameraPos.x, Player.main.transform.position.y, hudCameraPos.z), hudCameraRot);

                //Player.main.gameObject.transform.position = new Vector3(hudCameraPos.x, Player.main.gameObject.transform.position.y, hudCameraPos.z);
                SNCameraRoot.main.transform.position = hudCameraPos;
                SNCameraRoot.main.transform.rotation = hudCameraRot;
            }
        }

        internal void ExitStore()
        {
            SNCameraRoot.main.transform.localPosition = Vector3.zero;
            SNCameraRoot.main.transform.localRotation = Quaternion.identity;
            ExitLockedMode();
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

        public bool AddItemToContainer(InventoryItem item)
        {
            CardSystem.main.AddFinances(StoreInventorySystem.GetPrice(item.item.GetTechType()));
            Destroy(item.item.gameObject);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return StoreInventorySystem.ValidStoreItem(pickupable.GetTechType());
        }
    }
}
