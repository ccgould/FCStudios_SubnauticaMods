using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.HoverLiftPad.Enums;
using FCS_HomeSolutions.HoverLiftPad.Mono;
using FCS_HomeSolutions.Patches;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.HoverLiftPad.Mono
{
    internal class HoverLiftPadController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private HoverLiftDataEntry _savedData;
        private bool _runStartUpOnEnable;
        private bool _playerLocked;
        private Text _speedMode;
        private List<GateController> _gates;
        private bool _isGatesClosed;
        private Text _statusMessage;
        private Transform _controllerPos;
        private PrawnSuitTrigger _prawnTrigger;
        private Transform _prawnTarget;
        private GameObject _liftPad;
        private const float PadMinHeight = 0.4209284f;
        private int _currentLevelIndex;
        private float _extra = 9f;
        private NameController _nameController;
        private Text _levelIndexLbl;

        internal SpeedModes CurrentSpeedMode = SpeedModes.Min;
        private LevelData _selectedLevel;
        private Exosuit _exosuitOnPad;
        private bool _isPrawnDocked;
        private Button _unLockPrawnButton;
        private Button _lockPlayerButton;
        private Text _distanceTxt1;
        private Text _distanceTxt2;
        private Image _distanceBG1;
        private Image _distanceBG2;
        private bool _isPrawnLocked;
        private static Exosuit[] _globalExosuits;
        private Lift _lift;
        private List<LevelData> _knownLevels = new List<LevelData>();
        private FCSMessageBox _messageBox;
        private Color _orangeColor { get; } = new Color(1f, 0.7176471f, 0f, 1f);

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void FixedUpdate()
        {
            if (!IsConstructed || !IsInitialized) return;

            if (_playerLocked)
            {
                CheckPlayerStatus();

                if (CheckGatesClosed())
                {
                    if (Input.GetKey(QPatch.HoverLiftPadConfiguration.LiftPadUpKeyCode))
                    {
                        _lift.LiftUp();
                    }

                    if (Input.GetKey(QPatch.HoverLiftPadConfiguration.LiftPadDownKeyCode))
                    {
                        _lift.LiftDown();
                    }
                }

                if (Input.GetKeyUp(QPatch.HoverLiftPadConfiguration.LiftPadUpKeyCode) ||
                    Input.GetKeyUp(QPatch.HoverLiftPadConfiguration.LiftPadDownKeyCode))
                {
                    _lift.SetIsMoving(false);
                }
            }


            if (_isPrawnDocked)
            {
                var colors = _unLockPrawnButton.colors;
                colors.normalColor = _orangeColor;
                _unLockPrawnButton.colors = colors;
            }
            else
            {
                var colors = _unLockPrawnButton.colors;
                colors.normalColor = Color.cyan;
                _unLockPrawnButton.colors = colors;
            }

            if (_playerLocked)
            {
                var colors = _lockPlayerButton.colors;
                colors.normalColor = _orangeColor;
                _lockPlayerButton.colors = colors;
            }
            else
            {
                var colors = _lockPlayerButton.colors;
                colors.normalColor = Color.cyan;
                _lockPlayerButton.colors = colors;
            }

            if (_exosuitOnPad)
            {
                UpdateDistance();
            }
        }

        private void UpdateDistance()
        {
            var getDistance = Vector3.Distance(_exosuitOnPad.transform.position, _prawnTrigger.transform.position);
            _distanceTxt1.text = _distanceTxt2.text = $"{getDistance:f1}M";

            if (getDistance < 1)
            {
                _distanceBG1.color = _distanceBG2.color = Color.green;
            }
            else
            {
                _distanceBG1.color = _distanceBG2.color = Color.red;
            }
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.HoverLiftPadTabID, Mod.ModName);

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

                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);

                    if (!string.IsNullOrWhiteSpace(_savedData.DockedPrawnID) && _exosuitOnPad == null)
                    {
                        if (_globalExosuits == null)
                        {
                            _globalExosuits = GameObject.FindObjectsOfType<Exosuit>();
                            QuickLogger.Debug($"Global Exosuit Count: {_globalExosuits.Length}");
                        }
                        
                        foreach (Exosuit exosuit in _globalExosuits)
                        {
                            if (exosuit.gameObject.GetComponentInChildren<PrefabIdentifier>().Id
                                .Equals(_savedData.DockedPrawnID))
                            {
                                QuickLogger.Debug("Setting Exosuit"); 
                                _exosuitOnPad = exosuit;
                            }
                        }
                    }

                    if (_savedData.PadCurrentPosition.ToVector3() != Vector3.zero)
                    {
                        _lift.GoToPosition(_savedData.PadCurrentPosition.ToVector3());
                    }

                    if (_savedData.KnownLevels != null)
                    {
                        _knownLevels = _savedData.KnownLevels;
                    }

                    if (_savedData.FrontGatesOpen)
                    {
                        ToggleGates(Gate.Front);
                    }

                    if (_savedData.BackGatesOpen)
                    {
                        ToggleGates(Gate.Back);
                    }

                    if (_savedData.PlayerLocked)
                    {
                        LockPlayer();
                    }

                    if (_savedData.ExosuitLocked)
                    {
                        QuickLogger.Debug($"Prawn On Pad On Load : {_exosuitOnPad == null}",true);
                        DockPrawn();
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {
            PrawnSuitHandler.RemoveExoSuit(_exosuitOnPad);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetHoverLiftSaveData(GetPrefabID());
        }
        
        private void ButtonAction(UIButtons buttonID)
        {
            if (!IsConstructed || !IsInitialized) return;
            QuickLogger.Debug($"Button Action: {buttonID}", true);
            switch (buttonID)
            {
                case UIButtons.DecreaseSpeed:
                    switch (CurrentSpeedMode)
                    {
                        case SpeedModes.Max:
                            CurrentSpeedMode = SpeedModes.High;
                            break;
                        case SpeedModes.High:
                            CurrentSpeedMode = SpeedModes.Low;
                            break;
                        case SpeedModes.Low:
                            CurrentSpeedMode = SpeedModes.Min;
                            break;
                    }

                    _lift.ChangeSpeed(CurrentSpeedMode);
                    UpdateSpeedModeText();
                    break;

                case UIButtons.IncreaseSpeed:
                    switch (CurrentSpeedMode)
                    {
                        case SpeedModes.High:
                            CurrentSpeedMode = SpeedModes.Max;
                            break;
                        case SpeedModes.Low:
                            CurrentSpeedMode = SpeedModes.High;
                            break;
                        case SpeedModes.Min:
                            CurrentSpeedMode = SpeedModes.Low;
                            break;
                    }
                    _lift.ChangeSpeed(CurrentSpeedMode);
                    UpdateSpeedModeText();
                    break;
                case UIButtons.LockPlayer:
                    LockPlayer();
                    break;
                case UIButtons.UnlockPlayer:
                    UnlockPlayer();
                    break;
                case UIButtons.SaveLevel:
                    _nameController.Show();
                    break;
                case UIButtons.ToggleFrontGate:
                    ToggleGates(Gate.Front);
                    break;
                case UIButtons.ToggleBackGate:
                    ToggleGates(Gate.Back);
                    break;
                case UIButtons.IncreaseLevel:
                    ChangeLevelAssignment(true);
                    break;
                case UIButtons.DecreaseLevel:
                    ChangeLevelAssignment(false);
                    break;
                case UIButtons.GoToFloor:
                    _statusMessage.text = AuxPatchers.GoingToLevelFormat(_selectedLevel.LevelName);
                    _lift.GoToFloor(_selectedLevel.CurrentPosition());
                    break;
                case UIButtons.ToggleDockPrawn:
                    if (_isPrawnDocked)
                    {
                        UnDockPrawn();
                    }
                    else
                    {
                        DockPrawn();
                    }
                    break;
                case UIButtons.Delete:
                    if (_currentLevelIndex == 0)
                    {
                        _messageBox.Show(AuxPatchers.CannotDeleteLevelFormat(_selectedLevel.LevelName),FCSMessageButton.OK,null);
                        return;
                    }

                    _messageBox.Show(AuxPatchers.DeleteLevelConfirmation(_selectedLevel.LevelName),FCSMessageButton.YESNO, DeleteLevel);

                    break;
            }
        }

        private void DeleteLevel(FCSMessageResult result)
        {
            if (result == FCSMessageResult.OKYES)
            {
                _knownLevels.RemoveAt(_currentLevelIndex);
                SelectedLevel(0);
            }
        }

        private void SelectedLevel(int index)
        {
            _statusMessage.text = AuxPatchers.FloorSelect(_knownLevels[index].LevelName);
            _levelIndexLbl.text = (index + 1).ToString();
            _selectedLevel = _knownLevels[index];
        }

        private void ChangeLevelAssignment(bool increase)
        {
            if (increase)
            {
                var newLevel = _currentLevelIndex + 1;
                _currentLevelIndex = newLevel > _knownLevels.Count - 1 ? 0 : newLevel;
            }
            else
            {
                var newLevel = _currentLevelIndex - 1;
                _currentLevelIndex = newLevel < 0 ? _knownLevels.Count - 1 : newLevel;
            }

            SelectedLevel(_currentLevelIndex);
        }

        private void UpdateSpeedModeText()
        {
            switch (CurrentSpeedMode)
            {
                case SpeedModes.Max:
                    _speedMode.text = AuxPatchers.Max();
                    break;
                case SpeedModes.High:
                    _speedMode.text = AuxPatchers.High();
                    break;
                case SpeedModes.Low:
                    _speedMode.text = AuxPatchers.Low();
                    break;
                case SpeedModes.Min:
                    _speedMode.text = AuxPatchers.Min();
                    break;
            }
        }

        private void LockPlayer()
        {
            LockMovement(true);
            Player.main.gameObject.transform.SetParent(_controllerPos);
            Player.main.transform.localPosition = new Vector3(_controllerPos.localPosition.x, _controllerPos.localPosition.y + _extra, _controllerPos.localPosition.z);
            Player.main.gameObject.transform.rotation = _controllerPos.rotation;
            _playerLocked = true;

            if (CheckGatesOpen())
            {
                CloseGates();
            }

            QuickLogger.Debug("Player controls locked", true);
        }

        private void UnlockPlayer()
        {
            if (_lift.goToFloor)
            {
                _lift.Stop();
            }
            LockMovement(false);
            Player.main.gameObject.transform.SetParent(null);
            _playerLocked = false;
            QuickLogger.Debug("Player controls unlocked", true);
        }

        private void LockMovement(bool state)
        {
            FPSInputModule.current.lockMovement = state;
            Player.main.playerController.SetEnabled(!state);
        }

        private void SaveCurrentLevel(string text)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                _knownLevels.Any(x => x.LevelName.Equals(text, StringComparison.OrdinalIgnoreCase))) return;
            _knownLevels.Add(new LevelData { LevelName = text, Position = _liftPad.transform.position.ToVec3() });
        }

        private void CheckPlayerStatus()
        {
            if (FPSInputModule.current.lockMovement != true)
            {
                UnlockPlayer();
            }
        }

        private void ControllerSetup()
        {
            var increaseSpeedButton = GameObjectHelpers.FindGameObject(gameObject, "IncreaseSpeedBTN").GetComponent<Button>();
            increaseSpeedButton.onClick.AddListener((() => { ButtonAction(UIButtons.IncreaseSpeed); }));

            _controllerPos = GameObjectHelpers.FindGameObject(gameObject, "Controller").transform;

            _liftPad = GameObjectHelpers.FindGameObject(gameObject, "HoverLiftPad_Anim");

            _prawnTarget = GameObjectHelpers.FindGameObject(gameObject, "PrawnTarget").transform;
            var prawnTrigger = GameObjectHelpers.FindGameObject(gameObject, "PrawnTrigger");

            _prawnTrigger = prawnTrigger.AddComponent<PrawnSuitTrigger>();
            _prawnTrigger.OnPrawnEntered += (prawn) => { _exosuitOnPad = prawn; };
            _prawnTrigger.OnPrawnExit += (prawn) => { _exosuitOnPad = null; };

            var playerTrigger = GameObjectHelpers.FindGameObject(gameObject, "PlayerTrigger").AddComponent<PlayerTrigger>();
            playerTrigger.OnPlayerExited += UnlockPlayer;

            _speedMode = GameObjectHelpers.FindGameObject(gameObject, "SpeedLBL").GetComponent<Text>();
            _statusMessage = GameObjectHelpers.FindGameObject(gameObject, "Status").GetComponent<Text>();

            var decreaseSpeedButton = GameObjectHelpers.FindGameObject(gameObject, "DecreaseSpeedBTN").GetComponent<Button>();
            decreaseSpeedButton.onClick.AddListener((() => { ButtonAction(UIButtons.DecreaseSpeed); }));

            var deleteButton = GameObjectHelpers.FindGameObject(gameObject, "DeleteBTN").GetComponent<Button>();
            deleteButton.onClick.AddListener((() => { ButtonAction(UIButtons.Delete); }));

            var decreaseLevelBTN = GameObjectHelpers.FindGameObject(gameObject, "DecreaseLevelBTN").GetComponent<Button>();
            decreaseLevelBTN.onClick.AddListener((() => { ButtonAction(UIButtons.DecreaseLevel); }));

            var increaseLevelBTN = GameObjectHelpers.FindGameObject(gameObject, "IncreaseLevelBTN").GetComponent<Button>();
            increaseLevelBTN.onClick.AddListener((() => { ButtonAction(UIButtons.IncreaseLevel); }));

            var toggleFrontGateBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleFrontGateBTN").GetComponent<Button>();
            toggleFrontGateBTN.onClick.AddListener((() => { ButtonAction(UIButtons.ToggleFrontGate); }));

            var toggleBackGateBTN = GameObjectHelpers.FindGameObject(gameObject, "ToggleBackGateBTN").GetComponent<Button>();
            toggleBackGateBTN.onClick.AddListener((() => { ButtonAction(UIButtons.ToggleBackGate); }));

            _unLockPrawnButton = GameObjectHelpers.FindGameObject(gameObject, "UnlockPrawnBTN").GetComponent<Button>();
            _unLockPrawnButton.onClick.AddListener((() => { ButtonAction(UIButtons.ToggleDockPrawn); }));

            _lockPlayerButton = GameObjectHelpers.FindGameObject(gameObject, "LockPlayerBTN").GetComponent<Button>();
            _lockPlayerButton.onClick.AddListener((() =>
            {
                ButtonAction(_playerLocked ? UIButtons.UnlockPlayer : UIButtons.LockPlayer);
            }));

            var saveLevel = GameObjectHelpers.FindGameObject(gameObject, "SaveFloorBTN").GetComponent<Button>();
            saveLevel.onClick.AddListener((() =>
            {
                ButtonAction(UIButtons.SaveLevel);
            }));

            _knownLevels = new List<LevelData>
            {
                new LevelData {IsBase = true,
                    LevelName = "Home",
                    Position = _liftPad.transform.position.ToVec3()
                    }
            };
            _selectedLevel = _knownLevels[0];
            _statusMessage.text = AuxPatchers.FloorSelect("Home");
        }
        
        private void DockPrawn()
        {
            if (_exosuitOnPad != null)
            {
                PrawnSuitHandler.RegisterExoSuit(_exosuitOnPad);
                //LargeWorldStreamer.main.cellManager.UnregisterEntity(_exosuitOnPad.gameObject);
                _exosuitOnPad.gameObject.transform.parent = _liftPad.transform;
                _isPrawnLocked = true;
                _isPrawnDocked = true;
                MovePrawnToPosition();
                QuickLogger.Debug($"Prawn Suit Docked", true);
            }
            else
            {
                QuickLogger.DebugError("No Exosuit on pad", true);
            }
        }

        private void UnDockPrawn()
        {
            if (_exosuitOnPad != null)
            {
                PrawnSuitHandler.RemoveExoSuit(_exosuitOnPad);
                _exosuitOnPad.gameObject.transform.parent = null;
                _isPrawnDocked = false;
                _isPrawnLocked = false;
                QuickLogger.Debug($"Prawn Suit UnDocked", true);
            }
            else
            {
                QuickLogger.DebugError("Exosuit On Pad returned null");
            }
        }

        private void MovePrawnToPosition()
        {
            if(_exosuitOnPad != null && _isPrawnDocked)
            {
                _exosuitOnPad.transform.localPosition = new Vector3(-1.06866f, 2.56f, 1.022756f);
                _exosuitOnPad.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
        }

        private void GatesSetup()
        {
            if (_gates == null)
            {
                _gates = new List<GateController>();
            }

            var frontGates = GameObjectHelpers.FindGameObject(gameObject, "front_gates").transform;
            CreateGate(frontGates.GetChild(0).gameObject, 0, 90.00001f, Gate.Front);
            CreateGate(frontGates.GetChild(1).gameObject, 0, -90.00001f, Gate.Front);

            var backGates = GameObjectHelpers.FindGameObject(gameObject, "back_gates").transform;
            CreateGate(backGates.GetChild(0).gameObject, -15f, 90.00001f, Gate.Back);
            CreateGate(backGates.GetChild(1).gameObject, 15f, -90.00001f, Gate.Back);
        }

        private void CreateGate(GameObject gate, float degrees, float startingDegrees, Gate gateType)
        {
            var controller = gate.AddComponent<GateController>();
            controller.RotateDegrees = degrees;
            controller.GateType = gateType;
            controller.StartRotateDegrees = startingDegrees;
            controller.OnGateStateChanged += ChangeGateStatus;
            _gates.Add(controller);
        }

        private void ChangeGateStatus(bool state)
        {
            if (_isGatesClosed != state)
            {
                _isGatesClosed = state;
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            _statusMessage.text = _isGatesClosed ? string.Empty : AuxPatchers.GatesOpenMessage();
        }

        private void ToggleGates(Gate gateType)
        {
            if (_gates == null)
            {
                QuickLogger.DebugError("Gates are null", true);
                return;
            }
            if (gateType == Gate.Both)
            {
                foreach (GateController gate in _gates)
                {
                    if (gate.IsOpen())
                    {
                        gate.Close();
                    }
                    else
                    {
                        gate.Open();
                    }
                }
            }
            else
            {
                foreach (GateController gate in _gates)
                {
                    if (gate.GateType == gateType)
                    {
                        if (gate.IsOpen())
                        {
                            gate.Close();
                        }
                        else
                        {
                            gate.Open();
                        }
                    }
                }
            }
        }

        private bool CheckGatesOpen()
        {
            return _gates.All(x => x.IsOpen());
        }

        private bool CheckGatesClosed()
        {
            return _gates.All(x => x.IsClosed());
        }

        private void CloseGates()
        {
            foreach (GateController gate in _gates)
            {
                gate.Close();
            }
        }

        public override void Initialize()
        {
            QuickLogger.Debug($"Starting Initialization: {GetPrefabID()}", true);
            GatesSetup();

            ControllerSetup();

            var door1 = GameObjectHelpers.FindGameObject(gameObject, "doors_02_2");
            var door2 = GameObjectHelpers.FindGameObject(gameObject, "doors_02");

            _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox")?.AddComponent<FCSMessageBox>();

            _distanceTxt1 = door1.GetComponentInChildren<Text>();
            _distanceTxt2 = door2.GetComponentInChildren<Text>();


            _distanceBG1 = GameObjectHelpers.FindGameObject(door1, "FrameBG").GetComponent<Image>();
            _distanceBG2 = GameObjectHelpers.FindGameObject(door2, "FrameBG").GetComponent<Image>();


            if (_nameController == null)
            {
                _nameController = gameObject.AddComponent<NameController>();
                _nameController.Initialize(AuxPatchers.Save(), AuxPatchers.SaveLevel());
                _nameController.OnLabelChanged += (text, controller) =>
                {
                    SaveCurrentLevel(text);
                };
            }

            var gotoFloorBtn = GameObjectHelpers.FindGameObject(gameObject, "GotoFloorBTN").GetComponent<Button>() ?? throw new ArgumentNullException("GameObjectHelpers.FindGameObject(gameObject, \"GotoFloorBTN\").GetComponent<Button>()");
            gotoFloorBtn.onClick.AddListener(() =>
            {
                if (_playerLocked)
                {
                    ButtonAction(UIButtons.GoToFloor);
                }
            });

            var callButton = GameObjectHelpers.FindGameObject(gameObject, "CallBTN").GetComponent<Button>();
            _levelIndexLbl = GameObjectHelpers.FindGameObject(gameObject, "LevelIndexLBL").GetComponent<Text>();
            callButton.onClick.AddListener((() =>
            {
                _selectedLevel = _knownLevels.First(x => x.IsBase);
                ButtonAction(UIButtons.GoToFloor);
            }));

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_lift == null)
            {
                _lift = _liftPad.AddComponent<Lift>();
            }

            IsInitialized = true;
            QuickLogger.Debug($"Initialization Complete: {GetPrefabID()} | ID {UnitID}", true);

            InGameMenuSavePatcher.AddEventHandlerIfMissing(OnGameSave);
            InGameMenuClosePatcher.AddEventHandlerIfMissing(OnSaveComplete);
        }

        private void OnSaveComplete()
        {
            if(_exosuitOnPad != null)
            {
                _exosuitOnPad.gameObject.transform.parent = _liftPad.transform;
                QuickLogger.Debug($"Parented {_exosuitOnPad.GetName()} to {UnitID}", true);
            }
        }

        private void OnGameSave()
        {
            if(_exosuitOnPad != null)
            {
                _exosuitOnPad.gameObject.transform.parent = null;
                QuickLogger.Debug($"UnParented {_exosuitOnPad.GetName()} to {UnitID}", true);
            }
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
            if (_exosuitOnPad != null || _playerLocked || _isPrawnDocked)
            {
                reason = AuxPatchers.HoverPadInOperation();
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug($"Construction State Changed: {GetPrefabID()} | State:{constructed}", true);
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

                QuickLogger.Debug($"{UnitID} Constructed", true);
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new HoverLiftDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.PlayerLocked = _playerLocked;
            _savedData.ExosuitLocked = _isPrawnDocked;
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.KnownLevels = _knownLevels;
            _savedData.PadCurrentPosition = _liftPad.transform.position.ToVec3();
            _savedData.FrontGatesOpen = _gates.Any(x => x.GateType == Gate.Front && x.IsOpen());
            _savedData.BackGatesOpen = _gates.Any(x => x.GateType == Gate.Back && x.IsOpen());
            _savedData.DockedPrawnID = _exosuitOnPad?.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id;
            newSaveData.HoverLiftDataEntries.Add(_savedData);
            QuickLogger.Debug($"Saved HoverPad ID {_savedData.Id}", true);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}