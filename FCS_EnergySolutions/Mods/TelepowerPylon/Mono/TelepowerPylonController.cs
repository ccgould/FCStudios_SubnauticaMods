using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Buildable;
using FCS_EnergySolutions.Mods.TelepowerPylon.Interfaces;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class TelepowerPylonController : FcsDevice,IFCSSave<SaveData>, IHandTarget, IFCSDumpContainer
    {
        private TelepowerPylonDataEntry _savedData;
        internal bool IsFromSave { get; private set; }
        private bool _runStartUpOnEnable;
        private BaseTelepowerPylonManager _baseTelepowerPylonManager;
        private GameObject _canvas;
        private NameController _nameController;
        private readonly Dictionary<string, FrequencyItemController> _trackedPullFrequencyItem = new();
        private readonly Dictionary<string, FrequencyItemController> _trackedPushFrequencyItem = new();


        private GameObject _pullGrid;
        private Text _status;

        private bool _attemptedToLoadConnections;
        private Toggle _pullToggle;
        private Toggle _pushToggle;
        private bool _loadingFromSave;
        private FCSMessageBox _messageBox;
        private bool _cursorLockCached;
        private bool _isInRange ;
        private bool _isInUse;
        private GameObject _inputDummy;
        private GameObject _cameraPosition;
        private GameObject _screenBlock;
        private GameObject _playerBody;
        private DumpContainerSimplified _dumpContainer;
        private Button _upgradeBTN;
        private ParticleSystem[] _particles;
        private bool _isFromConstructed;
        private InterfaceInteraction _interactionHelper;
        private GameObject _pushGrid;
        private GameObject _pullGridContent;
        private GameObject _pushGridContent;
        private bool _allowedToInteract;

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
        public override bool IsOperational => Manager != null && IsConstructed;
        public Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }
        public TelepowerPylonTrigger _telepowerPylonTrigger { get; private set; }

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, TelepowerPylonBuildable.TelepowerPylonTabID, Mod.ModPackID);

            if (Manager == null)
            {
                foreach (ParticleSystem particle in _particles)
                {
                    particle.Stop();
                }
            }
            else
            {
              _baseTelepowerPylonManager =   Manager.Habitat.gameObject.GetComponentInChildren<BaseTelepowerPylonManager>();
            }
            InvokeRepeating(nameof(CheckTeleportationComplete), 0.2f, 0.2f);
            InvokeRepeating(nameof(UpdateStatus), 1f, 1f);
            _baseTelepowerPylonManager.RegisterPylon(this);
            Manager.NotifyByID(TelepowerPylonBuildable.TelepowerPylonTabID, "PylonBuilt");
        }

        private void CheckTeleportationComplete()
        {
            QuickLogger.Debug("Checking if world is settled");
            if (LargeWorldStreamer.main.IsWorldSettled())
            {
                OnWorldSettled();
                CancelInvoke(nameof(CheckTeleportationComplete));
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

                if (IsFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (!string.IsNullOrEmpty(_savedData.BaseId))
                    {
                        BaseId = _savedData.BaseId;
                    }
                    _colorManager.LoadTemplate(_savedData.ColorTemplate);

                    switch (_savedData.PylonMode)
                    {
                        case TelepowerPylonMode.PULL:
                            _pullToggle.isOn = true;
                            break;
                        case TelepowerPylonMode.PUSH:
                            _pushToggle.isOn = true;
                            break;
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        private void Update()
        {
            _canvas.gameObject.SetActive(Manager != null);
            

            if (Input.GetKeyDown(KeyCode.Escape) && _isInRange)
            {
                ExitDisplay();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyCalledAction?.Invoke(null);
            _baseTelepowerPylonManager?.UnRegisterPylon(this);
            Manager?.NotifyByID(TelepowerPylonBuildable.TelepowerPylonTabID, "PylonDestroy");

        }

        #endregion

        #region Public Methods
        
        public override float GetPowerUsage()
        {
            if (!IsConstructed || Manager == null) return 0f;
            return CalculatePowerUsage();
        }
        
        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            
            FindCanvas();

            InitializeMessageBox();

            InitializeColorManager();

            InitializeNameController();

            InitializeGrids();

            _status = GameObjectHelpers.FindGameObject(gameObject, "Status")?.GetComponent<Text>();

            InitializeUpgradeButton();
            
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeSpecSettings(AlterraHub.BaseDecalsExterior, AlterraHub.TBaseSpec, gameObject, 2.61f, 8f);

            InitializeToggles();

            InitializePylonTrigger();

            InitializeDumpContainer();
            
            _particles = gameObject.GetComponentsInChildren<ParticleSystem>();
            _cameraPosition = GameObjectHelpers.FindGameObject(gameObject, "CameraPosition");
            _screenBlock = GameObjectHelpers.FindGameObject(gameObject, "MainBlocker");

            _playerBody = Player.main.playerController.gameObject.FindChild("body");
            
            InitializeIPC();
            
            RefreshUI();

            IsInitialized = true;
        }

        private void InitializeDumpContainer()
        {
            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform, "Add Upgrade", this, 1, 1);
            }
        }

        private void InitializePylonTrigger()
        {
            if (_telepowerPylonTrigger == null)
            {
                _telepowerPylonTrigger = GameObjectHelpers.FindGameObject(gameObject, "Trigger")
                    .AddComponent<TelepowerPylonTrigger>();
            }

            _telepowerPylonTrigger.onTriggered += value =>
            {
                _isInRange = true;
                if (value) return;
                _isInRange = false;
                ExitDisplay();
            };
        }

        private void InitializeUpgradeButton()
        {
            _upgradeBTN = GameObjectHelpers.FindGameObject(gameObject, "UpgradeButton")?.GetComponent<Button>();
            _upgradeBTN.onClick.AddListener(() =>
            {
                ExitDisplay();
                _dumpContainer?.OpenStorage();
            });
        }

        private void InitializeToggles()
        {
            _pushToggle = GameObjectHelpers.FindGameObject(gameObject, "PushToggle")?.GetComponent<Toggle>();
            if (_pushToggle != null)

                _pushToggle.onValueChanged.AddListener((value =>
                {
                    if (_baseTelepowerPylonManager == null || _messageBox == null || _pullToggle == null)
                    {
                        return;
                    }

                    if (_baseTelepowerPylonManager.HasConnections())
                    {
                        _messageBox?.Show(AuxPatchers.RemoveAllTelepowerConnectionsPush(), FCSMessageButton.OK, null);
                        _pullToggle.SetIsOnWithoutNotify(true);
                        return;
                    }

                    if (value)
                    {
                        _baseTelepowerPylonManager?.SetCurrentMode(TelepowerPylonMode.PUSH);
                        _pullGrid?.SetActive(false);
                        _pushGrid?.SetActive(true);
                        Manager?.NotifyByID(TelepowerPylonBuildable.TelepowerPylonTabID, "PylonModeSwitch");
                    }
                }));

            _pullToggle = GameObjectHelpers.FindGameObject(gameObject, "PullToggle")?.GetComponent<Toggle>();
            if (_pullToggle != null)
                _pullToggle.onValueChanged.AddListener((value =>
                {
                    if (_trackedPullFrequencyItem == null || _messageBox == null || _pushToggle == null)
                    {
                        return;
                    }

                    if (CheckIfAnyCheckForPushMode())
                    {
                        _messageBox?.Show(AuxPatchers.RemoveAllTelepowerConnectionsPull(), FCSMessageButton.OK, null);
                        _pushToggle.SetIsOnWithoutNotify(true);
                        return;
                    }

                    QuickLogger.Debug($"Has Frequency Item: {_trackedPullFrequencyItem.Any()}", true);

                    if (value)
                    {
                        _baseTelepowerPylonManager?.SetCurrentMode(TelepowerPylonMode.PULL);
                        _pullGrid?.SetActive(true);
                        _pushGrid?.SetActive(false);
                        Manager?.NotifyByID(TelepowerPylonBuildable.TelepowerPylonTabID, "PylonModeSwitch");
                    }
                }));
        }

        private void InitializeIPC()
        {
            IPCMessage += message =>
            {
                if (message.Equals("UpdateEffects"))
                {
                    ChangeTrailColor();
                }
            };

            IPCMessage += message =>
            {
                if (message.Equals("PylonBuilt") || message.Equals("PylonDestroy") || message.Equals("PylonModeSwitch"))
                {
                    RefreshUI();
                    GoToPage(_baseTelepowerPylonManager.GetCurrentMode());
                }
            };
        }

        private void InitializeGrids()
        {
            _pullGrid = GameObjectHelpers.FindGameObject(gameObject, "Pull Scroll View");
            _pullGridContent = GameObjectHelpers.FindGameObject(_pullGrid, "Content");

            _pushGrid = GameObjectHelpers.FindGameObject(gameObject, "Push Scroll View");
            _pushGridContent = GameObjectHelpers.FindGameObject(_pushGrid, "Content");
        }

        private void InitializeNameController()
        {
            if (_nameController == null)
            {
                _nameController = gameObject.AddComponent<NameController>();
                _nameController.Initialize("Connect", "Telepower Pylon Search");
                _nameController.SetCurrentName("TP");
                //_nameController.OnLabelChanged += OnSearchConnection;
                _nameController.SetMaxChar(20);
            }
        }

        private void InitializeColorManager()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }
        }

        private void InitializeMessageBox()
        {
            var message = GameObjectHelpers.FindGameObject(gameObject, "SelectAMode")?.GetComponent<Text>();
            if (message != null) message.text = AuxPatchers.SelectAMode();

            if (_messageBox == null)
            {
                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            }
        }

        private void FindCanvas()
        {
            if (_canvas == null)
            {
                _canvas = GameObjectHelpers.FindGameObject(gameObject, "Canvas");
                _canvas?.SetActive(IsConstructed);
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
        
        public FcsDevice GetDevice()
        {
            return this;
        }

        public bool IsPlayerInRange()
        {
            return _telepowerPylonTrigger.IsPlayerInRange;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            var result = _baseTelepowerPylonManager.AttemptUpgrade(item.item.GetTechType());
            if(result)
            {
                Destroy(item.item.gameObject);
                return true;
            }

            PlayerInteractionHelper.GivePlayerItem(item);
            return false;
        }

        internal void ChangeTrailColor()
        {
            foreach (ParticleSystem system in _particles)
            {
                var index = QPatch.Configuration.TelepowerPylonTrailBrightness;
                var h = system.trails;
                h.colorOverLifetime = new Color(index, index, index);
            }

        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            var result = techType == BaseTelepowerPylonManager.Mk2UpgradeTechType || techType == BaseTelepowerPylonManager.Mk3UpgradeTechType;
            if (!result)
            {
                QuickLogger.ModMessage("Only Telepower Pylon Upgrade MK2 and MK3 Allowed.");
            }

            return result;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var result = pickupable.GetTechType() == BaseTelepowerPylonManager.Mk2UpgradeTechType || pickupable.GetTechType() == BaseTelepowerPylonManager.Mk3UpgradeTechType;
            if (!result)
            {
                QuickLogger.ModMessage("Only Telepower Pylon Upgrade MK2 and MK3 Allowed.");
            }

            return result;
        }
        
        public bool CanAddNewPylon()
        {
            return _baseTelepowerPylonManager.GetConnections().Count < _baseTelepowerPylonManager.GetMaxConnectionLimit();
        }

        public void ActivateItemOnPushGrid(ITelepowerPylonConnection parentController)
        {
            var id = parentController.UnitID;
            if (IsPushFrequencyItem(id))
            {
                GetPushFrequencyItem(id)?.Check();
            }
        }

        public void ActivateItemOnPullGrid(ITelepowerPylonConnection parentController)
        {
            var id = parentController.UnitID.ToLower();
            if (IsPullFrequencyItem(id))
            {
                GetPullFrequencyItem(id)?.Check();
            }
        }

        public List<string> GetBasePylons()
        {
           return Manager.GetConnectedDevices(TelepowerPylonBuildable.TelepowerPylonTabID);
        }

        public override void RefreshUI()
        {
            ClearPullGrid();
            ClearPushGrid();
            
            foreach (var telePowerPylonBase in BaseTelepowerPylonManager.GetGlobalTelePowerPylonsManagers())
            {
                if (telePowerPylonBase != null && !telePowerPylonBase.HasPylon()) continue;
                AttemptToAddToGrid(telePowerPylonBase);
            }

            QuickLogger.Debug($"WindSurfer Operator Count: {FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(Mod.WindSurferOperatorTabID).Count}");

            foreach (var fcsDeviceKey in FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(Mod.WindSurferOperatorTabID))
            {
                if (fcsDeviceKey.Value is not WindSurferOperatorController pylon) continue;
                AttemptToAddToGrid(pylon.GetPowerManager());
            }
        }

        public void AddItemToPullGrid(BaseTelepowerPylonManager targetController)
        {
            var baseID = targetController.GetBaseID();
            if (string.IsNullOrWhiteSpace(baseID) || IsPullFrequencyItem(targetController.GetBaseID())) return;

            var prefab = Instantiate(ModelPrefab.FrequencyItemPrefab);
            var freqItem = prefab.AddComponent<FrequencyItemController>();
            freqItem.Initialize(targetController, _baseTelepowerPylonManager);


            //Check is the frequencyItem is already in the list
            if (_trackedPullFrequencyItem.ContainsKey(baseID))
            {
                _trackedPullFrequencyItem[baseID] = freqItem;
            }
            else
            {
                _trackedPullFrequencyItem.Add(targetController.GetBaseID(), freqItem);
            }

            prefab.transform.SetParent(_pullGridContent.transform, false);
            QuickLogger.Debug($"Added to Grid {targetController.GetBaseID()} to PULL grid");
        }

        public void AddItemToPushGrid(BaseTelepowerPylonManager targetController)
        {
            var baseID = targetController.GetBaseID();
            if (string.IsNullOrWhiteSpace(baseID) || IsPushFrequencyItem(targetController.GetBaseID())) return;

            var prefab = Instantiate(ModelPrefab.FrequencyItemPrefab);
            var freqItem = prefab.AddComponent<FrequencyItemController>();
            freqItem.Initialize(targetController, _baseTelepowerPylonManager);


            //Check is the frequencyItem is already in the list
            if (_trackedPushFrequencyItem.ContainsKey(baseID))
            {
                _trackedPushFrequencyItem[baseID] = freqItem;
            }
            else
            {
                _trackedPushFrequencyItem.Add(targetController.GetBaseID(), freqItem);
            }

            prefab.transform.SetParent(_pushGridContent.transform, false);
            QuickLogger.Debug($"Added to Grid {targetController.GetBaseID()} to PUSH grid");
        }

        internal bool IsPullFrequencyItem(string id)
        {
            return _trackedPullFrequencyItem.ContainsKey(id.ToLower());
        }

        internal bool IsPushFrequencyItem(string id)
        {
            return _trackedPushFrequencyItem.ContainsKey(id.ToLower());
        }

        public void ClearConnections()
        {
            foreach (var connection in _baseTelepowerPylonManager.GetConnections())
            {
                //DeleteFrequencyItemAndDisconnectRelay(connection.Key);
            }
        }

        public void UnCheckFrequencyItem(string getBaseId)
        {
            if (_trackedPullFrequencyItem.ContainsKey(getBaseId))
            {
                _trackedPullFrequencyItem[getBaseId].UnCheck();
            }


            if (_trackedPushFrequencyItem.ContainsKey(getBaseId))
            {
                _trackedPushFrequencyItem[getBaseId].UnCheck();
            }
        }

        public void CheckFrequencyItem(string getBaseId)
        {
            if (_trackedPullFrequencyItem.ContainsKey(getBaseId))
            {
                _trackedPullFrequencyItem[getBaseId].Check();
            }


            if (_trackedPushFrequencyItem.ContainsKey(getBaseId))
            {
                _trackedPushFrequencyItem[getBaseId].Check();
            }
        }

        internal void UpdateStatus()
        {
            if (_baseTelepowerPylonManager == null || _trackedPullFrequencyItem == null || _trackedPushFrequencyItem == null || _status == null) return;
            
            var upgrade = GetCurrentUpgrade();

            var checkedAmount = _baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PULL ? _trackedPullFrequencyItem.Count(x => x.Value.IsChecked()) : _trackedPushFrequencyItem.Count(x => x.Value.IsChecked());

            _status.text = $"Mark: {upgrade} | Frequency Slots: {checkedAmount}/{_baseTelepowerPylonManager.GetMaxConnectionLimit()}";
        }

        #endregion

        #region Private Methods

        private TelepowerPylonUpgrade GetCurrentUpgrade()
        {
            return _baseTelepowerPylonManager.GetUpgrade();
        }
        
        private float CalculatePowerUsage()
        {
            float amount = 0f;

            if (_baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PUSH)
            {
                foreach (var manager in BaseTelepowerPylonManager.GetGlobalTelePowerPylonsManagers())
                {
                    if (manager.GetIsConnected(_baseTelepowerPylonManager.GetBaseID()))
                    {
                        var distance = WorldHelpers.GetDistance(this, manager.GetRoot());
                        amount += distance * QPatch.Configuration.TelepowerPylonPowerUsagePerMeter;
                    }
                }
            }
            return amount;
        }

        private FrequencyItemController GetPullFrequencyItem(string unitID)
        {
            return _trackedPullFrequencyItem[unitID.ToLower()];
        }

        private FrequencyItemController GetPushFrequencyItem(string unitID)
        {
            return _trackedPushFrequencyItem[unitID.ToLower()];
        }
        
        private void AttemptToAddToGrid(BaseTelepowerPylonManager baseTelepowerPylonManager)
        {
            QuickLogger.Debug($"Attempting to add base {baseTelepowerPylonManager?.GetBaseID()} {UnitID}",true);
            //if (string.IsNullOrWhiteSpace(baseTelepowerPylonManager.GetBaseID()) || string.IsNullOrWhiteSpace(Manager.GetBaseFriendlyId()) || baseTelepowerPylonManager.GetBaseID().Equals(Manager.GetBaseFriendlyId())) return;

            if (baseTelepowerPylonManager == null)
            {
                QuickLogger.Debug($"AttemptToAddToGrid Failed {UnitID}", true);
                return;
            }
            
            QuickLogger.Debug($"AttemptToAddToGrid Passed {UnitID}",true);


            if (baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PULL)
            { 
                AddItemToPushGrid(baseTelepowerPylonManager);
            }

            if (baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PUSH)
            {
                AddItemToPullGrid(baseTelepowerPylonManager);
            }
        }
        
        private void ClearPullGrid()
        {
            foreach (Transform item in _pullGridContent.transform)
            {
                Destroy(item.gameObject);
            }
            _trackedPullFrequencyItem.Clear();
        }

        private void ClearPushGrid()
        {
            foreach (Transform item in _pushGridContent.transform)
            {
                Destroy(item.gameObject);
            }
            _trackedPushFrequencyItem.Clear();
        }
        
        private void DeleteConnection(string id,bool removeCurrentContection = true, bool removeTrackedFrequencyItem = true)
        {
            QuickLogger.Debug($"Attempting to delete current connection {id}",true);

            if (_baseTelepowerPylonManager.GetConnections().ContainsKey(id))
            {
                _baseTelepowerPylonManager.GetConnections().Remove(id);
            }
            else
            {
                QuickLogger.Debug($"Failed to find connection in the list: {id}");
            }
            
            RefreshUI();
        }
        
        private void OnWorldSettled()
        {
            if (_attemptedToLoadConnections) return;

            QuickLogger.Debug("OnWorld Settled",true);
            
            RefreshUI();

            //if (_savedData?.CurrentConnections != null)
            //{

            //    foreach (string connection in _savedData.CurrentConnections)
            //    {
            //        QuickLogger.Debug($"Does Current Connections Contain Key: {connection} = {_baseTelepowerPylonManager.GetConnections().ContainsKey(connection)}");
                    
            //        if (_baseTelepowerPylonManager.GetConnections().ContainsKey(connection)) continue;
                    
            //        var item = _trackedPullFrequencyItem.SingleOrDefault(x => x.Key.Equals(connection));
                    
            //        if (item.Value != null)
            //        {
            //            item.Value.Check(true);
            //        }
            //    }
            //}

            _attemptedToLoadConnections = true;
            _loadingFromSave = false;
        }

        private void ExitDisplay()
        {
            _isInUse = false;
            SNCameraRoot.main.transform.localPosition = Vector3.zero;
            SNCameraRoot.main.transform.localRotation = Quaternion.identity;
            ExitLockedMode();
            _playerBody.SetActive(true);
        }

        private void ExitLockedMode()
        {
            InterceptInput(false);
        }

        private void InterceptInput(bool state)
        {
            if (inputDummy.activeSelf == state)
            {
                return;
            }
            if (state)
            {
                _screenBlock.SetActive(false);
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
        
        internal void ChangeEffectColor(Color color)
        {
            foreach (ParticleSystem system in _particles)
            {
                var main = system.main;
                main.startColor = color;
            }
        }

        #endregion

        #region IConstructable

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            _canvas?.SetActive(constructed);
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

                _isFromConstructed = false;
            }

            QuickLogger.Debug($"Contstructed: {constructed}");
        }

        public override bool CanDeconstruct(out string reason)
        {

            bool result = false;
            reason = string.Empty;
            if (_baseTelepowerPylonManager == null) return true;


            if (_baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PULL)
            {
                if (_baseTelepowerPylonManager.HasConnections())
                {
                    result = true;
                }
            }
            else
            {
                result = CheckIfAnyCheckForPushMode();
            }

            if (result)
            {
                reason = AuxPatchers.RemoveAllTelepowerConnections();
                return false;
            }

            if (_baseTelepowerPylonManager.GetPylonCount() == 1)
            {
                if (_baseTelepowerPylonManager.GetUpgrade() != TelepowerPylonUpgrade.MK1 &&
                    !PlayerInteractionHelper.CanPlayerHold(BaseTelepowerPylonManager.Mk2UpgradeTechType) ||
                    !PlayerInteractionHelper.CanPlayerHold(BaseTelepowerPylonManager.Mk3UpgradeTechType))
                {
                    reason = AlterraHub.InventoryFull();
                    return false;
                }
            }

            _isFromConstructed = true;

            return true;
        }

        private bool CheckIfAnyCheckForPushMode()
        {
            foreach (KeyValuePair<string, FrequencyItemController> controller in _trackedPushFrequencyItem)
            {
                if (controller.Value.IsChecked())
                {
                    return  true;
                }
            }

            return false;
        }

        #endregion

        #region IProtoEventListener

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetTelepowerPylonSaveData(GetPrefabID());
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new TelepowerPylonDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            _savedData.PylonMode = _baseTelepowerPylonManager.GetCurrentMode();
            _savedData.CurrentConnections = GetCurrentConnectionIDs().ToList();
            newSaveData.TelepowerPylonEntries.Add(_savedData);
        }

        private IEnumerable<string> GetCurrentConnectionIDs()
        {
            foreach (var connection in _baseTelepowerPylonManager.GetConnections())
            {
                var item = _trackedPullFrequencyItem.Any(x => x.Key.Equals(connection.Key) && x.Value.IsChecked());

                if(!item) continue;

                yield return connection.Key;
            }
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
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }


        //public void AddPylonToPushGrid (ITelepowerPylonConnection pylon)
        //{
        //    if(_baseTelepowerPylonManager.GetConnections().ContainsKey(pylon.UnitID.ToLower())) return;
        //    ClearPushGrid();
        //    _baseTelepowerPylonManager.GetConnections().Add(pylon.UnitID.ToLower(), pylon);
        //    foreach (KeyValuePair<string, ITelepowerPylonConnection> connection in _baseTelepowerPylonManager.GetConnections())
        //    {
        //        AddItemToPushGrid(pylon);
        //    }
        //}

        #endregion

        #region IHand Target

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange)
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Default);
                return;
            }

            base.OnHandHover(hand);

            if (_isInRange)
            {
                var additionalInformation = Manager == null ? "\nMust be built on platform." : string.Empty;
                var message = hand.IsTool()
                    ? $"Clear Hands to interact with {TelepowerPylonBuildable.TelepowerPylonFriendlyName}"
                    : $"Click to use configure {TelepowerPylonBuildable.TelepowerPylonFriendlyName}";

                _allowedToInteract = !hand.IsTool();

                string additionalText =  string.Empty;
                if (_baseTelepowerPylonManager.GetCurrentMode() == TelepowerPylonMode.PUSH)
                {
                    additionalText = AlterraHub.PowerPerMinute(CalculatePowerUsage() * 60);
                }

                string[] data;
                if (string.IsNullOrWhiteSpace(additionalText))
                {
                    data = new[]
                    {
                        $"Unit ID: {UnitID} {additionalInformation} \n {message}",
                    };
                }
                else
                {
                    data = new[]
                    {
                        $"Unit ID: {UnitID} {additionalInformation} \n {message} |",
                        additionalText
                    };
                }

                data.HandHoverPDAHelperEx(GetTechType(), Manager == null ? HandReticle.IconType.HandDeny : HandReticle.IconType.Info);
            }
        }
        
        public void OnHandClick(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || Manager== null || !_allowedToInteract)
            {
                return;
            }

            if (_isInRange)
            {
                InterceptInput(true);
                _isInUse = true;
                var hudCameraPos = _cameraPosition.transform.position;
                var hudCameraRot = _cameraPosition.transform.rotation;

                Player.main.SetPosition(new Vector3(hudCameraPos.x, Player.main.transform.position.y, hudCameraPos.z), hudCameraRot);
                _playerBody.SetActive(false);
                SNCameraRoot.main.transform.position = hudCameraPos;
                SNCameraRoot.main.transform.rotation = hudCameraRot;

            }
        }
        
        #endregion

        public void GoToPage(TelepowerPylonMode mode)
        {
            if (mode == TelepowerPylonMode.PUSH)
            {
                _pushToggle.isOn = true;
            }

            if (mode == TelepowerPylonMode.PULL)
            {
                _pullToggle.isOn = true;
            }
        }
    }
}
