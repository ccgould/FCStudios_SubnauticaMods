using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    public class FCSPDAController : MonoBehaviour, IFCSDisplay
    {
        private PDA _pda;
        private int prevQuickSlot;
        public GameObject PDAObj { get; set; }
        private Sequence sequence = new Sequence(false);
        private BaseManager _currentBase;
        private GameObject _inputDummy;
        private uGUI_InputGroup _ui;
        private Text _clock;
        private bool _isBeingDestroyed;
        private List<DSSInventoryItem> _inventoryButtons = new List<DSSInventoryItem>();
        private List<DSSBaseItem> _baseButtons = new List<DSSBaseItem>();
        private GridHelperV2 _inventoryGrid;
        private GridHelperV2 _basesGrid;
        private GameObject _home;
        private GameObject _bases;
        private GameObject _baseInventory;
        private Text _baseNameLBL;
        private Text _currentBiome;
        private Text _accountName;
        private Text _accountBalance;
        private bool _goToEncyclopedia;
        //private GameObject _missionPage;
        //private GameObject _messagesPage;
        private bool _addtempMessage = true;
        private PaginatorController _inventoryPaginatorController;
        private PaginatorController _basePaginatorController;
        private bool _depthState;

        public float cameraFieldOfView = 62f;
        public float cameraFieldOfViewAtFourThree = 66f;
        public Canvas PdaCanvas { get; set; }
        internal bool IsOpen { get; private set; }
        public Action OnClose { get; set; }
        public Channel AudioTrack { get; set; }
        public bool isFocused => this.ui != null && this.ui.focused;

        #region SINGLETON PATTERN
        private static FCSPDAController _instance;
        private GameObject _operationsScrollView;
        private Transform _operationsScrollViewTrans;
        private GameObject _itemTransferOperations;
        private GameObject _addNewOperation;
        private InputField _deviceIdInput;
        private FcsDevice _selectedDevice;
        private Toggle _isPullOperationToggle;
        private TechType _selectedTransferItem;
        private Text _itemName;
        private BaseManager _manager;
        private bool _fromTransferDialog;

        public static FCSPDAController Instance => _instance;



        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    
    #endregion


    public uGUI_InputGroup ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = gameObject.GetComponentInChildren<Canvas>(true).gameObject.AddComponent<uGUI_InputGroup>();
                }
                return _ui;
            }
        }
        
        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        private void Update()
        {
            sequence.Update();
            if (sequence.active)
            {
                float b = (SNCameraRoot.main.mainCamera.aspect > 1.5f) ? cameraFieldOfView : cameraFieldOfViewAtFourThree;
                SNCameraRoot.main.SetFov(Mathf.Lerp(MiscSettings.fieldOfView, b, sequence.t));
            }
            
            if (!ui.selected && IsOpen && AvatarInputHandler.main.IsEnabled())
            {
                ui.Select(false);
            }

            if (IsOpen && this.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) || Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode)))
            {
                this.Close();
                return;
            }

            if (_clock != null)
            {
                _clock.text = WorldHelpers.GetGameTimeFormat();
            }

            FPSInputModule.current.EscapeMenu();
        }

        private void Start()
        {
            _home = GameObjectHelpers.FindGameObject(gameObject, "Home");
            //_missionPage = GameObjectHelpers.FindGameObject(gameObject, "Missions");
            
            _bases = GameObjectHelpers.FindGameObject(gameObject, "Bases");
            _baseInventory = GameObjectHelpers.FindGameObject(gameObject, "BasesInventory");
            _baseNameLBL = GameObjectHelpers.FindGameObject(gameObject, "BaseNameLBL")?.GetComponent<Text>();
            _currentBiome = GameObjectHelpers.FindGameObject(gameObject, "BiomeLBL")?.GetComponent<Text>();
            _accountName = GameObjectHelpers.FindGameObject(gameObject, "UserName")?.GetComponent<Text>();
            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance")?.GetComponent<Text>();
            _addNewOperation = GameObjectHelpers.FindGameObject(gameObject, "AddNewOperation");
            _itemTransferOperations = GameObjectHelpers.FindGameObject(gameObject, "ItemTransferOperations");
            _deviceIdInput = GameObjectHelpers.FindGameObject(gameObject, "DeviceIdInput")?.GetComponent<InputField>();
            _operationsScrollView = GameObjectHelpers.FindGameObject(gameObject, "OperationsContent");
            _operationsScrollViewTrans = _operationsScrollView?.transform;
            _itemName = GameObjectHelpers.FindGameObject(gameObject, "ItemName")?.GetComponent<Text>();
            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock")?.GetComponent<Text>();
            _basePaginatorController = GameObjectHelpers.FindGameObject(gameObject, "BasePaginator")?.AddComponent<PaginatorController>();
            _basePaginatorController?.Initialize(this);

            _isPullOperationToggle = GameObjectHelpers.FindGameObject(gameObject, "IsPullOperationToggle")?.GetComponent<Toggle>();

            _inventoryPaginatorController = GameObjectHelpers.FindGameObject(gameObject, "InventoryPaginator")?.AddComponent<PaginatorController>();
            _inventoryPaginatorController?.Initialize(this);

            _addtempMessage = false;

            _basesGrid = gameObject.AddComponent<GridHelperV2>();
            if (_basesGrid != null)
            {
                _basesGrid.OnLoadDisplay += OnLoadBasesGrid;
                _basesGrid.Setup(10, gameObject, Color.gray, Color.white, OnButtonClick);
            }
            
            _inventoryGrid = gameObject.AddComponent<GridHelperV2>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(15, gameObject, Color.gray, Color.white, OnButtonClick);

            var vpb = GameObjectHelpers.FindGameObject(gameObject, "Progress")?.AddComponent<VideoProgressBar>();
            if(vpb != null)
            {
                vpb.VideoPlayer = gameObject.GetComponentInChildren<VideoPlayer>();
                var stopButton = GameObjectHelpers.FindGameObject(gameObject, "StopButton")?.GetComponent<Button>();
                if(stopButton != null)
                    stopButton.onClick.AddListener((() => { vpb.Stop(); }));
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();

            CreateBaseInventoryPage();
            
            CreateBasePage();

            CreateHomePage();

            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject,
                Color.cyan);
            
            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
        }

        private void CreateHomePage()
        {
            var baseBTNObj = GameObjectHelpers.FindGameObject(gameObject, "BasesButton");
            var baseBTNToolTip = baseBTNObj.AddComponent<FCSToolTip>();
            baseBTNToolTip.RequestPermission += () => true;
            baseBTNToolTip.Tooltip = "Allows you to pull items from online bases requires connection chip";
            var basesButton = baseBTNObj.GetComponent<Button>();
            basesButton.onClick.AddListener(() =>
            {
                _bases.SetActive(true);
                _home.SetActive(false);
                _baseInventory.SetActive(false);
            });

            var encyclopObj = GameObjectHelpers.FindGameObject(gameObject, "EncyclopediaButton");
            var encyclopToolTip = encyclopObj.AddComponent<FCSToolTip>();
            encyclopToolTip.Tooltip = "Opens the PDA to the encyclopedia tab";
            encyclopToolTip.RequestPermission += () => true;
            var encyclopediaButton = encyclopObj.GetComponent<Button>();
            encyclopediaButton.onClick.AddListener(() =>
            {
                _goToEncyclopedia = true;
                Close();
            });

            var settingsButton = GameObjectHelpers.FindGameObject(gameObject, "SettingsButton").GetComponent<Button>();
            settingsButton.onClick.AddListener(() => { QuickLogger.ModMessage("Page Not Implemented"); });

            var messageBTNObj = GameObjectHelpers.FindGameObject(gameObject, "MessagesButton");
            var messagesButton = messageBTNObj.GetComponent<Button>();
            var messageBTNToolTip = messageBTNObj.AddComponent<FCSToolTip>();
            messageBTNToolTip.Tooltip = "Disabled until further notice!";
            messageBTNToolTip.RequestPermission += () => true;
            //messagesButton.onClick.AddListener(() =>
            //{
            //    _home.SetActive(false);
            //    _messagesPage.SetActive(true);
            //});

            var contactsButton = GameObjectHelpers.FindGameObject(gameObject, "ContactsButton").GetComponent<Button>();
            contactsButton.onClick.AddListener(() => { QuickLogger.ModMessage("Page Not Implemented"); });

            //var messagesBackBTN = GameObjectHelpers.FindGameObject(gameObject, "MessagesBackBTN").GetComponent<Button>();
            //messagesBackBTN.onClick.AddListener(() =>
            //{
            //    _home.SetActive(true);
            //    _messagesPage.SetActive(false);
            //});

            //var missionsBackBTN = GameObjectHelpers.FindGameObject(gameObject, "MissionsBackBTN").GetComponent<Button>();
            //missionsBackBTN.onClick.AddListener(() =>
            //{
            //    _home.SetActive(true);
            //    _missionPage.SetActive(false);
            //});

            var missionBTNObj = GameObjectHelpers.FindGameObject(gameObject, "MissionsButton");
            var missionsButton = missionBTNObj.GetComponent<Button>();
            var missionBTNToolTip = missionBTNObj.AddComponent<FCSToolTip>();
            missionBTNToolTip.Tooltip = "Disabled until further notice!";
            missionBTNToolTip.RequestPermission += () => true;
            //missionsButton.onClick.AddListener(() =>
            //{
            //    _home.SetActive(false);
            //    _missionPage.SetActive(true);
            //});
        }

        private void CreateBasePage()
        {
            foreach (Transform baseItem in GameObjectHelpers.FindGameObject(_bases, "Grid").transform)
            {
                var baseButton = baseItem.gameObject.EnsureComponent<DSSBaseItem>();
                baseButton.ButtonMode = InterfaceButtonMode.HoverImage;
                baseButton.BtnName = "BaseBTN";
                baseButton.OnButtonClick += OnButtonClick;
                _baseButtons.Add(baseButton);
            }

            var basesBackButton = GameObjectHelpers.FindGameObject(_bases, "ReturnBTN").GetComponent<Button>();
            basesBackButton.onClick.AddListener(() =>
            {
                _bases.SetActive(false);
                _home.SetActive(true);
                _baseInventory.SetActive(false);
            });
        }

        private void CreateBaseInventoryPage()
        {
            foreach (Transform invItem in GameObjectHelpers.FindGameObject(_baseInventory, "Grid").transform)
            {
                var invButton = invItem.gameObject.EnsureComponent<DSSInventoryItem>();
                invButton.ButtonMode = InterfaceButtonMode.HoverImage;
                invButton.BtnName = "InventoryBTN";
                invButton.OnButtonClick += OnButtonClick;
                _inventoryButtons.Add(invButton);
            }

            var baseInventoryBackButton = GameObjectHelpers.FindGameObject(_baseInventory, "ReturnBTN").GetComponent<Button>();
            baseInventoryBackButton.onClick.AddListener(() =>
            {
                _bases.SetActive(true);
                _home.SetActive(false);
                _baseInventory.SetActive(false);
                UpdateDisplay();
            });

            var homeButton = GameObjectHelpers.FindGameObject(_baseInventory, "HomeBTN").GetComponent<Button>();
            homeButton.onClick.AddListener(() =>
            {
                _bases.SetActive(false);
                _home.SetActive(true);
                _baseInventory.SetActive(false);
            });
        }

        private bool ValidateInformation()
        {
            var device = FCSAlterraHubService.PublicAPI.FindDevice(_deviceIdInput.text);

            if (device.Value == null)
            {
                QuickLogger.Message($"Invalid Device: Device with id {_deviceIdInput.text} not found on base.");
                return false;
            }

            _selectedDevice = device.Value;

            if (_isPullOperationToggle.isOn && _selectedTransferItem == TechType.None)
            {
                QuickLogger.Message($"Please choose which item to send");
                return false;
            }

            return true;
        }
        
        private void UpdateDisplay()
        {
            if (_inventoryGrid == null || _basesGrid == null || _currentBiome == null || _accountName == null) return;
            _basesGrid.DrawPage();
            _inventoryGrid.DrawPage();
            _currentBiome.text = Player.main.GetBiomeString();
            _accountName.text = CardSystem.main.GetUserName();
            _accountBalance.text = $"{CardSystem.main.GetAccountBalance():N0}";
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "InventoryBTN":
                    var techType = (TechType) arg2;
                    if (PlayerInteractionHelper.CanPlayerHold(techType))
                    {
                        var pickup = _currentBase?.TakeItem(techType);
                        PlayerInteractionHelper.GivePlayerItem(pickup);
                        UpdateDisplay();
                    }
                    else
                    {
                        QuickLogger.ModMessage(Buildables.AlterraHub.InventoryFull());
                    }
                    break;

                case "BaseBTN":
                    _bases.SetActive(false);
                    _home.SetActive(false);
                    _baseInventory.SetActive(true);
                    _currentBase = (BaseManager) arg2;
                    _baseNameLBL.text = _currentBase.GetBaseName();
                    UpdateDisplay();
                    break;
            }
        }

        internal void OnEnable()
        {
            QuickLogger.Debug($"FCS PDA: Active and Enabled {isActiveAndEnabled}",true);
        }

        public void Open()
        {
            FindPDA();
            _depthState = UwePostProcessingManager.GetDofEnabled();

            UwePostProcessingManager.ToggleDof(false);

            _pda.isInUse = true;
            uGUI.main.quickSlots.SetTarget(null);
            prevQuickSlot = Inventory.main.quickSlots.activeSlot;
            bool flag = Inventory.main.ReturnHeld();
            Player main = Player.main;
            if (!flag || main.cinematicModeActive)
            {
                return;
            }
            
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            IsOpen = true;
            gameObject.SetActive(true);
            sequence.Set(0.5f, true, Activated);
            UWE.Utils.lockCursor = false;
            if (HandReticle.main != null)
            {
                HandReticle.main.RequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(false);
            UwePostProcessingManager.OpenPDA();
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", true);
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);
            if (_pda.screen.activeSelf)
            {
                _pda.screen.SetActive(false);
            }
            QuickLogger.Debug("FCS PDA Is Open", true);
        }
        
        public void Close()
        {
            IsOpen = false;
            _pda.isInUse = false;
            Player main = Player.main;
            MainCameraControl.main.ResetLockedVRViewModelAngle();
            Vehicle vehicle = main.GetVehicle();
            if (vehicle != null)
            {
                uGUI.main.quickSlots.SetTarget(vehicle);
            }

#if SUBNAUTICA_STABLE
            MainGameController.Instance.PerformGarbageAndAssetCollection();
#else
            MainGameController.Instance.PerformIncrementalGarbageCollection();
#endif
            if (HandReticle.main != null)
            {
                HandReticle.main.UnrequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(true);
            sequence.Set(0.5f, false, Deactivated);
            
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", false);
            ui.Deselect(null);
            UwePostProcessingManager.ClosePDA();
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundClose);
            UwePostProcessingManager.ToggleDof(_depthState);
            _itemTransferOperations?.SetActive(false);

            if (_fromTransferDialog)
            {
                _home.SetActive(true);
                _fromTransferDialog = false;
            }
            QuickLogger.Debug("FCS PDA Is Closed", true);
        }

        public void Activated()
        {
            ui.Select();
        }

        public void Deactivated()
        {
            SNCameraRoot.main.SetFov(0f);
            OnClose?.Invoke();
            gameObject.SetActive(false);

            if (!_goToEncyclopedia)
            {
                Inventory.main.quickSlots.Select(prevQuickSlot);
            }
            else
            {
                Player.main.GetPDA().Open(PDATab.Encyclopedia);
                _goToEncyclopedia = false;
            }
        }
        
        private void FindPDA()
        {
            QuickLogger.Debug("In Find PDA");
            if (PdaCanvas == null)
            {
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
                Player main = Player.main;
                _pda = main.GetPDA();
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _currentBase == null) return;

                var grouped = _currentBase.GetItemsWithin().OrderBy(x => x.Key).ToList();

                //if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                //}

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                var g = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[g++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

                _inventoryGrid.UpdaterPaginator(grouped.Count);
                _inventoryPaginatorController.ResetCount(_inventoryGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadBasesGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed|| _baseButtons?.Count <=0) return;

                var grouped = BaseManager.Managers.Where(x => x != null && !x.GetBaseName().Equals("Cyclops 0")).ToList();

                //if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                //}

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _baseButtons[i].Reset();
                }

                var g = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {

                    _baseButtons[g++].Set(grouped[i]);
                }

                _basesGrid.UpdaterPaginator(grouped.Count);
                _basePaginatorController.ResetCount(_basesGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void GoToPage(int index)
        {
 
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            if (sender == _inventoryPaginatorController)
            {
                _inventoryGrid.DrawPage(index);
            }
            else if(sender == _basePaginatorController)
            {
                QuickLogger.Debug($"Refreshing Base Grid going to page {index} | Controller {sender.gameObject.name}",true);
                _basesGrid.DrawPage(index);
            }
        }

    }
}