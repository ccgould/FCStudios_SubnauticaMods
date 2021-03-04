using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class TelepowerPylonController : FcsDevice,IFCSSave<SaveData>, IHandTarget
    {
        private TelepowerPylonDataEntry _savedData;
        internal bool IsFromSave { get; private set; }
        private bool _runStartUpOnEnable;
        private TelepowerPylonPowerManager _powerManager;
        private GameObject _canvas;
        private NameController _nameController;
        private int _maxConnectionLimit;
        private readonly Dictionary<string, TelepowerPylonController> _currentConnections = new Dictionary<string, TelepowerPylonController>();
        private readonly Dictionary<string, GameObject> _trackedFrequencyItem = new Dictionary<string, GameObject>();

        private GameObject _connectionsGrid;
        private Text _status;
        private readonly TelepowerPylonUpgrade _currentUpgrade = TelepowerPylonUpgrade.MK1;
        private TelepowerPylonMode _mode = TelepowerPylonMode.PUSH;
        private Button _addBTN;
        private bool _attemptedToLoadConnections;
        private Toggle _pullToggle;
        private Toggle _pushToggle;
        private bool _loadingFromSave;
        private PowerRelay _powerRelay;
        private FCSMessageBox _messageBox;
        private const int DEFAULT_CONNECTIONS_LIMIT = 6;
        public override bool IsOperational => Manager != null && IsConstructed;
        public Action<TelepowerPylonController> OnDestroyCalledAction { get; set; }

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.TelepowerPylonTabID, Mod.ModName);
            FCS_AlterraHub.Patches.Player_Update_Patch.OnWorldSettled += OnWorldSettled;
            //InvokeRepeating(nameof(MakeConnection),1f,1f);
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
                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(),ColorTargetMode.Secondary);

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
        
        public override void OnDestroy()
        {
            OnDestroyCalledAction?.Invoke(this);
            FCS_AlterraHub.Patches.Player_Update_Patch.OnWorldSettled -= OnWorldSettled;
        }

        #endregion

        #region Public Methods

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            if (_canvas == null)
            {
                _canvas = GameObjectHelpers.FindGameObject(gameObject,"Canvas");
                _canvas?.SetActive(IsConstructed);
            }

            if (_powerManager == null)
            {
                _powerManager = gameObject.EnsureComponent<TelepowerPylonPowerManager>();
                _powerManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol,AlterraHub.BaseSecondaryCol);
            }

            if (_nameController == null)
            {
                _nameController = gameObject.AddComponent<NameController>();
                _nameController.Initialize("Connect", "Telepower Pylon Search");
                _nameController.SetCurrentName("TP");
                _nameController.OnLabelChanged += OnSearchConnection;
                _nameController.SetMaxChar(20);
            }

            _connectionsGrid = GameObjectHelpers.FindGameObject(gameObject, "Content");

            _status = GameObjectHelpers.FindGameObject(gameObject, "Status")?.GetComponent<Text>();

            _addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN")?.GetComponent<Button>();
            _addBTN.onClick.AddListener(() =>
            {
                if (_currentConnections.Count == _maxConnectionLimit)
                {
                    _messageBox.Show(AuxPatchers.MaximumConnectionsReached(),FCSMessageButton.OK,null);
                    return;
                }
                _nameController.Show();
            });

            _addBTN.interactable = false;

            _maxConnectionLimit = DEFAULT_CONNECTIONS_LIMIT;

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, gameObject, Color.cyan);
            MaterialHelpers.ChangeSpecSettings(AlterraHub.BaseDefaultDecals, AlterraHub.BaseSpec, gameObject, 2.61f, 8f);

            _pushToggle = GameObjectHelpers.FindGameObject(gameObject, "PushToggle")?.GetComponent<Toggle>();
            if (_pushToggle != null)
                _pushToggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        _mode = TelepowerPylonMode.PUSH;
                    }
                    _addBTN.interactable = false;
                }));

            _pullToggle = GameObjectHelpers.FindGameObject(gameObject, "PullToggle")?.GetComponent<Toggle>();
            if (_pullToggle != null)
                _pullToggle.onValueChanged.AddListener((value =>
                {
                    if (value)
                    {
                        _mode = TelepowerPylonMode.PULL;
                    }

                    _addBTN.interactable = true;

                }));

            _powerRelay = gameObject.GetComponentInChildren<PowerRelay>();

            UpdateStatus();

            IsInitialized = true;
        }
        
        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void DeleteFrequencyItemAndDisconnectRelay(FrequencyItemController frequencyItemController)
        {
            DeleteFrequencyItem(frequencyItemController.TargetController.UnitID.ToLower());
        }
        
        public TelepowerPylonMode GetCurrentMode()
        {
            return _mode;
        }

        public IPowerInterface GetPowerRelay()
        {
            return _powerManager?.GetPowerRelay();
        }

        #endregion

        #region Private Methods
        private void OnSearchConnection(string text, NameController arg2)
        {
            var unit = FCSAlterraHubService.PublicAPI.FindDevice(text);
            var idToLower = text.ToLower();
            if (unit.Value == null)
            {
                QuickLogger.Message($"Failed to find pylon with unit ID: {text}", true);
                return;
            }

            var pylon = (TelepowerPylonController) unit.Value;

            if (pylon == null)
            {
                QuickLogger.DebugError("Failed to cast object to Pylon",true);
                return;
            }

            if (pylon.GetCurrentMode() != TelepowerPylonMode.PUSH)
            {
                _messageBox.Show($"Pylon {pylon.UnitID} is not in push mode and cannot be added as a connection.", FCSMessageButton.OK,null);
                return;
            }

            if (_messageBox == null)
            {
                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            }


            if (_currentConnections.ContainsKey(idToLower)) return;

            if (_currentConnections.Count < _maxConnectionLimit && WorldHelpers.CheckIfInRange(this, unit.Value, 1000))
            {
                AddConnection(idToLower, unit.Value);
                _powerManager.AddConnection(pylon);
            }
        }

        private void UpdateStatus()
        {
            var upgrade = GetCurrentUpgrade();

            _status.text = $"Mark: {upgrade} | Frequency Slots: {_currentConnections.Count}/{_maxConnectionLimit}";
        }

        private TelepowerPylonUpgrade GetCurrentUpgrade()
        {
            return _currentUpgrade;
        }
        
        private void AddConnection(string text, FcsDevice unit)
        {
            var controller = (TelepowerPylonController) unit;
            _currentConnections.Add(text.ToLower(), controller);
            AddConnectionItemToGrid(controller);
            controller.OnDestroyCalledAction += OnDestroyCalled;
            UpdateStatus();
        }

        private void OnDestroyCalled(TelepowerPylonController obj)
        {
            DeleteFrequencyItem(obj.UnitID.ToLower());
        }

        private bool AttemptUpgrade()
        {
            return false;
        }
        
        private void AddConnectionItemToGrid(TelepowerPylonController targetController)
        {
            var prefab = Instantiate(ModelPrefab.FrequencyItemPrefab);
            var freqItem = prefab.AddComponent<FrequencyItemController>();
            freqItem.Initialize(targetController,this);
            _trackedFrequencyItem.Add(targetController.UnitID.ToLower(), prefab);
            prefab.transform.SetParent(_connectionsGrid.transform, false);
        }

        private void DeleteFrequencyItem(string id)
        {
            if (!_currentConnections.ContainsKey(id))
            {
                QuickLogger.Debug($"Failed to find connection in the list: {id}");
                return;
            }

            if (_trackedFrequencyItem.ContainsKey(id))
            {
                Destroy(_trackedFrequencyItem[id]);
                _trackedFrequencyItem.Remove(id);
            }
            _currentConnections.Remove(id);
            UpdateStatus();
            _powerManager.RemoveConnection(id);
        }

        private void OnWorldSettled()
        {
            if (_attemptedToLoadConnections) return;
            if (_savedData?.CurrentConnections != null)
            {
                //_loadingFromSave = true;
                foreach (string connection in _savedData.CurrentConnections)
                {
                    OnSearchConnection(connection,null);
                }
                
            }

            _attemptedToLoadConnections = true;
            _loadingFromSave = false;

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
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            if (_powerManager != null && _powerManager.HasConnections())
            {
                reason = AuxPatchers.RemoveAllTelepowerConnections();
                return false;
            }
            
            return true;
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
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            _savedData.PylonMode = GetCurrentMode();
            _savedData.CurrentConnections = GetCurrentConnectionIDs().ToList();
            newSaveData.TelepowerPylonEntries.Add(_savedData);
        }

        private IEnumerable<string> GetCurrentConnectionIDs()
        {
            foreach (KeyValuePair<string, TelepowerPylonController> connection in _currentConnections)
            {
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

        #endregion

        #region IHand Target

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            
            main.SetInteractText($"Unit ID: {UnitID}", $"For more information press {FCS_AlterraHub.QPatch.Configuration.PDAInfoKeyCode}");
            main.SetIcon(HandReticle.IconType.Info);
            
            if (Input.GetKeyDown(FCS_AlterraHub.QPatch.Configuration.PDAInfoKeyCode))
            {

            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        #endregion
    }

    internal enum TelepowerPylonUpgrade
    {
        MK1 = 1,
        MK2 = 2,
        MK3 = 3
    }
}
